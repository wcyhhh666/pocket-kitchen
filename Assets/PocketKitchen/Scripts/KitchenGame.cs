using System.Collections.Generic;
using UnityEngine;

namespace PocketKitchen
{
    public class KitchenGame : MonoBehaviour
    {
        public static KitchenGame Instance { get; private set; }
        public ChefController chef;
        public KitchenStation[] Stations;
        public List<KitchenOrder> Orders = new List<KitchenOrder>();
        public GamePhase Phase { get; private set; } = GamePhase.Menu;
        public float Remaining { get; private set; } = 180;
        public int Score { get; private set; }
        public int Completed { get; private set; }
        public int Missed { get; private set; }
        public int Streak { get; private set; }
        public int Best { get; private set; }
        public bool VerificationMode { get; set; }
        public string Toast { get; private set; }
        public float ToastUntil { get; private set; }
        public bool ToastError { get; private set; }
        private int orderNumber;
        private int seed;
        private AudioSource audioSource;
        private AudioClip[] tones;

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            Best = PlayerPrefs.GetInt("PocketKitchen.Best", 0);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0;
            tones = new[] { MakeTone(520, 0.08f), MakeTone(300, 0.055f), MakeTone(880, 0.18f), MakeTone(180, 0.13f) };
        }

        private void Start()
        {
            bool smoke = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--smoke-test") >= 0;
#if UNITY_EDITOR
            smoke |= UnityEditor.SessionState.GetBool("PocketKitchen.RunSmoke", false);
            UnityEditor.SessionState.SetBool("PocketKitchen.RunSmoke", false);
#endif
            if (smoke) gameObject.AddComponent<KitchenSmokeTest>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
            if ((Phase == GamePhase.Menu || Phase == GamePhase.Finished) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R))) StartRound();
            AdvanceTime(Time.deltaTime);
        }

        public void StartRound()
        {
            Remaining = 180; Score = 0; Completed = 0; Missed = 0; Streak = 0; orderNumber = 0; seed = 0;
            Orders.Clear();
            foreach (var station in Stations) station.ResetStation();
            chef.ResetChef();
            Phase = GamePhase.Playing;
            AddOrder(KitchenItem.Salad); AddOrder(KitchenItem.Soup); AddOrder(KitchenItem.Salad);
            Notify("开店啦！先试一份沙拉：取生菜 → 切菜 → 装盘 → 出餐", false);
            Sound(2);
        }

        private void AddOrder(KitchenItem dish)
        {
            Orders.Add(new KitchenOrder { dish = dish, number = ++orderNumber, remaining = 75, duration = 75 });
        }

        public void AdvanceTime(float dt)
        {
            if (Phase != GamePhase.Playing || dt <= 0) return;
            Remaining = Mathf.Max(0, Remaining - dt);
            if (Remaining <= 0) { Finish(); return; }
            foreach (var station in Stations) station.Tick(dt);
            for (int i = Orders.Count - 1; i >= 0; i--)
            {
                Orders[i].remaining -= dt;
                if (Orders[i].remaining <= 0)
                {
                    Orders.RemoveAt(i); Missed++; Streak = 0;
                    Notify("一份订单超时了，别着急，继续准备下一份", true);
                }
            }
            while (Orders.Count < 3) AddOrder((seed++ % 3 == 0) ? KitchenItem.Salad : KitchenItem.Soup);
        }

        public bool Deliver(ChefController actor)
        {
            if (Phase != GamePhase.Playing) return false;
            KitchenOrder match = Orders.Find(order => order.dish == actor.Held);
            if (match == null || (actor.Held != KitchenItem.Salad && actor.Held != KitchenItem.Soup))
            {
                Notify("请按左侧订单准备成品：沙拉或番茄浓汤", true);
                return false;
            }
            Streak++; Completed++;
            int reward = (match.dish == KitchenItem.Soup ? 120 : 80) + Mathf.FloorToInt(match.remaining / match.duration * 20) + Mathf.Min(40, (Streak - 1) * 10);
            Score += reward;
            Orders.Remove(match);
            actor.SetHeld(KitchenItem.None);
            AddOrder((seed++ % 3 == 0) ? KitchenItem.Salad : KitchenItem.Soup);
            Notify("出餐成功  +" + reward + " 分" + (Streak > 1 ? "   连击 ×" + Streak : ""), false);
            Sound(2);
            return true;
        }

        public void TogglePause()
        {
            if (Phase == GamePhase.Playing) Phase = GamePhase.Paused;
            else if (Phase == GamePhase.Paused) Phase = GamePhase.Playing;
        }

        public void Finish()
        {
            Phase = GamePhase.Finished;
            if (!VerificationMode && Score > Best) { Best = Score; PlayerPrefs.SetInt("PocketKitchen.Best", Best); PlayerPrefs.Save(); }
        }

        public void Notify(string message, bool error)
        {
            Toast = message; ToastUntil = Time.unscaledTime + 4.5f; ToastError = error;
            if (error) Sound(3);
        }

        public void Sound(int kind)
        {
            if (audioSource != null && tones != null) audioSource.PlayOneShot(tones[Mathf.Clamp(kind, 0, tones.Length - 1)], 0.22f);
        }

        private static AudioClip MakeTone(float frequency, float duration)
        {
            int count = Mathf.RoundToInt(22050 * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / 22050f;
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * Mathf.Sin(Mathf.PI * i / count) * 0.4f;
            }
            AudioClip clip = AudioClip.Create("KitchenTone", count, 1, 22050, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

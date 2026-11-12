using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PocketKitchen
{
    public class KitchenSmokeTest : MonoBehaviour
    {
        [Serializable] private class Evidence
        {
            public string status;
            public string unityVersion;
            public string checkedAt;
            public string failure;
            public List<string> passed = new List<string>();
            public List<string> runtimeErrors = new List<string>();
        }
        private Evidence evidence = new Evidence();
        private string output;

        private IEnumerator Start()
        {
            output = Argument("--qa-dir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../Outputs/qa"));
            Directory.CreateDirectory(output);
            evidence.unityVersion = Application.unityVersion;
            evidence.checkedAt = DateTimeOffset.Now.ToString("o");
            Application.logMessageReceived += CaptureError;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "PocketKitchen-Menu.png"));
            yield return new WaitForSecondsRealtime(0.5f);
            try { ValidateGameplay(); evidence.status = "passed"; }
            catch (Exception exception) { evidence.status = "failed"; evidence.failure = exception.ToString(); Debug.LogException(exception); }
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "PocketKitchen-Gameplay.png"));
            yield return new WaitForSecondsRealtime(0.7f);
            Application.logMessageReceived -= CaptureError;
            if (evidence.runtimeErrors.Count > 0) evidence.status = "failed";
            File.WriteAllText(Path.Combine(output, "gameplay-smoke-test.json"), JsonUtility.ToJson(evidence, true));
            Debug.Log("[PocketKitchen QA] " + evidence.status + ": " + evidence.passed.Count + " checks. " + output);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--quit-after-test") >= 0)
                Application.Quit(evidence.status == "passed" ? 0 : 1);
        }

        private void CaptureError(string condition, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
                evidence.runtimeErrors.Add(condition);
        }

        private void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("Failed: " + name);
            evidence.passed.Add(name);
        }

        private KitchenStation Station(StationKind kind) => Array.Find(KitchenGame.Instance.Stations, station => station.kind == kind);

        private void ValidateGameplay()
        {
            var game = KitchenGame.Instance;
            game.VerificationMode = true;
            var chef = game.chef;
            var tomato = Station(StationKind.Tomatoes);
            var lettuce = Station(StationKind.Lettuce);
            var plates = Station(StationKind.Plates);
            var board = Station(StationKind.Chopping);
            var stove = Station(StationKind.Stove);
            var trash = Station(StationKind.Trash);
            Check(game.Stations.Length == 7, "Seven configured kitchen stations");
            Check(chef.Motor != null && Camera.main != null, "Playable character and camera exist");
            game.StartRound();
            Check(game.Phase == GamePhase.Playing && game.Orders.Count == 3, "Round starts with three orders");
            Check(game.Remaining == 180 && game.Score == 0, "Timer and score initialize");
            Check(KitchenHUD.FormatRemaining(179.9f) == "03:00", "Timer rounds fractional minute without displaying 02:60");
            Check(KitchenHUD.FormatRemaining(120.1f) == "02:01", "Timer formats minute boundary correctly");
            tomato.Interact(chef);
            Check(chef.Held == KitchenItem.Tomato, "Tomato source provides raw tomato");
            plates.Interact(chef);
            Check(chef.Held == KitchenItem.Tomato, "Full hands cannot take another item");
            stove.Interact(chef);
            Check(stove.Surface == KitchenItem.None && chef.Held == KitchenItem.Tomato, "Raw tomato cannot bypass chopping");
            Check(!game.Deliver(chef) && game.Score == 0 && chef.Held == KitchenItem.Tomato, "Raw ingredient cannot score or disappear at delivery");
            trash.Interact(chef);
            Check(chef.Held == KitchenItem.None, "Recycling frees occupied hands");
            lettuce.Interact(chef); board.Interact(chef);
            Check(chef.Held == KitchenItem.None && board.Surface == KitchenItem.Lettuce, "Ingredient transfers from hands to cutting board");
            board.Work(); board.Work();
            Check(board.Surface == KitchenItem.Lettuce && board.Cuts == 2, "Two cuts do not finish preparation");
            board.Work();
            Check(board.Surface == KitchenItem.ChoppedLettuce, "Third cut completes lettuce");
            plates.Interact(chef); board.Interact(chef);
            Check(chef.Held == KitchenItem.Salad && board.Surface == KitchenItem.None, "Empty plate assembles salad and consumes prepared lettuce");
            Check(game.Deliver(chef) && game.Score > 0 && game.Completed == 1, "Salad fulfills matching order");
            int saladScore = game.Score;
            Check(!game.Deliver(chef) && game.Score == saladScore, "Repeated empty delivery cannot duplicate score");
            tomato.Interact(chef); board.Interact(chef); board.Work(); board.Work(); board.Work(); board.Interact(chef);
            Check(chef.Held == KitchenItem.ChoppedTomato, "Prepared tomato can be picked up");
            stove.Interact(chef);
            Check(chef.Held == KitchenItem.None && stove.Surface == KitchenItem.ChoppedTomato, "Prepared tomato starts soup");
            stove.Tick(4.9f);
            Check(!stove.SoupReady, "Soup cannot finish before five seconds");
            stove.Tick(0.11f);
            Check(stove.SoupReady, "Soup becomes ready after cooking duration");
            stove.Interact(chef);
            Check(chef.Held == KitchenItem.None && stove.SoupReady, "Soup requires a plate and survives invalid pickup");
            plates.Interact(chef); stove.Interact(chef);
            Check(chef.Held == KitchenItem.Soup && stove.Surface == KitchenItem.None, "Plate serves soup and empties pot");
            Check(game.Deliver(chef) && game.Completed == 2 && game.Streak == 2 && game.Score > saladScore, "Soup scores and builds delivery streak");
            float beforePause = game.Remaining;
            game.TogglePause(); game.AdvanceTime(20);
            Check(game.Phase == GamePhase.Paused && game.Remaining == beforePause, "Paused round does not consume time");
            lettuce.Interact(chef);
            Check(chef.Held == KitchenItem.None, "Paused game rejects station interaction");
            game.TogglePause();
            game.Orders[0].remaining = 0.1f; game.AdvanceTime(0.2f);
            Check(game.Missed == 1 && game.Streak == 0 && game.Orders.Count == 3, "Expired order resets streak and is replaced");
            Vector3 counter = board.transform.position;
            chef.Teleport(new Vector3(counter.x + 2, 0.1f, counter.z));
            chef.Motor.Move(Vector3.left * 3);
            Check(chef.transform.position.x > counter.x + 0.95f, "Character collision prevents walking through counter");
            game.AdvanceTime(181);
            Check(game.Phase == GamePhase.Finished && game.Remaining == 0, "Round ends when timer reaches zero");
            game.StartRound();
            Check(game.Score == 0 && game.Completed == 0 && game.Missed == 0 && chef.Held == KitchenItem.None, "Restart clears gameplay state");
            Check(game.Orders.Count == 3 && game.Remaining == 180 && stove.Surface == KitchenItem.None && board.Surface == KitchenItem.None, "Restart restores timer, orders and stations");
            Check(chef.transform.position.z < -2 && Mathf.Abs(chef.transform.position.x) < 0.1f, "Restart returns chef to spawn");
            game.VerificationMode = false;
            game.Notify("欢迎营业！左侧是订单，右侧是配方。先试一份田园沙拉吧。", false);
        }

        private static string Argument(string key)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, key);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}

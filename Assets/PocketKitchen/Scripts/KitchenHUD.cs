using UnityEngine;

namespace PocketKitchen
{
    public class KitchenHUD : MonoBehaviour
    {
        private Font font;
        private Texture2D round;
        private Texture2D circle;
        private GUIStyle panel;
        private GUIStyle label;
        private GUIStyle hit;
        private readonly Color ink = new Color(0.08f, 0.19f, 0.20f);
        private readonly Color muted = new Color(0.43f, 0.53f, 0.51f);
        private readonly Color paper = new Color(0.98f, 0.97f, 0.92f);
        private readonly Color mint = new Color(0.29f, 0.70f, 0.55f);
        private readonly Color gold = new Color(1, 0.78f, 0.34f);

        private void Init()
        {
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 24);
            round = Rounded(48, 10); circle = Rounded(48, 24);
            panel = new GUIStyle(); panel.normal.background = round; panel.border = new RectOffset(12, 12, 12, 12);
            label = new GUIStyle { font = font, wordWrap = true, richText = false };
            hit = new GUIStyle();
        }

        private static Texture2D Rounded(int size, int radius)
        {
            Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x - 0.5f, x - (size - radius) + 0.5f, 0);
                float dy = Mathf.Max(radius - y - 0.5f, y - (size - radius) + 0.5f, 0);
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy)));
            }
            t.SetPixels(pixels); t.Apply(); return t;
        }

        private void P(Rect rect, Color color)
        {
            if (rect.width <= 0 || rect.height <= 0) return;
            GUI.color = color;
            if (rect.height < 24 || rect.width < 24) GUI.DrawTexture(rect, Texture2D.whiteTexture);
            else GUI.Box(rect, GUIContent.none, panel);
            GUI.color = Color.white;
        }

        private void L(Rect rect, string text, int size, Color color, bool bold = false, TextAnchor align = TextAnchor.UpperLeft)
        {
            label.fontSize = size; label.normal.textColor = color; label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; label.alignment = align;
            GUI.Label(rect, text, label);
        }

        private bool B(Rect rect, string text, bool secondary = false)
        {
            bool hover = rect.Contains(Event.current.mousePosition);
            Color color = secondary ? new Color(0.86f, 0.90f, 0.84f) : gold;
            P(rect, hover ? Color.Lerp(color, Color.white, 0.15f) : color);
            L(rect, text, 21, ink, true, TextAnchor.MiddleCenter);
            return GUI.Button(rect, GUIContent.none, hit);
        }

        private void OnGUI()
        {
            var game = KitchenGame.Instance;
            if (game == null) return;
            if (panel == null) Init();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            if (game.Phase == GamePhase.Menu) { Menu(game); return; }
            Hud(game);
            if (game.Phase == GamePhase.Paused) Pause(game);
            if (game.Phase == GamePhase.Finished) End(game);
        }

        private void Menu(KitchenGame game)
        {
            P(new Rect(-200, -200, 1680, 1120), new Color(0.04f, 0.13f, 0.14f, 0.76f));
            L(new Rect(82, 78, 600, 32), "POCKET KITCHEN    /    FRESHLY MADE", 19, mint, true);
            L(new Rect(78, 174, 650, 110), "口袋小厨", 84, paper, true);
            L(new Rect(85, 296, 570, 80), "一个厨房，两道拿手菜。\n在三分钟里，把新鲜食材变成客人的笑容。", 25, new Color(0.80f, 0.88f, 0.83f));
            if (B(new Rect(86, 422, 252, 62), "开始营业   ENTER")) game.StartRound();
            L(new Rect(88, 507, 520, 50), "单人 3D 料理挑战  ·  两种配方  ·  180 秒", 17, new Color(0.71f, 0.80f, 0.75f));
            P(new Rect(810, 150, 380, 414), paper);
            L(new Rect(840, 181, 320, 36), "主厨操作手册", 27, ink, true);
            HelpRow(840, 250, "W A S D", "移动主厨");
            HelpRow(840, 307, "E", "取食材、放下、装盘、出餐");
            HelpRow(840, 364, "空格", "在切菜台加工，按三次");
            HelpRow(840, 421, "ESC", "暂停，查看配方");
            L(new Rect(840, 493, 320, 47), "先做沙拉熟悉路线，再试一锅浓汤。", 17, muted);
            L(new Rect(86, 646, 860, 30), "FRESH INGREDIENTS. GOOD FOOD. HAPPY PEOPLE.", 15, new Color(0.67f, 0.76f, 0.71f));
            if (B(new Rect(1070, 633, 118, 42), "退出", true)) Application.Quit();
        }

        private void HelpRow(float x, float y, string key, string description)
        {
            P(new Rect(x, y, 92, 38), new Color(0.87f, 0.92f, 0.86f));
            L(new Rect(x, y, 92, 38), key, 17, ink, true, TextAnchor.MiddleCenter);
            L(new Rect(x + 109, y + 4, 220, 43), description, 17, ink);
        }

        private void Hud(KitchenGame game)
        {
            P(new Rect(28, 22, 1224, 82), paper);
            L(new Rect(50, 33, 330, 25), "POCKET KITCHEN", 17, muted, true);
            L(new Rect(50, 58, 330, 34), "口袋小厨  /  今日营业", 25, ink, true);
            L(new Rect(520, 34, 230, 19), "距离打烊", 14, muted, false, TextAnchor.MiddleCenter);
            string time = FormatRemaining(game.Remaining);
            L(new Rect(520, 51, 230, 46), time, 33, game.Remaining < 30 ? KitchenVisuals.Red : ink, true, TextAnchor.MiddleCenter);
            L(new Rect(924, 34, 300, 22), "营业额   " + game.Completed + " 单已完成", 15, muted, false, TextAnchor.MiddleRight);
            L(new Rect(924, 56, 300, 37), game.Score.ToString("0000") + "  分", 29, ink, true, TextAnchor.MiddleRight);
            P(new Rect(32, 101, 1216 * game.Remaining / 180, 3), mint);
            L(new Rect(32, 133, 240, 32), "待制作订单", 24, paper, true);
            for (int i = 0; i < game.Orders.Count; i++) OrderCard(new Rect(28, 180 + i * 104, 230, 92), game.Orders[i]);
            P(new Rect(28, 517, 230, 98), paper);
            L(new Rect(45, 529, 194, 22), "主厨手中", 14, muted);
            L(new Rect(45, 556, 194, 42), KitchenNames.Item(game.chef.Held), 22, ink, true);
            P(new Rect(1048, 142, 204, 334), paper);
            L(new Rect(1066, 159, 168, 30), "今日菜单", 23, ink, true);
            L(new Rect(1066, 205, 168, 28), "01  田园沙拉", 19, KitchenVisuals.Teal, true);
            L(new Rect(1066, 240, 168, 75), "取生菜 → 放切菜台\n空格切三次\n取空盘 → 回台装盘", 16, muted);
            L(new Rect(1066, 329, 168, 28), "02  番茄浓汤", 19, KitchenVisuals.Red, true);
            L(new Rect(1066, 364, 168, 88), "番茄切好 → 下锅\n等待 5 秒\n取空盘 → 回锅盛汤", 16, muted);
            L(new Rect(1057, 493, 190, 70), "最后都要送到出餐口。\n连续完成可获得加分！", 16, paper);
            if (game.Streak > 1) L(new Rect(1049, 565, 200, 40), "完美配合 ×" + game.Streak, 22, gold, true);
            P(new Rect(28, 646, 1224, 52), new Color(0.07f, 0.19f, 0.20f, 0.96f));
            L(new Rect(49, 659, 1100, 30), "WASD 移动      E 互动 / 装盘 / 出餐      空格 切菜      ESC 暂停      回收桶可清空手中物品", 17, paper);
            if (game.chef.Focus != null)
            {
                P(new Rect(340, 566, 638, 61), paper);
                L(new Rect(360, 574, 600, 21), game.chef.Focus.Title, 14, muted, true);
                L(new Rect(360, 596, 600, 26), game.chef.Focus.Hint(game.chef), 19, ink, true);
            }
            if (Time.unscaledTime < game.ToastUntil)
            {
                P(new Rect(330, 119, 668, 44), game.ToastError ? new Color(0.98f, 0.81f, 0.62f) : new Color(0.82f, 0.94f, 0.82f));
                L(new Rect(345, 125, 638, 33), game.Toast, 16, ink, false, TextAnchor.MiddleCenter);
            }
            foreach (var station in game.Stations)
            {
                bool chop = station.kind == StationKind.Chopping && (station.Surface == KitchenItem.Tomato || station.Surface == KitchenItem.Lettuce);
                bool cook = station.kind == StationKind.Stove && station.Surface != KitchenItem.None;
                if (!chop && !cook) continue;
                Vector3 point = Camera.main.WorldToViewportPoint(station.transform.position + Vector3.up * 2.25f);
                Rect bar = new Rect(point.x * 1280 - 48, (1 - point.y) * 720, 96, 11);
                P(bar, new Color(0.1f, 0.2f, 0.2f));
                P(new Rect(bar.x + 2, bar.y + 2, 92 * (chop ? station.Cuts / 3f : station.CookTime / 5), 7), gold);
            }
        }

        private void OrderCard(Rect rect, KitchenOrder order)
        {
            P(rect, paper);
            GUI.color = KitchenNames.DishColor(order.dish); GUI.DrawTexture(new Rect(rect.x + 14, rect.y + 18, 43, 43), circle); GUI.color = Color.white;
            L(new Rect(rect.x + 14, rect.y + 18, 43, 43), order.dish == KitchenItem.Soup ? "汤" : "菜", 20, Color.white, true, TextAnchor.MiddleCenter);
            L(new Rect(rect.x + 70, rect.y + 12, 149, 29), KitchenNames.Item(order.dish), 20, ink, true);
            L(new Rect(rect.x + 70, rect.y + 42, 150, 23), "#" + order.number.ToString("00") + "  ·  " + Mathf.CeilToInt(order.remaining) + " 秒", 15, muted);
            P(new Rect(rect.x + 14, rect.y + 76, 202, 5), new Color(0.84f, 0.88f, 0.81f));
            P(new Rect(rect.x + 14, rect.y + 76, 202 * Mathf.Clamp01(order.remaining / order.duration), 5), order.remaining < 20 ? KitchenVisuals.Red : mint);
        }

        public static string FormatRemaining(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0, seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        private void Pause(KitchenGame game)
        {
            P(new Rect(-200, -200, 1680, 1120), new Color(0.04f, 0.13f, 0.14f, 0.78f));
            P(new Rect(408, 206, 464, 305), paper);
            L(new Rect(448, 239, 384, 58), "休息一下", 42, ink, true, TextAnchor.MiddleCenter);
            L(new Rect(446, 312, 388, 61), "计时已经暂停。\n准备好了，就继续今天的营业吧。", 20, muted, false, TextAnchor.MiddleCenter);
            if (B(new Rect(476, 411, 328, 58), "继续营业   ESC")) game.TogglePause();
        }

        private void End(KitchenGame game)
        {
            P(new Rect(-200, -200, 1680, 1120), new Color(0.04f, 0.13f, 0.14f, 0.82f));
            P(new Rect(332, 126, 616, 476), paper);
            L(new Rect(372, 157, 536, 30), "SHIFT COMPLETE  /  今日结算", 18, muted, true, TextAnchor.MiddleCenter);
            L(new Rect(372, 207, 536, 67), "辛苦了，主厨！", 43, ink, true, TextAnchor.MiddleCenter);
            L(new Rect(372, 284, 536, 76), game.Score + " 分", 58, KitchenVisuals.Teal, true, TextAnchor.MiddleCenter);
            L(new Rect(372, 382, 536, 68), "完成订单 " + game.Completed + "    ·    超时 " + game.Missed + "\n个人最佳  " + game.Best + " 分", 22, muted, false, TextAnchor.MiddleCenter);
            if (B(new Rect(447, 491, 386, 61), "再开一家店   ENTER / R")) game.StartRound();
        }
    }
}

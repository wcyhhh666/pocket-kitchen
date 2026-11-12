using UnityEngine;

namespace PocketKitchen
{
    public class KitchenStation : MonoBehaviour
    {
        public StationKind kind;
        public Transform itemPoint;
        public Renderer accent;
        public Renderer potLiquid;
        public KitchenItem Surface { get; private set; }
        public int Cuts { get; private set; }
        public float CookTime { get; private set; }
        public bool SoupReady => Surface == KitchenItem.ChoppedTomato && CookTime >= 5f;
        private GameObject itemVisual;
        private Color normalColor;
        private bool colorSet;

        private void LateUpdate()
        {
            if (potLiquid != null)
                potLiquid.material.color = Surface == KitchenItem.ChoppedTomato ? KitchenVisuals.Red : new Color(0.20f, 0.23f, 0.23f);
        }

        public string Title
        {
            get
            {
                switch (kind)
                {
                    case StationKind.Tomatoes: return "番茄箱";
                    case StationKind.Lettuce: return "生菜箱";
                    case StationKind.Plates: return "餐盘架";
                    case StationKind.Chopping: return "切菜台";
                    case StationKind.Stove: return "汤锅";
                    case StationKind.Delivery: return "出餐口";
                    default: return "回收桶";
                }
            }
        }

        public string Hint(ChefController chef)
        {
            if (kind == StationKind.Chopping && (Surface == KitchenItem.Tomato || Surface == KitchenItem.Lettuce))
                return "空格 切菜 " + Cuts + "/3    ·    E 拿起";
            if (kind == StationKind.Chopping && Surface == KitchenItem.ChoppedLettuce && chef.Held == KitchenItem.Plate)
                return "E 将生菜装盘";
            if (kind == StationKind.Stove && Surface != KitchenItem.None)
                return SoupReady ? "E 用空盘盛汤" : "正在熬煮，先去取一个空盘";
            if (kind == StationKind.Delivery) return "E 交付手中的成品";
            if (kind == StationKind.Trash) return "E 丢弃手中的物品";
            if (kind == StationKind.Chopping) return "E 放下食材 / 拿起食材";
            if (kind == StationKind.Stove) return "E 放入切好的番茄";
            return "E 取一份" + (kind == StationKind.Plates ? "餐盘" : kind == StationKind.Tomatoes ? "番茄" : "生菜");
        }

        public void SetSelected(bool selected)
        {
            if (accent == null) return;
            if (!colorSet) { normalColor = accent.material.color; colorSet = true; }
            accent.material.color = selected ? new Color(1, 0.82f, 0.35f) : normalColor;
        }

        public void Interact(ChefController chef)
        {
            var game = KitchenGame.Instance;
            if (game.Phase != GamePhase.Playing) return;
            if (kind == StationKind.Delivery) { game.Deliver(chef); return; }
            if (kind == StationKind.Trash)
            {
                if (chef.Held != KitchenItem.None) { chef.SetHeld(KitchenItem.None); game.Notify("已清理，可以重新准备", false); game.Sound(0); }
                else game.Notify("手中没有需要丢弃的物品", false);
                return;
            }
            if (kind == StationKind.Tomatoes || kind == StationKind.Lettuce || kind == StationKind.Plates)
            {
                if (chef.Held != KitchenItem.None) { game.Notify("双手已满，先放下物品或到回收桶清理", true); return; }
                chef.SetHeld(kind == StationKind.Tomatoes ? KitchenItem.Tomato : kind == StationKind.Lettuce ? KitchenItem.Lettuce : KitchenItem.Plate);
                game.Sound(0);
                return;
            }
            if (kind == StationKind.Stove)
            {
                if (chef.Held == KitchenItem.ChoppedTomato && Surface == KitchenItem.None)
                {
                    SetSurface(KitchenItem.ChoppedTomato); chef.SetHeld(KitchenItem.None); CookTime = 0;
                    game.Notify("开始熬汤，需要 5 秒；现在可以去取盘子", false); game.Sound(0);
                }
                else if (chef.Held == KitchenItem.Plate && SoupReady)
                {
                    chef.SetHeld(KitchenItem.Soup); SetSurface(KitchenItem.None); game.Sound(0);
                    game.Notify("浓汤装盘完成，送到出餐口吧", false);
                }
                else game.Notify(SoupReady ? "先去餐盘架取一个空盘，再来盛汤" : Surface != KitchenItem.None ? "汤还在熬煮，请稍等" : "汤锅需要切好的番茄", true);
                return;
            }
            if (kind == StationKind.Chopping)
            {
                if (chef.Held == KitchenItem.Plate && Surface == KitchenItem.ChoppedLettuce)
                {
                    chef.SetHeld(KitchenItem.Salad); SetSurface(KitchenItem.None); game.Notify("沙拉装盘完成，送到出餐口吧", false); game.Sound(0);
                }
                else if (chef.Held == KitchenItem.None && Surface != KitchenItem.None)
                {
                    chef.SetHeld(Surface); SetSurface(KitchenItem.None); game.Sound(0);
                }
                else if (Surface == KitchenItem.None && (chef.Held == KitchenItem.Tomato || chef.Held == KitchenItem.Lettuce || chef.Held == KitchenItem.ChoppedTomato || chef.Held == KitchenItem.ChoppedLettuce))
                {
                    SetSurface(chef.Held); chef.SetHeld(KitchenItem.None); game.Sound(0);
                    game.Notify("食材已放好，按空格 3 次切菜", false);
                }
                else game.Notify(Surface != KitchenItem.None ? "台面已有食材，请先取走，或拿空盘来装沙拉" : "请先取一份番茄或生菜", true);
            }
        }

        public void Work()
        {
            if (KitchenGame.Instance.Phase != GamePhase.Playing || kind != StationKind.Chopping) return;
            if (Surface != KitchenItem.Tomato && Surface != KitchenItem.Lettuce) return;
            Cuts++;
            KitchenGame.Instance.Sound(1);
            if (Cuts >= 3)
            {
                KitchenItem result = Surface == KitchenItem.Tomato ? KitchenItem.ChoppedTomato : KitchenItem.ChoppedLettuce;
                SetSurface(result);
                KitchenGame.Instance.Notify(result == KitchenItem.ChoppedLettuce ? "生菜切好了！去拿空盘，再回来装盘" : "番茄切好了！按 E 取走，送入汤锅", false);
            }
        }

        public void Tick(float seconds)
        {
            if (kind == StationKind.Stove && Surface == KitchenItem.ChoppedTomato && !SoupReady)
            {
                float before = CookTime;
                CookTime = Mathf.Min(5, CookTime + seconds);
                if (before < 5 && SoupReady) { KitchenGame.Instance.Notify("叮！番茄浓汤已煮好，用空盘来盛", false); KitchenGame.Instance.Sound(2); }
            }
        }

        public void SetSurface(KitchenItem item)
        {
            Surface = item; Cuts = 0; CookTime = 0;
            if (itemVisual != null) Destroy(itemVisual);
            if (item == KitchenItem.None) { itemVisual = null; return; }
            if (kind == StationKind.Chopping) itemVisual = KitchenVisuals.MakeItem(item, itemPoint, 0.9f);
        }

        public void ResetStation() { SetSurface(KitchenItem.None); SetSelected(false); }
    }
}

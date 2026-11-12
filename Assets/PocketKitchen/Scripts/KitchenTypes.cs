using System;
using UnityEngine;

namespace PocketKitchen
{
    public enum KitchenItem { None, Tomato, Lettuce, ChoppedTomato, ChoppedLettuce, Plate, Salad, Soup }
    public enum StationKind { Tomatoes, Lettuce, Plates, Chopping, Stove, Delivery, Trash }
    public enum GamePhase { Menu, Playing, Paused, Finished }

    [Serializable]
    public class KitchenOrder
    {
        public KitchenItem dish;
        public float remaining = 75f;
        public float duration = 75f;
        public int number;
    }

    public static class KitchenNames
    {
        public static string Item(KitchenItem item)
        {
            switch (item)
            {
                case KitchenItem.Tomato: return "番茄";
                case KitchenItem.Lettuce: return "生菜";
                case KitchenItem.ChoppedTomato: return "切好的番茄";
                case KitchenItem.ChoppedLettuce: return "切好的生菜";
                case KitchenItem.Plate: return "空盘子";
                case KitchenItem.Salad: return "田园沙拉";
                case KitchenItem.Soup: return "番茄浓汤";
                default: return "双手空闲";
            }
        }

        public static Color DishColor(KitchenItem item) => item == KitchenItem.Soup
            ? new Color(0.93f, 0.34f, 0.25f) : new Color(0.36f, 0.70f, 0.35f);
    }
}

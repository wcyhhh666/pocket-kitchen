using System.Collections.Generic;
using UnityEngine;

namespace PocketKitchen
{
    public static class KitchenVisuals
    {
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        public static IEnumerable<Material> AllMaterials => Materials.Values;
        public static readonly Color Teal = new Color(0.12f, 0.43f, 0.40f);
        public static readonly Color Cream = new Color(0.95f, 0.91f, 0.80f);
        public static readonly Color Wood = new Color(0.67f, 0.39f, 0.20f);
        public static readonly Color Red = new Color(0.91f, 0.23f, 0.16f);
        public static readonly Color Green = new Color(0.35f, 0.65f, 0.20f);
        public static Font WorldFont;

        public static Material Mat(string name, Color color)
        {
            if (Materials.TryGetValue(name, out Material existing) && existing != null) return existing;
            Material result = new Material(Shader.Find("Standard")) { name = "PK_" + name, color = color };
            result.SetFloat("_Glossiness", 0.25f);
            Materials[name] = result;
            return result;
        }

        public static GameObject Shape(string name, PrimitiveType primitive, Transform parent, Vector3 position, Vector3 size, Material material, bool solid = false)
        {
            GameObject go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (!solid && collider != null)
            {
                if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            }
            return go;
        }

        public static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material material, bool solid = false)
            => Shape(name, PrimitiveType.Cube, parent, position, size, material, solid);

        public static GameObject MakeItem(KitchenItem kind, Transform parent, float scale = 1)
        {
            GameObject root = new GameObject(KitchenNames.Item(kind));
            root.transform.SetParent(parent, false);
            root.transform.localScale = Vector3.one * scale;
            Material tomato = Mat("Tomato", Red);
            Material leaf = Mat("Leaf", Green);
            Material white = Mat("Porcelain", new Color(0.98f, 0.97f, 0.91f));
            if (kind == KitchenItem.Tomato)
            {
                Shape("Tomato", PrimitiveType.Sphere, root.transform, new Vector3(0, 0.24f, 0), new Vector3(0.49f, 0.43f, 0.49f), tomato);
                for (int i = 0; i < 4; i++)
                {
                    var sprig = Box("Stem", root.transform, new Vector3(0, 0.45f, 0), new Vector3(0.08f, 0.025f, 0.32f), leaf);
                    sprig.transform.localRotation = Quaternion.Euler(0, i * 45, 0);
                }
            }
            if (kind == KitchenItem.Lettuce)
            {
                for (int i = 0; i < 5; i++)
                {
                    float a = i * Mathf.PI * 0.4f;
                    Shape("LettuceLeaf", PrimitiveType.Sphere, root.transform, new Vector3(Mathf.Cos(a) * 0.1f, 0.2f, Mathf.Sin(a) * 0.1f), new Vector3(0.39f, 0.32f, 0.35f), i % 2 == 0 ? leaf : Mat("LeafLight", new Color(0.60f, 0.78f, 0.31f)));
                }
            }
            if (kind == KitchenItem.ChoppedTomato || kind == KitchenItem.ChoppedLettuce)
            {
                for (int i = 0; i < 6; i++)
                {
                    var bit = Box("ChoppedPiece", root.transform, new Vector3((i % 3 - 1) * 0.17f, 0.07f, (i / 3 - 0.5f) * 0.21f), new Vector3(0.14f, 0.12f, 0.17f), kind == KitchenItem.ChoppedTomato ? tomato : leaf);
                    bit.transform.localRotation = Quaternion.Euler(0, i * 19, 0);
                }
            }
            if (kind == KitchenItem.Plate || kind == KitchenItem.Salad || kind == KitchenItem.Soup)
            {
                Shape("Plate", PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.045f, 0), new Vector3(0.8f, 0.045f, 0.8f), white);
                Shape("PlateInside", PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.096f, 0), new Vector3(0.62f, 0.009f, 0.62f), Mat("PorcelainShade", new Color(0.86f, 0.88f, 0.82f)));
            }
            if (kind == KitchenItem.Salad)
            {
                for (int i = 0; i < 7; i++)
                {
                    float a = i * 2.4f;
                    var bit = Shape("SaladLeaf", PrimitiveType.Sphere, root.transform, new Vector3(Mathf.Cos(a) * 0.18f, 0.15f + (i % 2) * 0.04f, Mathf.Sin(a) * 0.18f), new Vector3(0.28f, 0.16f, 0.21f), i % 2 == 0 ? leaf : Mat("LeafLight", new Color(0.60f, 0.78f, 0.31f)));
                    bit.transform.localRotation = Quaternion.Euler(0, i * 35, 0);
                }
            }
            if (kind == KitchenItem.Soup)
            {
                Shape("Bowl", PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.15f, 0), new Vector3(0.59f, 0.09f, 0.59f), white);
                Shape("Soup", PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.245f, 0), new Vector3(0.50f, 0.008f, 0.50f), tomato);
                Box("Herb", root.transform, new Vector3(0.05f, 0.26f, 0.02f), new Vector3(0.15f, 0.018f, 0.06f), leaf);
            }
            return root;
        }

        public static TextMesh Label(string text, Transform parent, Vector3 position, float size, Color color)
        {
            GameObject go = new GameObject("Label_" + text);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            if (Camera.main != null) go.transform.rotation = Camera.main.transform.rotation;
            var mesh = go.AddComponent<TextMesh>();
            go.AddComponent<KitchenWorldLabel>();
            mesh.text = text; mesh.fontSize = 60; mesh.characterSize = size; mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center; mesh.color = color;
            if (WorldFont == null) WorldFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 60);
            mesh.font = WorldFont;
            if (WorldFont != null) go.GetComponent<MeshRenderer>().sharedMaterial = WorldFont.material;
            return mesh;
        }

        public static void BuildRoom(Transform root)
        {
            Box("KitchenPlatform", root, new Vector3(0, -0.36f, 0), new Vector3(14.5f, 0.55f, 10.5f), Mat("Platform", Teal), true);
            for (int x = 0; x < 14; x++) for (int z = 0; z < 10; z++)
                Box("FloorTile", root, new Vector3(x - 6.5f, -0.055f, z - 4.5f), new Vector3(0.985f, 0.1f, 0.985f), Mat((x + z) % 2 == 0 ? "TileCream" : "TileMint", (x + z) % 2 == 0 ? new Color(0.91f, 0.88f, 0.76f) : new Color(0.71f, 0.81f, 0.71f)));
            Box("BackWall", root, new Vector3(0, 1.4f, 5.12f), new Vector3(14.5f, 2.9f, 0.28f), Mat("Wall", new Color(0.87f, 0.81f, 0.66f)), true);
            Box("BackWallTrim", root, new Vector3(0, 0.45f, 4.94f), new Vector3(14.4f, 0.85f, 0.12f), Mat("WallTeal", Teal));
            Box("LeftWall", root, new Vector3(-7.12f, 0.35f, 0), new Vector3(0.22f, 0.8f, 10.5f), Mat("Wall", new Color(0.87f, 0.81f, 0.66f)), true);
            Box("RightWall", root, new Vector3(7.12f, 0.35f, 0), new Vector3(0.22f, 0.8f, 10.5f), Mat("Wall", new Color(0.87f, 0.81f, 0.66f)), true);
            Box("FrontBoundary", root, new Vector3(0, 0.18f, -5.08f), new Vector3(14.4f, 0.5f, 0.18f), Mat("WallTeal", Teal), true);
            for (int i = 0; i < 14; i++)
                Box("AwningStripe", root, new Vector3(i - 6.5f, 3.02f, 4.74f), new Vector3(1, 0.20f, 0.95f), Mat(i % 2 == 0 ? "AwningTeal" : "AwningCream", i % 2 == 0 ? Teal : Cream));
            Box("WindowFrame", root, new Vector3(-3.7f, 1.98f, 4.91f), new Vector3(2.9f, 1.5f, 0.12f), Mat("Wood", Wood));
            Box("WindowGlass", root, new Vector3(-3.7f, 1.98f, 4.82f), new Vector3(2.65f, 1.28f, 0.07f), Mat("Glass", new Color(0.48f, 0.74f, 0.78f)));
            Box("WindowDivider", root, new Vector3(-3.7f, 1.98f, 4.75f), new Vector3(0.09f, 1.3f, 0.09f), Mat("Wood", Wood));
            Box("MenuBoard", root, new Vector3(2.9f, 2.0f, 4.89f), new Vector3(3.5f, 1.45f, 0.15f), Mat("Board", new Color(0.10f, 0.21f, 0.20f)));
            Label("POCKET KITCHEN", root, new Vector3(2.9f, 2.12f, 4.65f), 0.07f, Cream);
            Label("FRESH  /  SIMPLE  /  HAPPY", root, new Vector3(2.9f, 1.78f, 4.6f), 0.028f, new Color(0.8f, 0.86f, 0.68f));
            Plant(root, new Vector3(-6.2f, 0, 3.9f));
            Plant(root, new Vector3(6.2f, 0, 3.9f));
            Box("ChefMat", root, new Vector3(0, 0.018f, -2.3f), new Vector3(2.4f, 0.035f, 1.3f), Mat("Mat", new Color(0.22f, 0.48f, 0.45f)));
        }

        private static void Plant(Transform parent, Vector3 at)
        {
            Shape("PlantPot", PrimitiveType.Cylinder, parent, at + new Vector3(0, 0.28f, 0), new Vector3(0.57f, 0.28f, 0.57f), Mat("Terracotta", new Color(0.75f, 0.38f, 0.22f)));
            for (int i = 0; i < 4; i++)
                Shape("PlantLeaf", PrimitiveType.Sphere, parent, at + new Vector3(Mathf.Sin(i * 2) * 0.2f, 0.78f + i * 0.08f, Mathf.Cos(i * 2) * 0.2f), new Vector3(0.38f, 0.72f, 0.30f), Mat("Leaf", Green));
        }

        public static KitchenStation BuildStation(StationKind kind, Vector3 position, Transform parent)
        {
            GameObject root = new GameObject("Station_" + kind);
            root.transform.SetParent(parent, false); root.transform.localPosition = position;
            var station = root.AddComponent<KitchenStation>(); station.kind = kind;
            bool trash = kind == StationKind.Trash;
            var baseColor = kind == StationKind.Delivery ? Teal : kind == StationKind.Chopping ? Wood : new Color(0.31f, 0.43f, 0.43f);
            Box("Cabinet", root.transform, new Vector3(0, 0.51f, 0), new Vector3(1.75f, 1.02f, 1.35f), Mat("Cabinet_" + kind, baseColor), true);
            Box("Countertop", root.transform, new Vector3(0, 1.08f, 0), new Vector3(1.90f, 0.15f, 1.50f), Mat("Countertop", Cream));
            Box("Door", root.transform, new Vector3(0, 0.53f, -0.69f), new Vector3(1.45f, 0.68f, 0.025f), Mat("Door_" + kind, Color.Lerp(baseColor, Color.white, 0.1f)));
            Box("Handle", root.transform, new Vector3(0, 0.73f, -0.73f), new Vector3(0.38f, 0.055f, 0.06f), Mat("Metal", new Color(0.75f, 0.78f, 0.72f)));
            var trim = Box("InteractionTrim", root.transform, new Vector3(0, 0.99f, -0.77f), new Vector3(1.8f, 0.08f, 0.06f), Mat("Trim_" + kind, new Color(0.36f, 0.72f, 0.63f)));
            station.accent = trim.GetComponent<Renderer>();
            station.itemPoint = new GameObject("ItemPoint").transform; station.itemPoint.SetParent(root.transform, false); station.itemPoint.localPosition = new Vector3(0, 1.17f, 0);
            Label(station.Title, root.transform, new Vector3(0, 1.95f, 0), 0.065f, new Color(0.07f, 0.20f, 0.19f));
            if (kind == StationKind.Tomatoes || kind == StationKind.Lettuce)
            {
                Box("ProduceTray", root.transform, new Vector3(0, 1.22f, 0), new Vector3(1.45f, 0.15f, 1), Mat("Wood", Wood));
                for (int i = 0; i < 3; i++)
                {
                    var item = MakeItem(kind == StationKind.Tomatoes ? KitchenItem.Tomato : KitchenItem.Lettuce, root.transform, 0.8f);
                    item.transform.localPosition = new Vector3((i - 1) * 0.42f, 1.3f, i % 2 * 0.15f);
                }
            }
            if (kind == StationKind.Plates)
                for (int i = 0; i < 5; i++) { var plate = MakeItem(KitchenItem.Plate, root.transform, 1); plate.transform.localPosition = new Vector3(0, 1.18f + i * 0.075f, 0); }
            if (kind == StationKind.Chopping)
            {
                Box("CuttingBoard", root.transform, new Vector3(0, 1.18f, 0), new Vector3(1.3f, 0.055f, 0.86f), Mat("BoardWood", new Color(0.80f, 0.61f, 0.35f)));
                Box("KnifeBlade", root.transform, new Vector3(0.65f, 1.24f, 0), new Vector3(0.1f, 0.03f, 0.48f), Mat("Metal", new Color(0.75f, 0.78f, 0.72f)));
                Box("KnifeHandle", root.transform, new Vector3(0.65f, 1.24f, -0.35f), new Vector3(0.1f, 0.07f, 0.28f), Mat("Board", new Color(0.10f, 0.21f, 0.20f)));
                station.itemPoint.localPosition = new Vector3(-0.12f, 1.23f, 0);
            }
            if (kind == StationKind.Stove)
            {
                Shape("Burner", PrimitiveType.Cylinder, root.transform, new Vector3(0, 1.17f, 0), new Vector3(1.0f, 0.025f, 1.0f), Mat("Burner", new Color(0.15f, 0.17f, 0.18f)));
                Shape("Pot", PrimitiveType.Cylinder, root.transform, new Vector3(0, 1.36f, 0), new Vector3(0.85f, 0.17f, 0.85f), Mat("Pot", new Color(0.22f, 0.26f, 0.27f)));
                station.potLiquid = Shape("PotLiquid", PrimitiveType.Cylinder, root.transform, new Vector3(0, 1.535f, 0), new Vector3(0.72f, 0.009f, 0.72f), Mat("Liquid", new Color(0.2f, 0.23f, 0.23f))).GetComponent<Renderer>();
                Box("PotHandle", root.transform, new Vector3(0.6f, 1.4f, 0), new Vector3(0.5f, 0.08f, 0.16f), Mat("Burner", new Color(0.15f, 0.17f, 0.18f)));
            }
            if (kind == StationKind.Delivery)
            {
                Box("ServingPad", root.transform, new Vector3(0, 1.18f, 0), new Vector3(1.4f, 0.05f, 1), Mat("ServingPad", new Color(0.80f, 0.65f, 0.34f)));
                Shape("ServiceBell", PrimitiveType.Sphere, root.transform, new Vector3(0.6f, 1.3f, 0.42f), new Vector3(0.3f, 0.18f, 0.3f), Mat("Bell", new Color(0.95f, 0.75f, 0.31f)));
            }
            if (trash)
            {
                Box("BinOpening", root.transform, new Vector3(0, 1.17f, 0), new Vector3(1.05f, 0.025f, 0.75f), Mat("Burner", new Color(0.15f, 0.17f, 0.18f)));
                Label("RECYCLE", root.transform, new Vector3(0, 0.6f, -0.78f), 0.04f, Cream);
            }
            return station;
        }

        public static ChefController BuildChef(Transform parent)
        {
            GameObject chef = new GameObject("Chef"); chef.transform.SetParent(parent, false); chef.transform.position = new Vector3(0, 0.1f, -2.3f);
            var motor = chef.AddComponent<CharacterController>(); motor.height = 1.95f; motor.radius = 0.30f; motor.center = new Vector3(0, 0.98f, 0); motor.stepOffset = 0.2f;
            var actor = chef.AddComponent<ChefController>();
            var white = Mat("Uniform", new Color(0.98f, 0.96f, 0.86f)); var skin = Mat("Skin", new Color(0.85f, 0.59f, 0.38f)); var dark = Mat("Shoes", new Color(0.10f, 0.18f, 0.18f));
            Shape("ChefBody", PrimitiveType.Capsule, chef.transform, new Vector3(0, 0.91f, 0), new Vector3(0.64f, 0.44f, 0.47f), white);
            Box("Apron", chef.transform, new Vector3(0, 0.87f, 0.22f), new Vector3(0.43f, 0.60f, 0.12f), Mat("Apron", Teal));
            Shape("Head", PrimitiveType.Sphere, chef.transform, new Vector3(0, 1.49f, 0), new Vector3(0.52f, 0.52f, 0.48f), skin);
            for (int i = -1; i <= 1; i += 2)
            {
                Shape("Eye", PrimitiveType.Sphere, chef.transform, new Vector3(i * 0.11f, 1.54f, 0.225f), Vector3.one * 0.055f, dark);
                Shape("Arm", PrimitiveType.Capsule, chef.transform, new Vector3(i * 0.40f, 1.02f, 0.1f), new Vector3(0.18f, 0.24f, 0.18f), white);
                Shape("Hand", PrimitiveType.Sphere, chef.transform, new Vector3(i * 0.38f, 0.87f, 0.26f), Vector3.one * 0.17f, skin);
            }
            Shape("Nose", PrimitiveType.Sphere, chef.transform, new Vector3(0, 1.44f, 0.25f), new Vector3(0.12f, 0.10f, 0.10f), skin);
            Shape("HatBand", PrimitiveType.Cylinder, chef.transform, new Vector3(0, 1.79f, 0), new Vector3(0.55f, 0.13f, 0.52f), white);
            for (int i = -1; i <= 1; i++) Shape("HatPuff", PrimitiveType.Sphere, chef.transform, new Vector3(i * 0.19f, 2.00f + (i == 0 ? 0.06f : 0), 0), new Vector3(0.39f, 0.37f, 0.44f), white);
            actor.leftFoot = Box("LeftFoot", chef.transform, new Vector3(-0.19f, 0.24f, 0.03f), new Vector3(0.24f, 0.44f, 0.36f), dark).transform;
            actor.rightFoot = Box("RightFoot", chef.transform, new Vector3(0.19f, 0.24f, 0.03f), new Vector3(0.24f, 0.44f, 0.36f), dark).transform;
            actor.carryPoint = new GameObject("CarryPoint").transform; actor.carryPoint.SetParent(chef.transform, false); actor.carryPoint.localPosition = new Vector3(0, 0.96f, 0.66f);
            return actor;
        }
    }
}

using UnityEngine;

namespace PocketKitchen
{
    [RequireComponent(typeof(TextMesh))]
    public class KitchenWorldLabel : MonoBehaviour
    {
        private void Start()
        {
            if (KitchenVisuals.WorldFont == null)
                KitchenVisuals.WorldFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 60);
            GetComponent<TextMesh>().font = KitchenVisuals.WorldFont;
            GetComponent<MeshRenderer>().sharedMaterial = KitchenVisuals.WorldFont.material;
            if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
        }
    }
}

using UnityEngine;

namespace PocketKitchen
{
    [RequireComponent(typeof(CharacterController))]
    public class ChefController : MonoBehaviour
    {
        public float speed = 4.2f;
        public Transform leftFoot;
        public Transform rightFoot;
        public Transform carryPoint;
        public KitchenItem Held { get; private set; }
        public CharacterController Motor { get; private set; }
        public KitchenStation Focus { get; private set; }
        private GameObject heldVisual;
        private Vector3 spawn;
        private float walk;

        private void Awake()
        {
            Motor = GetComponent<CharacterController>();
            spawn = transform.position;
        }

        private void Update()
        {
            var game = KitchenGame.Instance;
            if (game == null) return;
            if (game.Phase != GamePhase.Playing)
            {
                if (Focus != null) Focus.SetSelected(false);
                Focus = null;
                return;
            }

            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1);
            Motor.Move((input * speed + Vector3.down * 6) * Time.deltaTime);
            if (input.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(input), 16 * Time.deltaTime);
            walk += input.magnitude * Time.deltaTime * 12;
            if (leftFoot != null) leftFoot.localRotation = Quaternion.Euler(Mathf.Sin(walk) * 22 * input.magnitude, 0, 0);
            if (rightFoot != null) rightFoot.localRotation = Quaternion.Euler(-Mathf.Sin(walk) * 22 * input.magnitude, 0, 0);

            KitchenStation nearest = null;
            float best = 1.85f;
            foreach (var station in game.Stations)
            {
                Vector3 offset = station.transform.position - transform.position;
                offset.y = 0;
                float distance = offset.magnitude;
                if (distance < best) { best = distance; nearest = station; }
            }
            if (Focus != nearest)
            {
                if (Focus != null) Focus.SetSelected(false);
                Focus = nearest;
                if (Focus != null) Focus.SetSelected(true);
            }
            if (Input.GetKeyDown(KeyCode.E) && Focus != null) Focus.Interact(this);
            if (Input.GetKeyDown(KeyCode.Space) && Focus != null) Focus.Work();
        }

        public void SetHeld(KitchenItem item)
        {
            Held = item;
            if (heldVisual != null) Destroy(heldVisual);
            heldVisual = item == KitchenItem.None ? null : KitchenVisuals.MakeItem(item, carryPoint, 0.8f);
        }

        public void Teleport(Vector3 position)
        {
            Motor.enabled = false;
            transform.position = position;
            Motor.enabled = true;
            Physics.SyncTransforms();
        }

        public void ResetChef()
        {
            SetHeld(KitchenItem.None);
            Teleport(spawn);
            transform.rotation = Quaternion.identity;
        }
    }
}

using UnityEngine;

namespace WheelOfFortune.Wheel
{
    public class WheelTestInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WheelController wheelController;

        [Header("Settings")]
        [SerializeField] private bool enableTestInput = true;

        private void Awake()
        {
            if (wheelController == null)
            {
                wheelController = FindObjectOfType<WheelController>();
            }
        }

        private void Update()
        {
            if (!enableTestInput || wheelController == null)
                return;

            // SPACE = Spin
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[WheelTestInput] SPACE pressed - Spinning!");
                wheelController.SpinWheel();
            }

            // R = Regenerate
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[WheelTestInput] R pressed - Regenerating!");
                wheelController.RegenerateWheel();
            }

            // Number keys = Load zone
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("[WheelTestInput] 1 pressed - Zone 1");
                wheelController.ConfigureWheelForZone(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Debug.Log("[WheelTestInput] 5 pressed - Zone 5 (Safe)");
                wheelController.ConfigureWheelForZone(5);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && Input.GetKey(KeyCode.LeftShift))
            {
                Debug.Log("[WheelTestInput] Shift+3 pressed - Zone 30 (Super)");
                wheelController.ConfigureWheelForZone(30);
            }
        }

        private void OnGUI()
        {
            if (!enableTestInput)
                return;

            // Help text
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== WHEEL TEST CONTROLS ===");
            GUILayout.Label("SPACE = Spin Wheel");
            GUILayout.Label("R = Regenerate Wheel");
            GUILayout.Label("1 = Load Zone 1");
            GUILayout.Label("5 = Load Zone 5 (Safe)");
            GUILayout.Label("Shift+3 = Load Zone 30 (Super)");
            GUILayout.EndArea();
        }
    }
}

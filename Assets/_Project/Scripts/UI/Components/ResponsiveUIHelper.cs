using UnityEngine;

namespace WheelOfFortune.UI.Components
{
    public class ResponsiveUIHelper : MonoBehaviour
    {

        [Header("Aspect Ratio Settings")]
        [SerializeField] private float targetAspectRatio = 16f / 9f;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Responsive Adjustments")]
        [SerializeField] private bool adjustForUltraWide = true;
        [SerializeField] private bool adjustForLegacy = true;

        [Header("Scale Multipliers")]
        [SerializeField] private float ultraWideScaleMultiplier = 1.2f;
        [SerializeField] private float legacyScaleMultiplier = 0.9f;

        private AspectRatioType currentAspectRatio;
        private RectTransform rectTransform;

        public enum AspectRatioType
        {
            UltraWide,  // 20:9
            Standard,   // 16:9
            Legacy      // 4:3
        }

        public AspectRatioType CurrentAspectRatio => currentAspectRatio;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            CheckAndApplyAspectRatio();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                CheckAndApplyAspectRatio();
            }
        }
#endif

        private void CheckAndApplyAspectRatio()
        {
            float currentAspect = (float)Screen.width / Screen.height;
            currentAspectRatio = DetectAspectRatioType(currentAspect);

            LogDebug($"Screen Resolution: {Screen.width}x{Screen.height}");
            LogDebug($"Current Aspect: {currentAspect:F2}");
            LogDebug($"Detected Type: {currentAspectRatio}");

            ApplyResponsiveAdjustments();
        }

        private AspectRatioType DetectAspectRatioType(float aspectRatio)
        {
            // 20:9 = 2.22
            if (Mathf.Abs(aspectRatio - (20f / 9f)) < 0.1f)
            {
                return AspectRatioType.UltraWide;
            }
            // 16:9 = 1.78
            else if (Mathf.Abs(aspectRatio - (16f / 9f)) < 0.1f)
            {
                return AspectRatioType.Standard;
            }
            // 4:3 = 1.33
            else if (Mathf.Abs(aspectRatio - (4f / 3f)) < 0.1f)
            {
                return AspectRatioType.Legacy;
            }

            // Default olarak Standard kabul et
            return AspectRatioType.Standard;
        }

        private void ApplyResponsiveAdjustments()
        {
            if (rectTransform == null)
                return;

            switch (currentAspectRatio)
            {
                case AspectRatioType.UltraWide:
                    if (adjustForUltraWide)
                    {
                        ApplyUltraWideAdjustments();
                    }
                    break;

                case AspectRatioType.Legacy:
                    if (adjustForLegacy)
                    {
                        ApplyLegacyAdjustments();
                    }
                    break;

                case AspectRatioType.Standard:
                default:
                    // Standard için ayarlama gerekmiyor
                    break;
            }
        }

        private void ApplyUltraWideAdjustments()
        {
            // UI elementlerini biraz büyült
            Vector3 newScale = rectTransform.localScale * ultraWideScaleMultiplier;
            rectTransform.localScale = newScale;

            LogDebug($"Applied Ultra-Wide adjustments: Scale = {ultraWideScaleMultiplier}x");
        }

        private void ApplyLegacyAdjustments()
        {
            // UI elementlerini biraz küçült
            Vector3 newScale = rectTransform.localScale * legacyScaleMultiplier;
            rectTransform.localScale = newScale;

            LogDebug($"Applied Legacy adjustments: Scale = {legacyScaleMultiplier}x");
        }

        public void ForceAspectRatioCheck()
        {
            CheckAndApplyAspectRatio();
        }

        public string GetAspectRatioInfo()
        {
            float currentAspect = (float)Screen.width / Screen.height;
            return $"{currentAspectRatio} ({currentAspect:F2})";
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ResponsiveUIHelper] {message}");
            }
        }

        [ContextMenu("Debug Aspect Ratio Info")]
        public void DebugAspectRatioInfo()
        {
            Debug.Log("=== Aspect Ratio Info ===");
            Debug.Log($"Screen: {Screen.width}x{Screen.height}");
            Debug.Log($"Current Aspect: {(float)Screen.width / Screen.height:F2}");
            Debug.Log($"Type: {currentAspectRatio}");
            Debug.Log($"Target: {targetAspectRatio:F2}");
        }

    }
}

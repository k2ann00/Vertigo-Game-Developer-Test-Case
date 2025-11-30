using System;
using System.Collections;
using UnityEngine;
using WheelOfFortune.Interfaces;

namespace WheelOfFortune.Wheel
{
    public class WheelSpinner : MonoBehaviour, ISpinnable
    {

        [Header("Spin Settings")]
        [SerializeField] private float minSpinDuration = 2f;
        [SerializeField] private float maxSpinDuration = 4f;
        [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private int minExtraRotations = 3;
        [SerializeField] private int maxExtraRotations = 5;

        [Header("References")]
        [SerializeField] private Transform wheelTransform;

        private bool isSpinning;
        private Coroutine spinCoroutine;

        public bool IsSpinning => isSpinning;

        private void Awake()
        {
            if (wheelTransform == null)
            {
                wheelTransform = transform;
            }
        }

        public void Spin(float duration)
        {
            if (isSpinning)
            {
                Debug.LogWarning("[WheelSpinner] Already spinning!");
                return;
            }

            // Rastgele bir hedef belirle
            int randomSlice = UnityEngine.Random.Range(0, 8); // 8 dilim varsayıyoruz
            Stop(randomSlice);
        }

        public void Stop(int targetIndex)
        {
            if (isSpinning)
            {
                Debug.LogWarning("[WheelSpinner] Already spinning!");
                return;
            }

            if (spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
            }

            float duration = UnityEngine.Random.Range(minSpinDuration, maxSpinDuration);
            spinCoroutine = StartCoroutine(SpinToTargetCoroutine(targetIndex, duration));
        }

        private IEnumerator SpinToTargetCoroutine(int targetIndex, float duration)
        {
            isSpinning = true;

            // Mevcut rotation'ı al
            float startRotation = wheelTransform.eulerAngles.z;

            // Mevcut rotation'ı normalize et (0-360 arası)
            float normalizedStartRotation = startRotation % 360f;
            if (normalizedStartRotation < 0f)
                normalizedStartRotation += 360f;

            int extraRotations = UnityEngine.Random.Range(minExtraRotations, maxExtraRotations);

            // Hedef açıyı hesapla (absolute angle from 0)
            // Her slice 45 derece (360 / 8)
            float degreesPerSlice = 360f / 8f;
            float targetAngle = (targetIndex * degreesPerSlice);

            // FİX: Target'a ulaşmak için gereken DELTA rotation'ı hesapla
            // Delta her zaman pozitif olmalı (ileri döneceğiz)
            float rotationDelta = targetAngle - normalizedStartRotation;

            // Eğer delta negatifse, tam tur ekle (ileri dönüş için)
            if (rotationDelta < 0f)
            {
                rotationDelta += 360f;
            }

            // Ekstra dönüşleri ekle
            rotationDelta += (extraRotations * 360f);

            // FİX: Final target rotation = current rotation + delta
            float targetRotation = startRotation + rotationDelta;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / duration;

                // AnimationCurve kullanarak smooth rotation
                float curveValue = spinCurve.Evaluate(normalizedTime);
                float currentRotation = Mathf.Lerp(startRotation, targetRotation, curveValue);

                wheelTransform.eulerAngles = new Vector3(0, 0, currentRotation);

                yield return null;
            }

            // Final rotation'ı ayarla (tam olarak target angle'da dur)
            // FİX: Normalize edilmiş final rotation kullan
            float finalRotation = targetAngle; // Target angle directly (0-360 range)
            wheelTransform.eulerAngles = new Vector3(0, 0, finalRotation);

            isSpinning = false;
            OnSpinCompleted?.Invoke(targetIndex);

            Debug.Log($"[WheelSpinner] Spin completed: targetIndex={targetIndex}, finalRotation={finalRotation}°");
        }

        public event Action<int> OnSpinCompleted;

        public bool IsCurrentlySpinning()
        {
            return isSpinning;
        }

        public void ForceStop()
        {
            if (spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
                spinCoroutine = null;
            }

            isSpinning = false;
        }

    }
}

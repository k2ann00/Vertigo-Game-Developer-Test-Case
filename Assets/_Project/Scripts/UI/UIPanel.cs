using System.Collections;
using UnityEngine;
// using DG.Tweening; // TODO: Import DOTween for smooth animations

namespace WheelOfFortune.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {

        [Header("Panel Settings")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected bool hideOnStart = false;

        [Header("Animation Settings")]
        [SerializeField] protected float animationDuration = 0.3f;
        // Ease types removed - using simple linear interpolation
        // TODO: Re-enable when DOTween is imported

        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            ValidateComponents();

            if (hideOnStart)
            {
                Hide(instant: true);
            }
        }

        protected virtual void OnValidate()
        {
            // CanvasGroup'u otomatik ata
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void ValidateComponents()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    Debug.LogError($"[UIPanel] CanvasGroup not found on {gameObject.name}");
                }
            }
        }

        public virtual void Show(bool animated = true)
        {
            if (IsVisible)
                return;

            gameObject.SetActive(true);
            IsVisible = true;

            if (animated)
            {
                AnimateShow();
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                OnShowCompleted();
            }
        }

        public virtual void Hide(bool instant = false)
        {
            if (!IsVisible && !gameObject.activeSelf)
                return;

            IsVisible = false;

            if (instant)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
                OnHideCompleted();
            }
            else
            {
                AnimateHide();
            }
        }

        protected virtual void AnimateShow()
        {
            // Fallback coroutine animation (DOTween yoksa)
            StartCoroutine(FadeIn());
        }

        protected virtual void AnimateHide()
        {
            // Fallback coroutine animation (DOTween yoksa)
            StartCoroutine(FadeOut());
        }

        protected IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            OnShowCompleted();
        }

        protected IEnumerator FadeOut()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / animationDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            OnHideCompleted();
        }

        protected virtual void OnShowCompleted()
        {
            // Override edilebilir
        }

        protected virtual void OnHideCompleted()
        {
            // Override edilebilir
        }

        public void SetInteractable(bool interactable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
            }
        }

        public void SetBlocksRaycasts(bool blocks)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = blocks;
            }
        }

        protected virtual void OnDestroy()
        {
            // Coroutine'leri durdur
            StopAllCoroutines();
        }

    }
}

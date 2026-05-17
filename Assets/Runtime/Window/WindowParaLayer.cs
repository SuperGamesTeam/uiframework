using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace deVoid.UIFramework {
    /// <summary>
    /// This is a "helper" layer so Windows with higher priority can be displayed.
    /// By default, it contains any window tagged as a Popup. It is controlled by the WindowUILayer.
    /// </summary>
    public class WindowParaLayer : MonoBehaviour {
        [SerializeField] private GameObject darkenBgObject = null;
        [SerializeField] private float _appearDarkenBgDuration = 0.4f;
        private CanvasGroup _darkenBgCanvasGroup;
        private Coroutine _activeRoutine;
        private bool _isShown;

        private List<GameObject> containedScreens = new List<GameObject>();
        
        private void Awake() {
            EnsureCanvasGroupExist();
        }
        
        public void AddScreen(Transform screenRectTransform) {
            screenRectTransform.SetParent(transform, false);
            containedScreens.Add(screenRectTransform.gameObject);
        }

        public void RefreshDarken() {
            for (int i = 0; i < containedScreens.Count; i++) {
                if (containedScreens[i] != null && containedScreens[i].activeSelf) {
                    ShowDarkenBg();
                    return;
                }
            }

            HideDarkenBg();
        }

        public void DarkenBG() => ShowDarkenBg();
        
        private void ShowDarkenBg() {
            if (_isShown) {
                RepositionDarkenBg();
                return;
            }

            _isShown = true;
            StopActiveDarkenBgRoutine();
            darkenBgObject.SetActive(true);
            RepositionDarkenBg();
            _activeRoutine = StartCoroutine(FadeInDarkenBgCoroutine());
        }

        private void RepositionDarkenBg() {
            // Find the last active contained screen and place darkenBg right before it
            GameObject lastActive = null;
            for (int i = containedScreens.Count - 1; i >= 0; i--) {
                if (containedScreens[i] != null && containedScreens[i].activeSelf) {
                    lastActive = containedScreens[i];
                    break;
                }
            }

            if (lastActive != null) {
                int targetIndex = lastActive.transform.GetSiblingIndex();
                // When darkenBg is already before the target, removing it shifts the target
                // down by one, so we must account for that to avoid landing in front of it.
                if (darkenBgObject.transform.GetSiblingIndex() < targetIndex) {
                    targetIndex--;
                }
                darkenBgObject.transform.SetSiblingIndex(targetIndex);
            }
            // When no active screen exists (transition in progress), leave darkenBg in place —
            // the incoming screen will trigger a reposition once it becomes active.
        }

        private void HideDarkenBg() {
            if (!_isShown) return;

            _isShown = false;
            StopActiveDarkenBgRoutine();
            _activeRoutine = StartCoroutine(FadeOutDarkenBgCoroutine());
        }
        
        private void EnsureCanvasGroupExist() {
            _darkenBgCanvasGroup = darkenBgObject.GetComponent<CanvasGroup>();
            if (_darkenBgCanvasGroup == null) {
                _darkenBgCanvasGroup = darkenBgObject.AddComponent<CanvasGroup>();
            }
        }
        
        private void StopActiveDarkenBgRoutine() {
            if (_activeRoutine != null) {
                StopCoroutine(_activeRoutine);
            }
        }
        
        private IEnumerator FadeInDarkenBgCoroutine() {
            float t = Time.deltaTime;
            SetDarkenBgAlpha(0);
            while (t < _appearDarkenBgDuration) {
                SetDarkenBgAlpha(t / _appearDarkenBgDuration);
                _darkenBgCanvasGroup.alpha = t / _appearDarkenBgDuration;
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }

            SetDarkenBgAlpha(1);
        }
        
        private IEnumerator FadeOutDarkenBgCoroutine() {
            float t = Time.deltaTime;
            SetDarkenBgAlpha(1);
            while (t < _appearDarkenBgDuration) {
                SetDarkenBgAlpha(1f - t / _appearDarkenBgDuration);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }

            SetDarkenBgAlpha(0);
            darkenBgObject.SetActive(false);
        }
        
        private void SetDarkenBgAlpha(float a) => _darkenBgCanvasGroup.alpha = a;
    }
}
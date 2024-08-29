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
                if (containedScreens[i] != null) {
                    if (containedScreens[i].activeSelf) {
                        ShowDarkenBg();
                        return;
                    }
                }
            }

            HideDarkenBg();
        }

        public void DarkenBG() => ShowDarkenBg();
        
        private void ShowDarkenBg() {
            StopActiveDarkenBgRoutine();
            darkenBgObject.SetActive(true);
            darkenBgObject.transform.SetAsFirstSibling();
            _activeRoutine = StartCoroutine(FadeInDarkenBgCoroutine());
        }

        private void HideDarkenBg() {
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
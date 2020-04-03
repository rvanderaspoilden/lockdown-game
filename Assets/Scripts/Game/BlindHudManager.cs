using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class BlindHudManager : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Image[] orderedSteps;

        [SerializeField] private float delayToRemove;

        private float timer;

        private Coroutine checkDelayCoroutine;
        private Coroutine hideAnimationCoroutine;

        private void OnDestroy() {
            StopAllCoroutines();
        }

        public void NextStep() {
            if (this.hideAnimationCoroutine != null) {
                StopCoroutine(this.hideAnimationCoroutine);
                this.hideAnimationCoroutine = null;
            }
            
            canvasGroup.alpha = 1f;

            foreach (Image step in this.orderedSteps) {
                if (!step.enabled) {
                    step.enabled = true;
                    break;
                }
            }

            timer = this.delayToRemove;

            if (this.checkDelayCoroutine == null) {
                this.checkDelayCoroutine = StartCoroutine(this.CheckDelay());
            }
        }

        public void Hide(bool instant = false) {
            if (this.checkDelayCoroutine != null) {
                StopCoroutine(this.checkDelayCoroutine);
                this.checkDelayCoroutine = null; // todo check utility
            }

            if (instant) {
                this.canvasGroup.alpha = 0;
            } else {
                this.hideAnimationCoroutine = StartCoroutine(this.HideAnimation());
            }
        }

        private IEnumerator HideAnimation() {
            while (canvasGroup.alpha > 0) {
                canvasGroup.alpha -= Time.deltaTime;
                
                yield return new WaitForEndOfFrame();
            }
            
            foreach (Image step in this.orderedSteps) {
                step.enabled = false;
            }
        }

        private IEnumerator CheckDelay() {
            while (this.timer > 0f) {
                this.timer -= Time.deltaTime;
                yield return null;
            }

            this.Hide();
        }
    }
}
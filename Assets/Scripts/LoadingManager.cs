using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

namespace Game {
    public class LoadingManager : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private GameObject hudContainer;
        [SerializeField] private float loadingSmoothSpeed;

        [SerializeField] private Image loadingBar;

        public static LoadingManager instance;

        private void Awake() {
            if (instance) {
                Destroy(this);
            } else {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        private void Start() {
            this.Hide();
        }

        public void Show(bool withLoader) {
            this.hudContainer.SetActive(true);
            this.loadingBar.enabled = withLoader;

            if (withLoader) {
                StartCoroutine(this.Loading());
            }
        }

        public void Hide() {
            this.hudContainer.SetActive(false);
        }

        private IEnumerator Loading() {
            float fillValue = 0.1f;
            this.loadingBar.fillAmount = 0f;

            while (this.loadingBar.fillAmount <= 1) {
                if (PhotonNetwork.LevelLoadingProgress >= 0.1f) {
                    fillValue = PhotonNetwork.LevelLoadingProgress;
                }
                
                this.loadingBar.fillAmount += fillValue * Time.deltaTime * this.loadingSmoothSpeed;

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
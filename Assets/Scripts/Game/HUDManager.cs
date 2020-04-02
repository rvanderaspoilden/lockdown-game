using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class HUDManager : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private Image statusImage;

        [SerializeField] private Sprite contaminedSprite;
        [SerializeField] private Sprite confinedSprite;

        [SerializeField] private TextMeshProUGUI informationText;

        [SerializeField] private Image aimImage;
        [SerializeField] private Image contaminedCameraFilter;
        [SerializeField] private float contaminedCameraFilterMaxOpacity;

        public static HUDManager instance;

        private void Awake() {
            instance = this;

            this.SetAimVisibility(false);
            this.SetContaminedStatus(false);
            this.SetInformation(String.Empty);
        }

        public void SetContaminedStatus(bool isContamined) {
            this.statusImage.sprite = isContamined ? this.contaminedSprite : this.confinedSprite;
            this.SetContaminedCameraFilterOpacity(isContamined ? 1 : 0);
        }

        public void SetAimVisibility(bool visible) {
            this.aimImage.enabled = visible;
        }

        /**
         * Opacity need to be between 0 and 1
         */
        public void SetContaminedCameraFilterOpacity(float opacity) {
            this.contaminedCameraFilter.color = new Color(1,1,1,opacity * this.contaminedCameraFilterMaxOpacity);
        }

        public void SetInformation(string value) {
            this.informationText.text = value;
        }
    }
}
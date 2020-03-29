using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class HUDManager : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private TextMeshProUGUI lifeText;
        [SerializeField] private Image aimImage;

        public static HUDManager instance;

        private void Awake() {
            instance = this;
            
            this.SetAimVisibility(false);
        }

        public void RefreshLifeUI(float life) {
            this.lifeText.text = life.ToString();
        }

        public void SetAimVisibility(bool visible) {
            this.aimImage.enabled = visible;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game {
    public class HUDManager : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private TextMeshProUGUI lifeText;

        public static HUDManager instance;

        private void Awake() {
            instance = this;
        }

        public void RefreshLifeUI(float life) {
            this.lifeText.text = life.ToString();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class HUDManager : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private Image statusImage;

        [SerializeField] private Sprite contaminedSprite;
        [SerializeField] private Sprite confinedSprite;

        [SerializeField] private Image aimImage;

        public static HUDManager instance;

        private void Awake() {
            instance = this;

            this.SetAimVisibility(false);
            this.SetContaminedStatus(false);
        }

        public void SetContaminedStatus(bool isContamined) {
            this.statusImage.sprite = isContamined ? this.contaminedSprite : this.confinedSprite;
        }

        public void SetAimVisibility(bool visible) {
            this.aimImage.enabled = visible;
        }
    }
}
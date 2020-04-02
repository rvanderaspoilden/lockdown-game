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

        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Image soundImage;
        [SerializeField] private Sprite muteSprite;
        [SerializeField] private Sprite unmuteSprite;

        [SerializeField] private Image aimImage;
        [SerializeField] private Image contaminedCameraFilter;
        [SerializeField] private float contaminedCameraFilterMaxOpacity;

        public static HUDManager instance;

        public static bool isHudOpened;

        private void Awake() {
            instance = this;

            this.SetAimVisibility(false);
            this.SetContaminedStatus(false);
            this.SetInformation(String.Empty);
            this.CloseOptions();
        }

        public void SetContaminedStatus(bool isContamined) {
            this.statusImage.sprite = isContamined ? this.contaminedSprite : this.confinedSprite;
            this.SetContaminedCameraFilterOpacity(isContamined ? 1 : 0);
        }

        public void SetAimVisibility(bool visible) {
            this.aimImage.enabled = visible;
        }

        public void CloseOptions() {
            this.optionsPanel.SetActive(false);
            isHudOpened = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (GameManager.localPlayer) {
                GameManager.localPlayer.UnlockCamera();
            }
        }

        public void OpenOptions() {
            this.soundImage.sprite = PlayerPrefs.GetInt("mute") == 1 ? this.muteSprite : this.unmuteSprite;
            this.optionsPanel.SetActive(true);
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            isHudOpened = true;

            if (GameManager.localPlayer) {
                GameManager.localPlayer.LockCamera();
            }
        }

        public void ToggleMusic() {
            PlayerPrefs.SetInt("mute", PlayerPrefs.GetInt("mute") == 1 ? 0 : 1);
            GameManager.instance.SetMuteMusic(PlayerPrefs.GetInt("mute") == 1);
            this.soundImage.sprite = PlayerPrefs.GetInt("mute") == 1 ? this.muteSprite : this.unmuteSprite;
        }

        /**
         * Opacity need to be between 0 and 1
         */
        public void SetContaminedCameraFilterOpacity(float opacity) {
            this.contaminedCameraFilter.color = new Color(1, 1, 1, opacity * this.contaminedCameraFilterMaxOpacity);
        }

        public void SetInformation(string value) {
            this.informationText.text = value;
        }
    }
}
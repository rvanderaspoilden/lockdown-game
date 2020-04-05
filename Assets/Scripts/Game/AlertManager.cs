using System;
using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Game {
    [RequireComponent(typeof(PhotonView))]
    public class AlertManager : MonoBehaviourPun {
        [Header("Settings")]
        [SerializeField] private CanvasGroup panel;

        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private float duration;

        [SerializeField] private Color contaminedColor;
        [SerializeField] private Color confinedColor;
        [SerializeField] private Color generalColor;

        private Coroutine displayCoroutine;

        public static AlertManager instance;

        private void Awake() {
            instance = this;
        }

        private void Start() {
            this.titleText.text = String.Empty;
        }

        private void OnDestroy() {
            StopAllCoroutines();
        }

        public void Alert(string message, AlertType alertType, RpcTarget rpcTarget) {
            photonView.RPC("RPC_Alert", rpcTarget, message, this.GetAlertTypeValue(alertType));
        }
        
        public void Alert(string message, AlertType alertType, Photon.Realtime.Player playerTarget) {
            photonView.RPC("RPC_Alert", playerTarget, message, this.GetAlertTypeValue(alertType));
        }

        [PunRPC]
        public void RPC_Alert(string message, int alertTypeValue) {
            this.panel.alpha = 1;
            this.titleText.text = message;

            AlertType alertType = (AlertType) alertTypeValue;

            switch (alertType) {
                case AlertType.GENERAL:
                    this.titleText.color = this.generalColor;
                    break;
                case AlertType.CONFINED:
                    this.titleText.color = this.confinedColor;
                    break;
                case AlertType.CONTAMINED:
                    this.titleText.color = this.contaminedColor;
                    break;
                default:
                    this.titleText.color = this.generalColor;
                    break;
            }

            if (this.displayCoroutine == null) {
                this.displayCoroutine = StartCoroutine(this.DisplayCoroutine());
            }
        }

        private IEnumerator DisplayCoroutine() {
            while (this.panel.alpha > 0) {
                this.panel.alpha -= Time.deltaTime / this.duration;
                yield return new WaitForEndOfFrame();
            }

            this.displayCoroutine = null;
        }

        private int GetAlertTypeValue(AlertType alertType) {
            return (int) Enum.Parse(typeof(AlertType), alertType.ToString());
        }
    }

    public enum AlertType {
        CONFINED = 0,
        CONTAMINED = 1,
        GENERAL = 2
    }
}
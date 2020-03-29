using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player {
    public class PlayerInteraction : MonoBehaviour {
        [Header("Only for debug")]
        [SerializeField] private PlayerHands hands;

        [SerializeField] private PlayerEntity playerEntity;

        private Coroutine covidCoroutine;

        private void Awake() {
            this.playerEntity = GetComponent<PlayerEntity>();
            this.hands = GetComponent<PlayerHands>();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Weapon") && !this.hands.HasWeapon()) {
                this.hands.SetWeapon(other.GetComponent<Weapon>());
            }
        }

        private void OnTriggerStay(Collider other) {
            if (other.CompareTag("CovidArea") && covidCoroutine == null) {
                Debug.Log("Enter in covid area");
                this.covidCoroutine = StartCoroutine(this.TakeDamageFromCovid());
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.CompareTag("CovidArea")) {
                Debug.Log("Covid left");
                StopCoroutine(this.covidCoroutine);
                this.covidCoroutine = null;
            }
        }

        private IEnumerator TakeDamageFromCovid() {
            while (true) {
                yield return new WaitForSeconds(3f);
                this.playerEntity.TakeDamage(10f);
                Debug.Log("Take damage from covid");
            }
        }
    }
}
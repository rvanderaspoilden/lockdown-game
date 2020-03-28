using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player {
    public class PlayerInteraction : MonoBehaviour {
        [Header("Only for debug")]
        [SerializeField] private PlayerHands hands;

        private void Awake() {
            this.hands = GetComponent<PlayerHands>();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Weapon") && !this.hands.HasWeapon()) {
                this.hands.SetWeapon(other.GetComponent<Weapon>());
            }
        }
    }
}

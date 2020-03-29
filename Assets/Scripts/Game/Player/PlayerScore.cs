using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player {
    public class PlayerScore : MonoBehaviour {
        [Header("Only for debug")]
        [SerializeField] private int contaminedAI;
        [SerializeField] private int contaminedPlayer;
        [SerializeField] private int contaminedKilled;

        public int GetContaminedAI() {
            return this.contaminedAI;
        }

        public int GetContaminedPlayer() {
            return this.contaminedPlayer;
        }

        public int GetContaminedKilled() {
            return this.contaminedKilled;
        }
    }
}
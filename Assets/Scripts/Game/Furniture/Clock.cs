using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class Clock : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private AudioClip oneDongSound;
        [SerializeField] private AudioClip threeDongSound;

        [Header("Only for debug")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Animator animator;

        private void Awake() {
            this.audioSource = GetComponent<AudioSource>();
            this.animator = GetComponent<Animator>();
        }

        public void StartClock() {
            this.audioSource.PlayOneShot(this.oneDongSound);
            this.animator.SetTrigger("start");
        }

        public void StopClock() {
            this.audioSource.PlayOneShot(this.threeDongSound);
        }
    }
}
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Game.Weapons {
    public class PQGun : Weapon {
        [Header("Settings")]
        [SerializeField] private PQ pqAmmo;

        protected override void Shoot() {
            PhotonNetwork.Instantiate("Prefabs/Game/" + this.pqAmmo.name, this.spawnPos.position, this.transform.rotation);

            this.StartCoroutine(this.Reloading());
        }

        private IEnumerator Reloading() {
            this.renderer.enabled = false;
            yield return new WaitForSeconds(this.fireRate);
            this.renderer.enabled = true;
        }
    }
}

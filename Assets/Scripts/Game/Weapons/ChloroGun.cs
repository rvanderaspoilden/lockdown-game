using Photon.Pun;
using UnityEngine;

namespace Game.Weapons {
    public class ChloroGun : Weapon
    {
        private void Start() {
            this.maxAmmo = PhotonNetwork.CurrentRoom.PlayerCount - 1;
            this.currentAmmo = 10;
            this.UpdateAmmoText();
        }
        
        protected override void ConsumeAmmo() {
            base.ConsumeAmmo();
            
            this.UpdateAmmoText();
        }

        private void UpdateAmmoText() {
            this.displayScreenAmmoText.text = this.currentAmmo + "/" + this.maxAmmo;
            this.displayScreenAmmoText.color = this.currentAmmo > 0 ? Color.white : Color.red;
        }
    }
}

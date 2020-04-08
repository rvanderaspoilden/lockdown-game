using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Weapons {
    public class HydroGun : Weapon
    {
        private void Start() {
            this.UpdateAmmoText();
        }
        
        protected override void ConsumeAmmo() {
            base.ConsumeAmmo();
            
            this.UpdateAmmoText();
        }

        private void UpdateAmmoText() {
            float percent = (((float) this.currentAmmo / (float) this.maxAmmo) * 100f);
            this.displayScreenAmmoText.text = percent + "%";
            this.displayScreenAmmoText.color = percent > 0 ? Color.green : Color.red;
        }
    }    
}


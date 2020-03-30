using Photon.Pun;

namespace Game {
    public abstract class Interactable : MonoBehaviourPun {
        public abstract string GetInformation();

        public abstract void Interact();
    }
}
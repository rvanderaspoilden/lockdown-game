using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby {
    public class LobbyPlayer : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI usernameInputField;
        [SerializeField] private Image starImage;

        public void Setup(string username, bool showStar) {
            this.usernameInputField.text = username;
            this.starImage.enabled = showStar;
        }
    }
}
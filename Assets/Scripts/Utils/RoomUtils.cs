using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils {
    public class RoomUtils : MonoBehaviour {
        private static System.Random random = new System.Random();
        
        public static string CreateRoomId() {
            return "#" + random.Next(1, 9999);
        }
    }
}
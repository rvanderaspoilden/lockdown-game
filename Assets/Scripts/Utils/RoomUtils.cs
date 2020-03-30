using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils {
    public class RoomUtils : MonoBehaviour {
        private static System.Random random = new System.Random();
        
        public static string CreateRoomId() {
            return "#" + random.Next(1, 9999);
        }
        
        public static List<int> FillListAndShuffle(int quantity)
        {
            List<int> spawnNumbers = new List<int>();

            for (int i = 0; i < quantity; i++) {
                spawnNumbers.Add(i);
            }
            
            spawnNumbers.Sort((x, y) => random.Next(-1, 1));

            return spawnNumbers;
        }
        
        public static void SetLayerRecursively(GameObject go, int layerNumber)
        {
            foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = layerNumber;
            }
        }
    }
}
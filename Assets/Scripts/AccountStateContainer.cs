using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class AccountStateContainer : MonoBehaviour {

        public int TotalEmbers { get; set; }

        public static AccountStateContainer Instance;

        public void Awake() {
            if (Instance == null) {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            } else if (Instance != this) {
                Destroy(gameObject);
            }
        }
    }
}

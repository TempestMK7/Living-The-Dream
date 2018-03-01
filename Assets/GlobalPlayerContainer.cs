using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GlobalPlayerContainer : MonoBehaviour {

        public static GlobalPlayerContainer Instance;

        public PunTeams.Team PlayerTeam { get; set; }
        public bool IsWinner { get; set; }

        // Use this for initialization
        void Awake() {
            if (Instance == null) {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            } else if (Instance != this) {
                Destroy(gameObject);
            }
        }
    }
}

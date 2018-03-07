using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GlobalPlayerContainer : MonoBehaviour {

        public const int DREAMER = 0;
        public const int NIGHTMARE = 1;
        public const int OBSERVER = 2;

        public const int DOUBLE_JUMP_DREAMER = 0;
        public const int JETPACK_DREAMER = 1;

        public const int GHAST = 0;

        public const string IS_READY = "is_ready";
        public const string STATUS_READY = "Ready";
        public const string STATUS_NOT_READY = "Not Ready";

        public static GlobalPlayerContainer Instance;

        public int TeamSelection { get; set; }
        public int DreamerSelection { get; set; }
        public int NightmareSelection { set; get; }
        public bool IsWinner { get; set; }
        
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

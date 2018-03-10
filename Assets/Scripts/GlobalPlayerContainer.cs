using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GlobalPlayerContainer : MonoBehaviour {

        // TODO: make these enums.
        public const int EXPLORER = 0;
        public const int NIGHTMARE = 1;
        public const int OBSERVER = 2;

        public const int DOUBLE_JUMP_EXPLORER = 0;
        public const int JETPACK_EXPLORER = 1;

        public const int GHAST = 0;
        public const int CRYO = 1;

        public const string TEAM_SELECTION = "team_selection";
        public const string IS_READY = "is_ready";

        public const string STATUS_READY = "Ready";
        public const string STATUS_NOT_READY = "Not Ready";

        public const string GAME_VERSION = "0.03";

        public static GlobalPlayerContainer Instance;

        public int TeamSelection { get; set; }
        public int ExplorerSelection { get; set; }
        public int NightmareSelection { set; get; }
        public string IsReady { get; set; }
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

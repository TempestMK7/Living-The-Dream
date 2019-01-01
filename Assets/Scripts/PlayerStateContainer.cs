using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class PlayerStateContainer : MonoBehaviour {

        // TODO: make these enums.
        public const int EXPLORER = 0;
        public const int NIGHTMARE = 1;
        public const int OBSERVER = 2;

        public const int DOUBLE_JUMP_EXPLORER = 0;
        public const int JETPACK_EXPLORER = 1;

        public const int GHAST = 0;
        public const int CRYO = 1;
        public const int GOBLIN = 2;

        public const string TEAM_SELECTION = "team_selection";
        public const string IS_READY = "is_ready";

        public const string STATUS_READY = "Ready";
        public const string STATUS_NOT_READY = "Not Ready";

        public static PlayerStateContainer Instance;

        public int TeamSelection { get; set; }
        public int ExplorerSelection { get; set; }
        public int NightmareSelection { set; get; }

        public string IsReady { get; set; }
        public bool IsWinner { get; set; }

        public float ObjectiveEmbers { get; set; }
        public int RescueEmbers { get; set; }
        public int UpgradeEmbers { get; set; }
        public int VictoryEmbers { get; set; }
        
        public void Awake() {
            if (Instance == null) {
                DontDestroyOnLoad(gameObject);
                Instance = this;
                ResetInstance();
            } else if (Instance != this) {
                Destroy(gameObject);
            }
        }

        public int TotalEmbers() {
            return (int) ObjectiveEmbers + RescueEmbers + UpgradeEmbers + VictoryEmbers;
        }

        public static void ResetInstance() {
            Instance.IsReady = STATUS_NOT_READY;
            Instance.ObjectiveEmbers = 0;
            Instance.RescueEmbers = 0;
            Instance.UpgradeEmbers = 0;
            Instance.VictoryEmbers = 0;
        }
    }
}

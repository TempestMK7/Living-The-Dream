using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class MusicManagerBehavior : MonoBehaviour {

        public float MinimumTransitionTime = 5.0f;

        private GeneratedGameManager gameManager;

        private bool isExplorer;
        private int musicIntensity;
        private float lastTransition;
    
        public void Awake() {
            gameManager = FindObjectOfType<GeneratedGameManager>();
            isExplorer = PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER;
        }

        public void Update() {
            if (isExplorer) {
                SetMusicIntensityExplorer();
            } else {
                SetMusicIntensityNightmare();
            }
        }

        private void SetMusicIntensityExplorer() {
            if (gameManager.Explorer == null || gameManager.Nightmares == null || gameManager.Nightmares.Count == 0) return;
            Vector3 explorerPosition = gameManager.Explorer.transform.position;
            Vector3 closestNightmare = new Vector3(float.MaxValue, float.MaxValue);
            foreach (BaseNightmare nightmare in gameManager.Nightmares) {
                if (Vector3.Distance(explorerPosition, nightmare.transform.position) < Vector3.Distance(explorerPosition, closestNightmare)) {
                    closestNightmare = nightmare.transform.position;
                }
            }
        }

        private void SetMusicIntensityNightmare() {

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class MusicManagerBehavior : MonoBehaviour {

        public const int SAFE = 0;
        public const int DANGER = 1;
        public const int CHASE = 2;

        public float minimumTransitionTime = 5.0f;

        public AudioSource safeSource;
        public AudioSource dangerSource;
        public AudioSource chaseSource;

        private GeneratedGameManager gameManager;

        private bool isExplorer;
        private int musicIntensity;
        private float lastTransition;
    
        public void Awake() {
            gameManager = FindObjectOfType<GeneratedGameManager>();
            isExplorer = PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER;
            musicIntensity = SAFE;
            safeSource.volume = ControlBindingContainer.GetInstance().musicVolume;
            dangerSource.volume = 0f;
            chaseSource.volume = 0f;
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
            if (Time.time - lastTransition < minimumTransitionTime) return;
            Vector3 explorerPosition = gameManager.Explorer.transform.position;
            Vector3 closestNightmare = new Vector3(float.MaxValue, float.MaxValue);
            foreach (BaseNightmare nightmare in gameManager.Nightmares) {
                if (Vector3.Distance(explorerPosition, nightmare.transform.position) < Vector3.Distance(explorerPosition, closestNightmare)) {
                    closestNightmare = nightmare.transform.position;
                }
            }
            Vector3 difference = explorerPosition - closestNightmare;
            if (difference.magnitude > 30f) {
                SetMusicIntensity(SAFE);
            } else if (difference.magnitude > 10f) {
                SetMusicIntensity(DANGER);
            } else {
                SetMusicIntensity(CHASE);
            }
        }

        private void SetMusicIntensityNightmare() {
            if (gameManager.Nightmare == null || gameManager.Explorers == null || gameManager.Explorers.Count == 0) return;
            if (Time.time - lastTransition < minimumTransitionTime) return;
            Vector3 nightmarePosition = gameManager.Nightmare.transform.position;
            Vector3 closestExplorer = new Vector3(float.MaxValue, float.MaxValue);
            foreach (BaseExplorer explorer in gameManager.Explorers) {
                if (!explorer.IsDead() && Vector3.Distance(nightmarePosition, explorer.transform.position) < Vector3.Distance(nightmarePosition, closestExplorer)) {
                    closestExplorer = explorer.transform.position;
                }
            }
            Vector3 difference = nightmarePosition - closestExplorer;
            if (difference.magnitude > 30f) {
                SetMusicIntensity(SAFE);
            } else if (difference.magnitude > 10f) {
                SetMusicIntensity(DANGER);
            } else {
                SetMusicIntensity(CHASE);
            }
        }

        private void SetMusicIntensity(int intensity) {
            if (intensity != musicIntensity) {
                int oldIntensity = musicIntensity;
                musicIntensity = intensity;
                lastTransition = Time.time;
                StartCoroutine(ActivateSource(musicIntensity));
                StartCoroutine(DeactivateSource(oldIntensity));
            }
        }

        private IEnumerator ActivateSource(int intensity) {
            float startTime = Time.time;
            AudioSource source = null;
            switch (intensity) {
                case SAFE:
                    source = safeSource;
                    break;
                case DANGER:
                    source = dangerSource;
                    break;
                case CHASE:
                    source = chaseSource;
                    break;
            }
            if (source == null) {
                yield return null;
            } else {
                float volumeMultiplier = ControlBindingContainer.GetInstance().musicVolume;
                float volumePercentage = 0f;
                while (volumePercentage < 1f) {
                    volumePercentage = Mathf.Min(Time.time - startTime, 1f);
                    source.volume = volumePercentage * volumeMultiplier;
                    yield return null;
                }
            }
        }

        private IEnumerator DeactivateSource(int intensity) {
            float startTime = Time.time;
            AudioSource source = null;
            switch (intensity) {
                case SAFE:
                    source = safeSource;
                    break;
                case DANGER:
                    source = dangerSource;
                    break;
                case CHASE:
                    source = chaseSource;
                    break;
            }
            if (source == null) {
                yield return null;
            } else {
                float volumeMultiplier = ControlBindingContainer.GetInstance().musicVolume;
                float volumePercentage = 1f;
                while (volumePercentage > 0f) {
                    volumePercentage = 1f - Mathf.Min(Time.time - startTime, 1f);
                    source.volume = volumePercentage * volumeMultiplier;
                    yield return null;
                }
            }
        }
    }
}

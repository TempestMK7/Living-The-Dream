using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class AlertTextBehavior : MonoBehaviour {

        public float totalLongDuration = 6f;
        public float totalShortDuration = 3f;
        public float fadeDuration = 1f;
        public Text alertText;

        private float creationTime;
        private float nonFadeTime;

        public bool IsShortNotification { get; set; }

        // Use this for initialization
        void Awake() {
            creationTime = Time.time;
        }

        // Update is called once per frame
        void Update() {
            float totalDuration = IsShortNotification ? totalShortDuration : totalLongDuration;
            nonFadeTime = totalDuration - fadeDuration;
            if (Time.time - creationTime > totalDuration) {
                Destroy(gameObject);
                return;
            }

            if (Time.time - creationTime > nonFadeTime) {
                Material material = new Material(alertText.material);
                Color color = new Color(material.color.r, material.color.g, material.color.b);
                color.a = 1f - ((Time.time - creationTime - nonFadeTime) / fadeDuration);
                material.color = color;
                alertText.material = material;
            }
        }
    }
}

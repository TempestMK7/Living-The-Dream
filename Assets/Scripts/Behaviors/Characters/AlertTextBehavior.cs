using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class AlertTextBehavior : MonoBehaviour {

        public float totalDuration = 6f;
        public float fadeDuration = 1f;
        public Text alertText;

        private float creationTime;
        private float nonFadeTime;

        // Use this for initialization
        void Awake() {
            creationTime = Time.time;
            nonFadeTime = totalDuration - fadeDuration;
        }

        // Update is called once per frame
        void Update() {
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {
    public class LightBoxBehavior : MonoBehaviour {

        public bool IsMine { get; set; }
        public bool IsDead { get; set; }
        public bool IsActive { get; set; }
        public Vector3 DefaultScale { get; set; }
        public Vector3 ActiveScale { get; set; }

        private SpriteRenderer spriteRenderer;

        public void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Update is called once per frame
        void Update () {
            if (IsActive) {
                spriteRenderer.enabled = true;
                transform.localScale = ActiveScale;
            } else {
                bool amNightmare = PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE;
                spriteRenderer.enabled = IsMine || (IsDead && !amNightmare);
                transform.localScale = DefaultScale;
            }
        }
    }
}
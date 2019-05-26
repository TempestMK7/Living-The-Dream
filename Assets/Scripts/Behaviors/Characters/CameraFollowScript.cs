using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    [RequireComponent(typeof(Transform))]
    public class CameraFollowScript : Photon.PunBehaviour {

        public float damping = 0.05f;

        private Transform cameraTransform;
        private Transform playerTransform;

        private Vector3 lastPlayerPosition;

        public float ZOffset { get; set; }

        void Awake() {
            cameraTransform = Camera.main.transform;
            playerTransform = GetComponent<Transform>();
            ZOffset = (cameraTransform.position - playerTransform.position).z;
            lastPlayerPosition = playerTransform.position;
        }

        // Update is called once per frame
        void Update() {
            if (photonView.isMine) {
                if (cameraTransform == null) cameraTransform = Camera.main.transform;
                Vector3 currentVelocity = playerTransform.position - lastPlayerPosition;
                Vector3 newPos;
                if (Vector3.Distance(playerTransform.position, cameraTransform.localPosition) > 20f) {
                    newPos = playerTransform.position;
                } else {
                    newPos = Vector3.SmoothDamp(cameraTransform.localPosition, playerTransform.position, ref currentVelocity, damping);
                }
                newPos.z = ZOffset;
                cameraTransform.position = newPos;
                lastPlayerPosition = playerTransform.position;
            }
        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    [RequireComponent(typeof(BoxCollider2D))]
    public class NightmareBehavior : Photon.PunBehaviour, IPunObservable {

        public float maxSpeed = 10f;
        public float accelerationFactor = 0.5f;
        public float snapToMaxThresholdFactor = 0.1f;
        public float bounceThreshold = 1.0f;

        public float dashFactor = 2f;
        public float dashDuration = 0.1f;
        public float dashDamageDuration = 0.5f;
        public float dashCooldown = 1f;
        public float collisionDebounceTime = 1f;

        public float rayBoundShrinkage = 0.001f;
        public int numRays = 4;

        public LayerMask whatIsSolid;
        public LayerMask whatIsPlayer;

        private BoxCollider2D boxCollider;

        private Vector3 currentSpeed;
        private Vector3 currentControllerState;

        private bool facingRight;
        private float acceleration;
        private float snapToMaxThreshold;
        private float dashSpeed;
        private float dashStart;
        private float lastCollisionTime;

        // Use this for initialization
        void Awake () {
            boxCollider = GetComponent<BoxCollider2D>();
            currentSpeed = new Vector3();
            currentControllerState = new Vector3();
            acceleration = accelerationFactor * maxSpeed;
            snapToMaxThreshold = maxSpeed * snapToMaxThresholdFactor;
            dashSpeed = maxSpeed * dashFactor;
            dashStart = 0f;
            facingRight = false;
        }
	
	    // Update is called once per frame
	    void Update () {
            if (photonView.isMine && Time.time - dashStart > dashDuration) {
                // The angle at which we are travelling.
                float angle = Mathf.Atan2(currentControllerState.y, currentControllerState.x);
                // This is the speed we are accelerating towards.
                Vector3 newMax = new Vector3(Mathf.Cos(angle) * maxSpeed * Mathf.Abs(currentControllerState.x), Mathf.Sin(angle) * maxSpeed * Mathf.Abs(currentControllerState.y));
                // This is how far we are from that speed.
                Vector3 difference = newMax - currentSpeed;
                if (Mathf.Abs(difference.x) > snapToMaxThreshold) {
                    difference.x *= acceleration * Time.deltaTime;
                }
                if (Mathf.Abs(difference.y) > snapToMaxThreshold) {
                    difference.y *= acceleration * Time.deltaTime;
                }
                currentSpeed += difference;
            }

            // Calculate how far we're going.
            Vector3 distanceForFrame = currentSpeed * Time.deltaTime;
            bool goingRight = distanceForFrame.x > 0;
            bool goingUp = distanceForFrame.y > 0;

            // Declare everything we need for the ray casting process.
            Bounds currentBounds = boxCollider.bounds;
            currentBounds.Expand(rayBoundShrinkage * -1f);
            Vector3 topLeft = new Vector3(currentBounds.min.x, currentBounds.max.y);
            Vector3 bottomLeft = currentBounds.min;
            Vector3 bottomRight = new Vector3(currentBounds.max.x, currentBounds.min.y);
            bool hitX = false;
            bool hitY = false;

            // Use raycasts to decide if we hit anything horizontally.
            if (distanceForFrame.x != 0) {
                float rayInterval = (bottomRight.x - bottomLeft.x) / (float)numRays;
                Vector3 rayOriginBase = currentSpeed.x > 0 ? bottomRight : bottomLeft;
                float rayOriginCorrection = currentSpeed.x > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
                for (int x = 0; x <= numRays; x++) {
                    Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayOriginCorrection, rayOriginBase.y + rayInterval * (float)x);
                    RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, goingRight ? Vector3.right : Vector3.left, Mathf.Abs(distanceForFrame.x), whatIsSolid);
                    if (rayCast) {
                        hitX = true;
                        distanceForFrame.x = rayCast.point.x - rayOrigin.x;
                    }
                    if (distanceForFrame.x == 0f) break;
                }
            }
            if (hitX) {
                if (Mathf.Abs(currentSpeed.x) > maxSpeed * bounceThreshold) {
                    currentSpeed.x *= -1f;
                } else {
                    currentSpeed.x = 0;
                }
            }

            // Use raycasts to decide if we hit anything vertically.
            if (distanceForFrame.y != 0) {
                float rayInterval = (topLeft.y - bottomLeft.y) / (float)numRays;
                Vector3 rayOriginBase = currentSpeed.y > 0 ? topLeft : bottomLeft;
                float rayOriginCorrection = currentSpeed.y > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
                for (int x = 0; x <= numRays; x++) {
                    Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayInterval * (float)x, rayOriginBase.y + rayOriginCorrection);
                    RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, distanceForFrame.y > 0 ? Vector3.up : Vector3.down, Mathf.Abs(distanceForFrame.y), whatIsSolid);
                    if (rayCast) {
                        hitY = true;
                        distanceForFrame.y = rayCast.point.y - rayOrigin.y;
                    }
                    if (distanceForFrame.y == 0f) break;
                }
            }
            if (hitY) {
                if (Mathf.Abs(currentSpeed.y) > maxSpeed * bounceThreshold) {
                    currentSpeed.y *= -1f;
                } else {
                    currentSpeed.y = 0;
                }
            }

            // If our horizontal and vertical ray casts did not find anything, there could still be an object to our corner.
            if (!(hitY || hitX) && distanceForFrame.x != 0 && distanceForFrame.y != 0) {
                Vector3 rayOrigin = new Vector3(goingRight ? bottomRight.x : bottomLeft.x, goingUp ? topLeft.y : bottomLeft.y);
                float distance = Mathf.Sqrt(Mathf.Pow(distanceForFrame.x, 2f) + Mathf.Pow(distanceForFrame.y, 2f));
                RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, distanceForFrame, distance, whatIsSolid);
                if (rayCast) {
                    distanceForFrame.x = rayCast.point.x - rayOrigin.x;
                    distanceForFrame.y = rayCast.point.y - rayOrigin.y;
                }
            }

            // Actually move at long last.
            transform.position += distanceForFrame;
            // Decide whether or not to flip.
            goingRight = distanceForFrame.x > 0;
            if (distanceForFrame.x != 0 && goingRight != facingRight) {
                Flip();
            }
	    }

        public void Accelerate(float horizontalScale, float verticalScale) {
            currentControllerState = new Vector3(horizontalScale, verticalScale);
        }

        public void Dash() {
            if (Time.time - dashStart < dashCooldown || Time.time - lastCollisionTime < collisionDebounceTime) return;
            dashStart = Time.time;
            float angle = Mathf.Atan2(currentControllerState.y, currentControllerState.x);
            currentSpeed.x = Mathf.Cos(angle) * dashSpeed;
            currentSpeed.y = Mathf.Sin(angle) * dashSpeed;
        }

        private void Flip() {
            facingRight = !facingRight;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }

        public void OnTriggerEnter2D(Collider2D other) {
            if (!photonView.isMine) return;
            DreamerBehavior associatedBehavior = other.gameObject.GetComponent<DreamerBehavior>();
            if (associatedBehavior == null || associatedBehavior.OutOfHealth()) return;
            if (Time.time - dashStart < dashDamageDuration && Time.time - lastCollisionTime > collisionDebounceTime) {
                associatedBehavior.photonView.RPC("HandleCollision", PhotonTargets.All, photonView.ownerId, associatedBehavior.photonView.ownerId, currentSpeed);
                this.currentSpeed *= -1;
                lastCollisionTime = Time.time;
            }
        }

        public void OnTriggerStay2D(Collider2D other) {
            if (Time.time - lastCollisionTime > collisionDebounceTime) {
                OnTriggerEnter2D(other);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(transform.position);
                stream.SendNext(currentSpeed);
            } else {
                Vector3 networkPosition = (Vector3)stream.ReceiveNext();
                transform.position = (transform.position + networkPosition) / 2;
                currentSpeed = (Vector3)stream.ReceiveNext();
            }
        }
    }
}

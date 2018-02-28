using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    [RequireComponent(typeof(CircleCollider2D))]
    public class BonfireBehavior : Photon.PunBehaviour, IPunObservable {

        public float requiredCharges = 60f;
        public Sprite unlitSprite;
        public Sprite partLitSprite;
        public Sprite litSprite;
        public LayerMask whatIsPlayer;

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private float currentCharges;
        private bool playersNearby;

        // Use this for initialization
        void Awake () {
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            currentCharges = 0f;
	    }
	
	    // Update is called once per frame
	    void Update () {
            Collider2D[] otherPlayers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius, whatIsPlayer);
            playersNearby = otherPlayers.Length != 0;
            if (photonView.isMine == true && currentCharges < requiredCharges) {
                if (playersNearby == false) {
                    currentCharges -= Time.deltaTime;
                    currentCharges = Mathf.Max(currentCharges, 0f);
                } else {
                    currentCharges += Time.deltaTime * otherPlayers.Length;
                    currentCharges = Mathf.Min(currentCharges, requiredCharges);
                }
            }
            HandleSprite();
        }

        private void HandleSprite() {
            if (IsLit()) {
                spriteRenderer.sprite = litSprite;
            } else if (playersNearby) {
                spriteRenderer.sprite = partLitSprite;
            } else {
                spriteRenderer.sprite = unlitSprite;
            }
        }

        public Sprite GetCurrentSprite() {
            return spriteRenderer.sprite;
        }

        public bool IsLit() {
            return currentCharges >= requiredCharges;
        }

        public bool PlayersNearby() {
            return playersNearby;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(currentCharges);
            } else {
                currentCharges = (float)stream.ReceiveNext();
            }
        }
    }
}

    

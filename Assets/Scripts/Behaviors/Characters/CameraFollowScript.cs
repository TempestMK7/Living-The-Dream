using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class CameraFollowScript : Photon.PunBehaviour {

    public float damping = 0.05f;

    private Transform cameraTransform;
    private Transform playerTransform;

    private float zOffset;
    private Vector3 lastPlayerPosition;
    
    void Awake () {
        cameraTransform = Camera.main.transform;
        playerTransform = GetComponent<Transform>();
        zOffset = (cameraTransform.position - playerTransform.position).z;
        lastPlayerPosition = playerTransform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (photonView.isMine) {
            if (cameraTransform == null) cameraTransform = Camera.main.transform;
            Vector3 currentVelocity = playerTransform.position - lastPlayerPosition;
            Vector3 newPos = Vector3.SmoothDamp(cameraTransform.localPosition, playerTransform.position, ref currentVelocity, damping);
            newPos.z = zOffset;
            cameraTransform.position = newPos;
            lastPlayerPosition = playerTransform.position;
        }
	}
}

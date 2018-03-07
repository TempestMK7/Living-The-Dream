using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBoxBehavior : MonoBehaviour {

    public bool IsMine { get; set; }
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
            spriteRenderer.enabled = IsMine;
            transform.localScale = DefaultScale;
        }
	}
}

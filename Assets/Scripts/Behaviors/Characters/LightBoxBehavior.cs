using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBoxBehavior : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        transform.position = gameObject.GetComponentInParent<Transform>().position;
	}
}

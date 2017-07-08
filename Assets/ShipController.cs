using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {
    public float speed = 5.0f;

    private Vector3 realPosition;
    private Vector3 pixelPosition;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        realPosition += new Vector3(0.0f, Input.GetAxisRaw("Vertical"), Input.GetAxisRaw( "Horizontal" ) ) *Time.deltaTime*speed;
        pixelPosition = new Vector3( Mathf.RoundToInt( realPosition.x ), Mathf.RoundToInt( realPosition.y ), Mathf.RoundToInt( realPosition.z ) );
        transform.position = pixelPosition;
	}
}

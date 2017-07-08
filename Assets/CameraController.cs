using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Vector3 oldMousePos = Vector3.zero;
    private Transform m_Transform;
    private Transform cameraTransform;
    private Camera m_Camera;
    private Vector3 rot = Vector3.zero;
    private float zoom = -10.0f;
	// Use this for initialization
	void Start () {
        m_Transform = transform;
        cameraTransform = m_Transform.GetChild( 0 );
        m_Camera = cameraTransform.GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        if ( Input.GetMouseButtonDown( 1 ) ) {
            oldMousePos = Input.mousePosition;
            rot.x = m_Transform.localEulerAngles.y;
            rot.y = m_Transform.localEulerAngles.x;
        }
        if ( Input.GetMouseButton( 1 ) ) {
            Vector3 mouseDelta = Input.mousePosition - oldMousePos;
            rot.x += mouseDelta.x;
            rot.y += -mouseDelta.y;
            m_Transform.rotation = Quaternion.Euler( rot.y, rot.x, 0.0f );
            oldMousePos = Input.mousePosition;
        }
        if ( Input.GetMouseButtonDown( 2 ) ) {
            m_Camera.orthographic = !m_Camera.orthographic;
        }
        zoom += Input.mouseScrollDelta.y;
        cameraTransform.localPosition = new Vector3( 0f, 0f, zoom );
        m_Camera.orthographicSize -= Input.mouseScrollDelta.y*0.4f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode {
    NONE,
    EDITOR,
    PLAYING,
}

public class GameManager : MonoBehaviour {
    public ShipEditor editor;
    public ShipController shipController;

    public GameMode gameMode = GameMode.EDITOR;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if ( Input.GetKeyDown( KeyCode.Tab ) ) {
            if(gameMode == GameMode.EDITOR ) {
                editor.enabled = false;
                shipController.enabled = true;
                gameMode = GameMode.PLAYING;
            } else if ( gameMode == GameMode.PLAYING ) {
                editor.enabled = true;
                shipController.enabled = false;
                gameMode = GameMode.EDITOR;
            }
        }
	}
}

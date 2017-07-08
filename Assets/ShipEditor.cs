using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public struct VectorInt2 {
    public int x;
    public int y;

    public VectorInt2 ( int x, int y ) {
        this.x = x;
        this.y = y;
    }

    public static VectorInt2 operator +( VectorInt2 v1, VectorInt2 v2 ) {
        return new VectorInt2( v1.x + v2.x, v1.y + v2.y );
    }
}
[System.Serializable]
public struct VectorInt3 {
    public int x;
    public int y;
    public int z;

    public VectorInt3(int x, int y, int z ) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static VectorInt3 operator +( VectorInt3 v1, VectorInt3 v2 ) {
        return new VectorInt3( v1.x + v2.x, v1.y + v2.y, v1.z + v2.z );
    }
    public static VectorInt3 Clamp(VectorInt3 val, VectorInt3 min, VectorInt3 max ) {
        return new VectorInt3( Mathf.Clamp( val.x, min.x, max.x-1 ), Mathf.Clamp( val.y, min.y, max.y-1 ), Mathf.Clamp( val.z, min.z, max.z-1 ) );
    }
    public static VectorInt3 Loop ( VectorInt3 val, VectorInt3 min, VectorInt3 max ) {
        if ( val.x > max.x )
            val.x = min.x;
        if ( val.y > max.y )
            val.y = min.y;
        if ( val.z > max.z )
            val.z = min.z;

        if ( val.x < min.x )
            val.x = max.x;
        if ( val.y < min.y )
            val.y = max.y;
        if ( val.z < min.z )
            val.z = max.z;

        return val;
    }
}

[System.Serializable]
public struct Block {
    public int pieceID;
    public int materialID;
    public VectorInt3 rotation;
}

public class ShipEditor : MonoBehaviour {
    public string       fileName = "default";
    public int          gridSize = 16;
    public Transform    ship;
    public GameObject[] pieces;
    public Material[]   materials;

    public Transform    cursor;
    public VectorInt3   cursorPos;
    public VectorInt3   cursorRotation;

    public Transform grid;

    private Block[,,]     shipArray;
    private GameObject[,,] shipObjects  = new GameObject[16,16,16];
    private int         currentPiece    = 0;
    private int         currentMat      = 0;

    // Use this for initialization
    void Awake () {
        shipArray   = new Block[gridSize, gridSize, gridSize];
        ResetArray();
        shipObjects = new GameObject[gridSize, gridSize, gridSize];
        cursorPos = new VectorInt3( gridSize / 2, gridSize / 2, gridSize / 2 );
        grid.localPosition = new Vector3( 0f, -( gridSize / 2 ), 0.0f );
        MoveCursor( new VectorInt3( 0, 0, 0 ) );
	}
	
	// Update is called once per frame
	void Update () {
        UpdateCursor();
	}

    void OnDestroy ( ) {
       // SaveShip();
        Debug.Log( "Saved" );
    }

    void SaveShip ( ) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create (Application.persistentDataPath + "/" + fileName + ".spu");
        bf.Serialize( file, shipArray );
        file.Close();
        Debug.Log( "Saved" );
    }

    public void Load ( ) {
        Debug.Log( Application.persistentDataPath );
        if ( File.Exists( Application.persistentDataPath + "/" + fileName + ".spu" ) ) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + fileName + ".spu", FileMode.Open);
            shipArray = ( Block[,,] ) bf.Deserialize( file );
            file.Close();
        }
        if(shipArray.Length != gridSize * gridSize * gridSize ) {
            Debug.Log( "SIZE ERROR" );
            shipArray = new Block[gridSize, gridSize, gridSize];
            ResetArray();
        }
    }

    private void ResetArray ( ) {
        for ( int y = 0; y < gridSize; y++ ) {
            for ( int x = 0; x < gridSize; x++ ) {
                for ( int z = 0; z < gridSize; z++ ) {
                    shipArray[x, y, z].pieceID = -1;
                }
            }
        }
    }

    private void UpdateCursor ( ) {
        //MOVE
        if ( Input.GetKeyDown( KeyCode.UpArrow ) )
            MoveCursor( new VectorInt3( 0, 0, 1 ) );
        if ( Input.GetKeyDown( KeyCode.DownArrow ) )
            MoveCursor( new VectorInt3( 0, 0, -1 ) );
        if ( Input.GetKeyDown( KeyCode.LeftArrow ) )
            MoveCursor( new VectorInt3( -1, 0, 0 ) );
        if ( Input.GetKeyDown( KeyCode.RightArrow ) )
            MoveCursor( new VectorInt3( 1, 0, 0 ) );
        if ( Input.GetKeyDown( KeyCode.X ) )
            MoveCursor( new VectorInt3( 0, 1, 0 ) );
        if ( Input.GetKeyDown( KeyCode.Z ) )
            MoveCursor( new VectorInt3( 0, -1, 0 ) );

        //ROTATE
        if ( Input.GetKeyDown( KeyCode.Keypad8 ) )
            RotateCursor( new VectorInt3( 0, 1, 0 ) );
        if ( Input.GetKeyDown( KeyCode.Keypad2 ) )
            RotateCursor( new VectorInt3( 0, -1, 0 ) );
        if ( Input.GetKeyDown( KeyCode.Keypad4 ) )
            RotateCursor( new VectorInt3( -1, 0, 0 ) );
        if ( Input.GetKeyDown( KeyCode.Keypad6 ) )
            RotateCursor( new VectorInt3( 1, 0, 0 ) );        

        //ADD/REMOVE
        if ( Input.GetKeyDown( KeyCode.Space ) ) {
            AddBlock();
        }
        if ( Input.GetKeyDown( KeyCode.LeftControl ) ) {
            DeleteBlock();
        }

        //CHANGE PIECE
        if ( Input.GetKeyDown( KeyCode.Alpha1 ) ) {
            ChangePiece( 0 );
        }
        if ( Input.GetKeyDown( KeyCode.Alpha2 ) ) {
            ChangePiece( 1 );
        }
        if ( Input.GetKeyDown( KeyCode.Alpha3 ) ) {
            ChangePiece( 2 );
        }
        if ( Input.GetKeyDown( KeyCode.Alpha4 ) ) {
            ChangePiece( 3 );
        }
        if ( Input.GetKeyDown( KeyCode.Alpha5 ) ) {
            ChangePiece( 4 );
        }
        if ( Input.GetKeyDown( KeyCode.Period ) ) {
            ChangePiece( currentPiece + 1 );
        }
        if ( Input.GetKeyDown( KeyCode.Comma ) ) {
            ChangePiece( currentPiece - 1 );
        }
        //MATERIAL
        if ( Input.GetKeyDown( KeyCode.RightBracket ) ) {
            ChangeMaterial( currentMat + 1 );
        }
        if ( Input.GetKeyDown( KeyCode.LeftBracket ) ) {
            ChangeMaterial( currentMat - 1 );
        }

        //LOAD/SAVE
        if ( Input.GetKeyDown( KeyCode.L ) ) {
            Load();
            BuildShip();
        }
        if ( Input.GetKeyDown( KeyCode.S ) ) {
            SaveShip();
        }
    }
    private void ChangePiece(int newPiece ) {
        if(newPiece > cursor.childCount ) {
            newPiece = 0;
        }
        if ( newPiece < 0 ) {
            newPiece = cursor.childCount-1;
        }
        for (int i=0; i < cursor.childCount; i++ ) {
            if(i == newPiece ) {
                cursor.GetChild( i ).gameObject.SetActive( true );
                if ( cursor.GetChild( i ).GetComponent<Renderer>() ) {
                    cursor.GetChild( i ).GetComponent<Renderer>().sharedMaterial = materials[currentMat];
                }
            } else {
                cursor.GetChild( i ).gameObject.SetActive( false );
            }
        }
        currentPiece = newPiece;
    }
    private void ChangeMaterial ( int newMat ) {
        if ( newMat > materials.Length ) {
            newMat = 0;
        }
        if ( newMat < 0 ) {
            newMat = materials.Length - 1;
        }
        for ( int i = 0; i < cursor.childCount; i++ ) {
            if(cursor.GetChild( i ).gameObject.activeSelf ) {
                cursor.GetChild( i ).GetComponent<Renderer>().sharedMaterial = materials[newMat];
            }
        }
        currentMat = newMat;
    }
    private void MoveCursor ( VectorInt3 direction ) {
        cursorPos = VectorInt3.Clamp( cursorPos + direction, new VectorInt3(0, 0, 0), new VectorInt3(gridSize, gridSize, gridSize));

        cursor.localPosition = new Vector3( ( -( gridSize / 2 ) + 0.5f ) + cursorPos.x, ( -( gridSize / 2 ) + 0.5f ) + cursorPos.y, ( -( gridSize / 2 ) + 0.5f ) + cursorPos.z );
    }

    private void RotateCursor ( VectorInt3 direction ) {
        /*   cursorRotation += direction;
           cursorRotation = VectorInt3.Loop( cursorRotation, new VectorInt3( 0, 0, 0 ), new VectorInt3( 3, 3, 3 ) );
           cursor.localEulerAngles = new Vector3( cursorRotation.y * 90.0f, cursorRotation.x * 90.0f, 0.0f );*/
            cursor.Rotate( Vector3.up, 90f * direction.x, Space.World );
            cursor.Rotate( Vector3.right, 90f * direction.y, Space.World );
        Vector3 angles = cursor.localEulerAngles;
        cursorRotation = new VectorInt3( Mathf.RoundToInt( angles.x), Mathf.RoundToInt( angles.y), Mathf.RoundToInt( angles.z ) );
    }

    private void AddBlock ( ) {
        if ( shipArray[cursorPos.x, cursorPos.y, cursorPos.z].pieceID < 0 ) {
            shipArray[cursorPos.x, cursorPos.y, cursorPos.z].pieceID = currentPiece;
            shipArray[cursorPos.x, cursorPos.y, cursorPos.z].rotation = cursorRotation;
            shipArray[cursorPos.x, cursorPos.y, cursorPos.z].materialID = currentMat;
            GameObject go = GameObject.Instantiate( pieces[currentPiece] );
            go.transform.SetParent( ship );
            go.transform.localPosition = new Vector3( ( -( gridSize / 2 ) + 0.5f ) + cursorPos.x, ( -( gridSize / 2 ) + 0.5f ) + cursorPos.y, ( -( gridSize / 2 ) + 0.5f ) + cursorPos.z );
            go.transform.localRotation = cursor.localRotation;
            if ( go.GetComponent<Renderer>() ) {
                go.GetComponent<Renderer>().sharedMaterial = materials[currentMat];
            }

            shipObjects[cursorPos.x, cursorPos.y, cursorPos.z] = go;
        }
    }
    private void DeleteBlock ( ) {
        GameObject.Destroy( shipObjects[cursorPos.x, cursorPos.y, cursorPos.z] );
        shipObjects[cursorPos.x, cursorPos.y, cursorPos.z] = null;
        shipArray[cursorPos.x, cursorPos.y, cursorPos.z].pieceID = -1;

    }

    private void BuildShip ( ) {
        for ( int y = 0; y < gridSize; y++ ) {
            for ( int x = 0; x < gridSize; x++ ) {
                for ( int z = 0; z < gridSize; z++ ) {
                    if(shipArray[x,y,z].pieceID > -1 ) {
                        GameObject go = GameObject.Instantiate( pieces[shipArray[x,y,z].pieceID] );
                        go.transform.SetParent( ship );
                        go.transform.localPosition = new Vector3( ( -( gridSize / 2 ) + 0.5f ) + x, ( -( gridSize / 2 ) + 0.5f ) + y, ( -( gridSize / 2 ) + 0.5f ) + z );
                        go.transform.localEulerAngles = new Vector3( shipArray[x, y, z].rotation.x, shipArray[x, y, z].rotation.y, shipArray[x, y, z].rotation.z );

                        if ( go.GetComponent<Renderer>() ) {
                            go.GetComponent<Renderer>().sharedMaterial = materials[shipArray[x, y, z].materialID];
                        }
                        shipObjects[x, y, z] = go;
                    }
                }
            }
        }
    }
}

//
//  KeystonePlaneManager.cs
//
//  Created by Dimitris Doukas
//  Copyright 2014 doukasd.com
//

/* This script extends a ProceduralPlane with the ability to edit
 * the corner offsets using the keyboard and save and load the result.
 * 
 * This is particularly useful if that plane is used as the output
 * to a projector where you want to do digital keystoning.
 * 
 * CONTROLS
 * M: toggle edit mode (also saves the mapping each time it is turned off)
 * I: move top left corner
 * O: move top right corner
 * K: move bottom left corner
 * L: move bottom right corner
 * Arrow keys: move the current corner around
 * Shift: Hold down to speed up the moving
 */

using UnityEngine;
using System.Collections;

using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;

[RequireComponent (typeof(ProceduralPlane))]

public class Keystone : MonoBehaviour {

	[System.Serializable]
	public class KeystoneOffset {

		// use floats since Vector2 is not serializable
		public float topLeftOffsetX = 0f;
		public float topLeftOffsetY = 0f;
		public float topRightOffsetX = 0f;
		public float topRightOffsetY = 0f;
		public float bottomLeftOffsetX = 0f;
		public float bottomLeftOffsetY = 0f;
		public float bottomRightOffsetX = 0f;
		public float bottomRightOffsetY = 0f;

		private const string FILENAME = "/keystone.map";

		public void Save() {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create(Application.persistentDataPath + FILENAME);
			bf.Serialize(file, this);
			file.Close();
		}

		public static KeystoneOffset Load() {
			Debug.Log("path = " + Application.persistentDataPath + FILENAME);
			KeystoneOffset keystone = new KeystoneOffset();
			if(File.Exists(Application.persistentDataPath + FILENAME)) {
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Application.persistentDataPath + FILENAME, FileMode.Open);
				keystone = (KeystoneOffset)bf.Deserialize(file);
				file.Close();
				Debug.Log("Keystone loaded");
			}
			return keystone;
		}

		// Vector2 public interface
		public Vector2 TopLeftOffset {
			get { return new Vector2(topLeftOffsetX, topLeftOffsetY);}
			set {
				topLeftOffsetX = value.x;
				topLeftOffsetY = value.y;
			}
		}
		public Vector2 TopRightOffset {
			get { return new Vector2(topRightOffsetX, topRightOffsetY); }
			set {
				topRightOffsetX = value.x;
				topRightOffsetY = value.y;
			}
		}
		public Vector2 BottomLeftOffset {
			get { return new Vector2(bottomLeftOffsetX, bottomLeftOffsetY); }
			set {
				bottomLeftOffsetX = value.x;
				bottomLeftOffsetY = value.y;
			}
		}
		public Vector2 BottomRightOffset {
			get { return new Vector2(bottomRightOffsetX, bottomRightOffsetY); }
			set {
				bottomRightOffsetX = value.x;
				bottomRightOffsetY = value.y;
			}
		}
	}

	private KeystoneOffset keystone;

	private bool editing = false;
	private int currentCorner = 0;
	private ProceduralPlane plane;

	private const float OFFSET_UNIT = 0.001f;

	// Use this for initialization
	void Start () {
		plane = this.gameObject.GetComponent<ProceduralPlane>();
		//keystone = new KeystoneOffset(); // LOAD
		keystone = KeystoneOffset.Load();

		// apply stored (serialized) offsets
		plane.topLeftOffset = keystone.TopLeftOffset;
		plane.topRightOffset = keystone.TopRightOffset;
		plane.bottomLeftOffset = keystone.BottomLeftOffset;
		plane.bottomRightOffset = keystone.BottomRightOffset;
		plane.Rebuild();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.M)) {
			EditMode(!editing);
		}

		if(editing) {
			// mapping speed
			float speedUp = 1f;
			if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				speedUp = 5f;
			}

			// select corner
			if(Input.GetKeyDown(KeyCode.I)) {
				currentCorner = 0;
			}
			if(Input.GetKeyDown(KeyCode.O)) {
				currentCorner = 1;
			}
			if(Input.GetKeyDown(KeyCode.K)) {
				currentCorner = 2;
			}
			if(Input.GetKeyDown(KeyCode.L)) {
				currentCorner = 3;
			}

			// move
			if(Input.GetKey(KeyCode.LeftArrow)) {
				MoveCorner(currentCorner, new Vector2(-OFFSET_UNIT * speedUp, 0f));
			}
			if(Input.GetKey(KeyCode.RightArrow)) {
				MoveCorner(currentCorner, new Vector2(OFFSET_UNIT * speedUp, 0f));
			}
			if(Input.GetKey(KeyCode.UpArrow)) {
				MoveCorner(currentCorner, new Vector2(0f, OFFSET_UNIT * speedUp));
			}
			if(Input.GetKey(KeyCode.DownArrow)) {
				MoveCorner(currentCorner, new Vector2(0f, -OFFSET_UNIT * speedUp));
			}
		}
	}

	void OnGUI() {
		if(editing) {
			GUI.Label(new Rect(10f, 10f, 300f, 30f), "Editing " + CornerName(currentCorner) + " corner");
		}
	}

	private string CornerName(int corner) {
		switch (corner) {
		case 0:
			return "Top Left";
		case 1:
			return "Top Right";
		case 2:
			return "Bottom Left";
		case 3:
			return "Bottom Right";
		default:
			return "INVALID";
		}
	}

	private void MoveCorner(int corner, Vector2 offset) {
		switch (corner) {
		case 0:
			keystone.TopLeftOffset += offset;
			plane.topLeftOffset += offset;
			break;
		case 1:
			keystone.TopRightOffset += offset;
			plane.topRightOffset += offset;
			break;
		case 2:
			keystone.BottomLeftOffset += offset;
			plane.bottomLeftOffset += offset;
			break;
		case 3:
			keystone.BottomRightOffset += offset;
			plane.bottomRightOffset += offset;
			break;
		default:
			Debug.LogError("Invalid KeystoneCorner specified");
			break;
		}
		plane.Rebuild();
	}

	public void EditMode(bool enable) {
		editing = enable;

		// set the camera background to something visible when editing and invisible when not
		Camera.main.backgroundColor = enable ? Color.green : Color.black;

		// save mapping when editing is disabled
		if(!enable) {
			keystone.Save();
		}
	}

}

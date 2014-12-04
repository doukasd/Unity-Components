//
//  ProceduralPlaneEditor.cs
//
//  Created by Dimitris Doukas
//  Copyright 2014 doukasd.com
//

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(ProceduralPlane))] 

public class ProceduralPlaneEditor : Editor {
	[MenuItem ("GameObject/Create Other/Procedural/Plane")]
	
	static void Create(){
		GameObject gameObject = new GameObject("ProceduralPlane");
		ProceduralPlane c = gameObject.AddComponent<ProceduralPlane>();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		c.Rebuild();
		c.AssignDefaultShader();
	}
	
	public override void OnInspectorGUI ()
		{
			ProceduralPlane obj;					
			obj = target as ProceduralPlane;
			if (obj == null) {
				return;
			}
			
			base.DrawDefaultInspector();			
			if (GUI.changed){
				obj.Rebuild();
			}

			if(GUILayout.Button("Rebuild"))
			{
				obj.Rebuild();
			}
		}
}


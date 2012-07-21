//
//  ProceduralCylinderEditor.cs
//
//  Created by Dimitris Doukas.
//  Copyright 2012 doukasd.com. All rights reserved.
//

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (ProceduralCylinder))] 

public class ProceduralCylinderEditor : Editor {
	[MenuItem ("GameObject/Create Other/Procedural/Cylinder")]
	
	static void Create(){
		GameObject gameObject = new GameObject("ProceduralCylinder");
		ProceduralCylinder c = gameObject.AddComponent<ProceduralCylinder>();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		c.Rebuild();
		c.AssignDefaultShader();
	}
	
	public override void OnInspectorGUI ()
		{
			ProceduralCylinder obj;					
			obj = target as ProceduralCylinder;
			if (obj == null) {
				return;
			}
			
			base.DrawDefaultInspector();			
			if (GUI.changed){
				obj.Rebuild();
			}
		}
}


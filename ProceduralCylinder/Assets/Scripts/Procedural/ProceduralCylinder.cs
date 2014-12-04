//
//  ProceduralCylinder.cs
//
//  Created by Dimitris Doukas.
//  Copyright 2012 doukasd.com. All rights reserved.
//

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]

public class ProceduralCylinder : MonoBehaviour {
	
	//constants
	private const int DEFAULT_RADIAL_SEGMENTS = 8;
	private const int DEFAULT_HEIGHT_SEGMENTS = 2;
	private const int MIN_RADIAL_SEGMENTS = 3;
	private const int MIN_HEIGHT_SEGMENTS = 1;
	private const float DEFAULT_RADIUS = 0.5f;
	private const float DEFAULT_HEIGHT = 1.0f;
	
	//public variables
	public int radialSegments = DEFAULT_RADIAL_SEGMENTS;
	public int heightSegments = DEFAULT_HEIGHT_SEGMENTS;
	
	//private variables
	private Mesh modelMesh;
	private MeshFilter meshFilter;
	private int numVertexColumns, numVertexRows;	//columns and rows of vertices
	private float radius = DEFAULT_RADIUS;
	private float length = DEFAULT_HEIGHT;
	
	public void AssignDefaultShader()
	{		
		//assign it a white Diffuse shader, it's better than the default magenta
		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
		meshRenderer.sharedMaterial.color = Color.white;
	}
	
	public void Rebuild ()
	{
		//create the mesh
		modelMesh = new Mesh();
		modelMesh.name = "ProceduralCylinderMesh";
		meshFilter = (MeshFilter)gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = modelMesh;
		
		//sanity check
		if(radialSegments < MIN_RADIAL_SEGMENTS)	radialSegments = MIN_RADIAL_SEGMENTS;
		if(heightSegments < MIN_HEIGHT_SEGMENTS)	heightSegments = MIN_HEIGHT_SEGMENTS;
		
		//calculate how many vertices we need
		numVertexColumns = radialSegments + 1;	//+1 for welding
		numVertexRows = heightSegments + 1;
		
		//calculate sizes
		int numVertices = numVertexColumns * numVertexRows;
		int numUVs = numVertices;									//always
		int numSideTris = radialSegments * heightSegments * 2;		//for one cap
		int numCapTris = radialSegments - 2;						//fact
		int trisArrayLength = (numSideTris  + numCapTris * 2) * 3;	//3 places in the array for each tri
		
		//optional: log the number of tris
		//Debug.Log ("CustomCylinder has " + trisArrayLength/3 + " tris");
		
		//initialize arrays
		Vector3[] Vertices = new Vector3[ numVertices ];
		Vector2[] UVs = new Vector2[ numUVs ];
		int[] Tris = new int[ trisArrayLength ];
		
		//precalculate increments to improve performance
		float heightStep = length / heightSegments;
		float angleStep = 2 *Mathf.PI / radialSegments;
		float uvStepH = 1.0f/radialSegments;
		float uvStepV = 1.0f/heightSegments;
		
		for(int j=0; j<numVertexRows; j++)
		{			
			for(int i=0; i<numVertexColumns; i++)
			{
				//calculate angle for that vertex on the unit circle
				float angle = i * angleStep;
				
				//"fold" the sheet around as a cylinder by placing the first and last vertex of each row at the same spot
				if(i == numVertexColumns - 1){
					angle = 0;
				}
				
				//position current vertex
				Vertices[j*numVertexColumns + i] = new Vector3(radius * Mathf.Cos(angle), j * heightStep, radius * Mathf.Sin(angle));
				
				//calculate UVs
				UVs[j*numVertexColumns + i] = new Vector2( i * uvStepH, j * uvStepV );
				
				//create the tris				
				if(j==0 || i >= numVertexColumns - 1){
					//nothing to do on the first and last "floor" on the tris, capping is done below
					//also nothing to do on the last column of vertices
					continue;
				}
				else {
					//create 2 tris below each vertex
					//6 seems like a magic number. For every vertex we draw 2 tris in this for-loop, therefore we need 2*3=6 indices in the Tris array
					//offset the base by the number of slots we need for the bottom cap tris. Those will be populated once we draw the cap
					int baseIndex = numCapTris * 3 + (j-1)*radialSegments*6 + i*6;
					
					//1st tri - below and in front
					Tris[baseIndex + 0] = j*numVertexColumns + i;
					Tris[baseIndex + 1]	= j*numVertexColumns + i + 1;
					Tris[baseIndex + 2]	= (j-1)*numVertexColumns + i;
					
					//2nd tri - the one it doesn't touch
					Tris[baseIndex + 3] = (j-1)*numVertexColumns + i;
					Tris[baseIndex + 4]	= j*numVertexColumns + i + 1;
					Tris[baseIndex + 5]	= (j-1)*numVertexColumns + i + 1;
				}
			}
		}
		
		//draw caps
		bool leftSided = true;
		int leftIndex = 0;
		int rightIndex = 0;
		int middleIndex = 0;
		int topCapVertexOffset = numVertices - numVertexColumns;
		for(int i=0; i<numCapTris; i++) {
			int bottomCapBaseIndex = i*3;
			int topCapBaseIndex = (numCapTris + numSideTris) * 3 + i*3;

			if(i==0) {
				middleIndex = 0;
				leftIndex = 1;
				rightIndex = numVertexColumns - 2;
				leftSided = true;
			}
			else if(leftSided){
				middleIndex = rightIndex;
				rightIndex--;
			}
			else {
				middleIndex = leftIndex;
				leftIndex++; 
			}
			leftSided = !leftSided;
			
			//assign bottom tris
			Tris[bottomCapBaseIndex + 0] = rightIndex;
			Tris[bottomCapBaseIndex + 1] = middleIndex;
			Tris[bottomCapBaseIndex + 2] = leftIndex;
			
			//assign top tris
			Tris[topCapBaseIndex + 0] = topCapVertexOffset + leftIndex;
			Tris[topCapBaseIndex + 1] = topCapVertexOffset + middleIndex;
			Tris[topCapBaseIndex + 2] = topCapVertexOffset + rightIndex;
		}
		
		//assign vertices, uvs and tris
		modelMesh.vertices = Vertices;
		modelMesh.uv = UVs;
		modelMesh.triangles = Tris;
		
		modelMesh.RecalculateNormals();
		modelMesh.RecalculateBounds();
		calculateMeshTangents(modelMesh);	
	}
	
	// Recalculate mesh tangents
	// I found this on the internet (Unity forums?), I don't take credit for it.
	
	void calculateMeshTangents(Mesh mesh)
	{
			
	    //speed up math by copying the mesh arrays
	    int[] triangles = mesh.triangles;
	    Vector3[] vertices = mesh.vertices;
	    Vector2[] uv = mesh.uv;
	    Vector3[] normals = mesh.normals;
	
	    //variable definitions
	    int triangleCount = triangles.Length;
	    int vertexCount = vertices.Length;
	
	    Vector3[] tan1 = new Vector3[vertexCount];
	    Vector3[] tan2 = new Vector3[vertexCount];
	
	    Vector4[] tangents = new Vector4[vertexCount];
	
	    for (long a = 0; a < triangleCount; a += 3)
	    {
	        long i1 = triangles[a + 0];
	        long i2 = triangles[a + 1];
	        long i3 = triangles[a + 2];
	
	        Vector3 v1 = vertices[i1];
	        Vector3 v2 = vertices[i2];
	        Vector3 v3 = vertices[i3];
	
	        Vector2 w1 = uv[i1];
	        Vector2 w2 = uv[i2];
	        Vector2 w3 = uv[i3];
	
	        float x1 = v2.x - v1.x;
	        float x2 = v3.x - v1.x;
	        float y1 = v2.y - v1.y;
	        float y2 = v3.y - v1.y;
	        float z1 = v2.z - v1.z;
	        float z2 = v3.z - v1.z;
	
	        float s1 = w2.x - w1.x;
	        float s2 = w3.x - w1.x;
	        float t1 = w2.y - w1.y;
	        float t2 = w3.y - w1.y;
	
	        float r = 1.0f / (s1 * t2 - s2 * t1);
	
	        Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
	        Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
	
	        tan1[i1] += sdir;
	        tan1[i2] += sdir;
	        tan1[i3] += sdir;
	
	        tan2[i1] += tdir;
	        tan2[i2] += tdir;
	        tan2[i3] += tdir;
	    }
	
	    for (long a = 0; a < vertexCount; ++a)
	    {
	        Vector3 n = normals[a];
	        Vector3 t = tan1[a];
	
	        //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
	        //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
	        Vector3.OrthoNormalize(ref n, ref t);
	        tangents[a].x = t.x;
	        tangents[a].y = t.y;
	        tangents[a].z = t.z;
	
	        tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
	    }
	
	    mesh.tangents = tangents;
	}
		
}


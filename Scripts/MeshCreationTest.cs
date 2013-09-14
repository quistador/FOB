using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshCreationTest : MonoBehaviour {

    public Material material;
    // Use this for initialization
    void Start () 
    {

    }

    // Update is called once per frame
    void Update () 
    {
        List<Vector3> vertList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();

        List<int> triangles = new List<int>();

        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();

        if(meshFilter == null)
        {
            meshFilter = this.gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
        }

        MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();

        if(meshRenderer == null)
        {
            meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        }


        Mesh mesh = meshFilter.sharedMesh;
        float edgeSize = 1f;
        int planeSize = 10;

        //create a planeSize*planeSize square grid of vertices.
        for(int i = 0; i < planeSize; i++)
        {
            for(int j = 0; j < planeSize; j++)
            {
                vertList.Add(new Vector3(edgeSize*i, edgeSize*j,0));
            }
        }

        //create the triangles from the vertices. 
        for(int i = 0; i < planeSize - 1; i++)
        {
            for(int j = 0; j < planeSize - 1; j++)
            {
                // forms the:
                //
                // t1--- t3
                // |    /
                // |  /
                // |/
                // t2
                //
                // portion of the square
                int t1,t2,t3;
                t1 = planeSize*i + j;
                t2 = planeSize*(i+1) + j;
                t3 = planeSize*i + j + 1;

                triangles.Add (t1);
                triangles.Add (t2);
                triangles.Add (t3);

                // forms the:
                //
                //        p1
                //       /|
                //     /  |
                //   /    |
                // p2-----p3
                //
                // portion of the square
                int p1,p2,p3;
                p1 = t3;
                p2 = t2;
                p3 = t2 + 1;

                triangles.Add (p1);
                triangles.Add (p2);
                triangles.Add (p3);
            }
        }

        for(int i = 0; i < vertList.Count; i++)
        {
            uvList.Add( new Vector2(vertList[i].x, vertList[i].y ) );
        }

        meshRenderer.material = material;
        mesh.vertices = vertList.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvList.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }
}

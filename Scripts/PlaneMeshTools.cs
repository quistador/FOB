using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaneMeshTools
{
    public static Mesh CreatePlane(
            float lengthX,
            float lengthY,
            int slicesX, 
            int slicesY, 
            Material material, 
            GameObject gameObject)
    {
        if(slicesX <= 0 || slicesY <= 0)
        {
            throw new System.ArgumentException("need a minimum of two slices, otherwise we can't define a poly face!");
        }

        List<Vector3> vertList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
        
        List<int> triangles = new List<int>();
        
        MeshFilter meshFilter = gameObject.gameObject.GetComponent<MeshFilter>();
        
        if(meshFilter == null)
        {
            meshFilter = gameObject.gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
        }
        
        MeshRenderer meshRenderer = gameObject.gameObject.GetComponent<MeshRenderer>();

        if(meshRenderer == null)
        {
            meshRenderer = gameObject.gameObject.AddComponent<MeshRenderer>();
        }

        Mesh mesh = meshFilter.sharedMesh;
        float edgeSizeX = lengthX/(float)(slicesX - 1);
        float edgeSizeY = lengthY/(float)(slicesY - 1);
        
        //create a slicesX*slicesY square grid of vertices.
        for(int row = 0; row < slicesY; row++)
        {
            for(int column = 0; column < slicesX; column++)
            {
                vertList.Add(new Vector3(
                    edgeSizeX*column, 
                    edgeSizeY*row,
                    gameObject.transform.position.z));
            }
        }
        
        //create the triangles from the vertices. 
        for(int row = 0; row < slicesY - 1; row++)
        {
            for(int column = 0; column < slicesX - 1; column++)
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
                t1 = slicesX*row + column;
                t2 = slicesX*(row+1) + column;
                t3 = slicesX*row + column + 1;
                
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

        //Debug.Log(string.Format("edgeX: {0}, edgeY: {1}, output vertCount: {2}, output triangleCount: {3} ", slicesX, slicesY, vertList.Count, triangles.Count)); 
        //Debug.Log(string.Format("gameObject xPos: {0}, firstVert xPos: {1}", gameObject.transform.position.x, mesh.vertices[0].x));
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
            
        return mesh;
    }
}

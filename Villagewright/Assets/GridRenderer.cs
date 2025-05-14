using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

[ExecuteAlways]
public class GridRenderer : MonoBehaviour
{
    public Material lineMaterial;
    public float gridSize = 1f;
    public int gridExtent = 10;
    public float gridHeight = 0f;
    public Color gridColor = Color.white;

    void OnRenderObject()
    {
        if(!lineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            
        }
        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(gridColor);
       
        

        for (int x = -gridExtent; x <= gridExtent; x++)
        {
            GL.Vertex(new Vector3(transform.position.x + (x * gridSize) + (gridSize / 2), gridHeight, transform.position.z + (-gridExtent * gridSize)));
            GL.Vertex(new Vector3(transform.position.x + (x * gridSize) + (gridSize / 2), gridHeight, transform.position.z + (gridExtent * gridSize)));
           
        }

        for (int z = -gridExtent; z <= gridExtent; z++)
        {
            GL.Vertex(new Vector3(transform.position.x + (-gridExtent * gridSize), gridHeight, transform.position.z + (z * gridSize) + (gridSize / 2)));
            GL.Vertex(new Vector3(transform.position.x + (gridExtent * gridSize), gridHeight, transform.position.z + (z * gridSize) + (gridSize / 2)));
        }

        GL.End();
    }
}


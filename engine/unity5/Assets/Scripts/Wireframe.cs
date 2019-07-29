using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wireframe : MonoBehaviour
{

    public bool render_mesh_normaly = true;
    public bool render_lines_1st = true;
    public bool render_lines_2nd = false;
    public bool render_lines_3rd = false;
    public Color lineColor = new Color(0.0f, 1.0f, 1.0f);
    public Color backgroundColor = new Color(0.0f, 0.5f, 0.5f);
    public bool ZWrite = true;
    public bool AWrite = true;
    public bool blend = true;
    public float lineWidth = 3;
    public int size = 0;

    private Vector3[] lines;
    private ArrayList lines_List;
    public Material lineMaterial;
    private Vector3 offset;
    //private MeshRenderer meshRenderer; 

    /*
    ████████       ▄▀▀■  ▀▀█▀▀  ▄▀▀▄  █▀▀▄  ▀▀█▀▀ 
    ████████       ▀■■▄    █    █■■█  █▀▀▄    █   
    ████████       ■▄▄▀    █    █  █  █  █    █   
    */


    void Start()
    {
        //meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (lineMaterial == null)
        {
            lineMaterial = new Material (Shader.Find("Sprites/Default"));
 
        }

        
    }

    public void Init(Mesh mesh, Vector3 offset)
    {
        this.offset = offset;
        lines_List = new ArrayList();
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            lines_List.Add(vertices[triangles[i]]);
            lines_List.Add(vertices[triangles[i + 1]]);
            lines_List.Add(vertices[triangles[i + 2]]);
        }

        //lines_List.CopyTo(lines);//arrays are faster than array lists
        lines = (Vector3[])lines_List.ToArray(typeof(Vector3));
        lines_List.Clear();//free memory from the arraylist
        size = lines.Length;
    }

    // to simulate thickness, draw line as a quad scaled along the camera's vertical axis.
    void DrawQuad(Vector3 p1, Vector3 p2)
    {
        float thisWidth = 1.0f / Screen.width * lineWidth * 0.5f;
        Vector3 edge1 = Camera.main.transform.position - (p2 + p1) / 2.0f;    //vector from line center to camera
        Vector3 edge2 = p2 - p1;    //vector from point to point
        Vector3 perpendicular = Vector3.Cross(edge1, edge2).normalized * thisWidth;

        GL.Vertex(p1 - perpendicular);
        GL.Vertex(p1 + perpendicular);
        GL.Vertex(p2 + perpendicular);
        GL.Vertex(p2 - perpendicular);
    }

    Vector3 to_world(Vector3 vec)
    {
        return gameObject.transform.TransformPoint(vec - offset) ;
    }

    /*
    ████████       █  █  █▀▀▄  █▀▀▄  ▄▀▀▄  ▀▀█▀▀  █▀▀▀ 
    ████████       █  █  █▀▀   █  █  █■■█    █    █■■  
    ████████       ▀▄▄▀  █     █▄▄▀  █  █    █    █▄▄▄ 
    */


    void OnRenderObject()
    {
       // gameObject.renderer.enabled = render_mesh_normaly;
        if (lines == null || lines.Length < lineWidth)
        {
            print("No lines");
        }
        else
        {
            lineMaterial.SetPass(0);
            GL.Color(lineColor);

            if (lineWidth == 1)
            {
                GL.Begin(GL.LINES);
                for (int i = 0; i + 2 < lines.Length; i += 3)
                {
                    Vector3 vec1 = to_world(lines[i]);
                    Vector3 vec2 = to_world(lines[i + 1]);
                    Vector3 vec3 = to_world(lines[i + 2]);
                    if (render_lines_1st)
                    {
                        GL.Vertex(vec1);
                        GL.Vertex(vec2);
                    }
                    if (render_lines_2nd)
                    {
                        GL.Vertex(vec2);
                        GL.Vertex(vec3);
                    }
                    if (render_lines_3rd)
                    {
                        GL.Vertex(vec3);
                        GL.Vertex(vec1);
                    }
                }
            }
            else
            {
                GL.Begin(GL.QUADS);
                for (int i = 0; i + 2 < lines.Length; i += 3)
                {
                    Vector3 vec1 = to_world(lines[i]);
                    Vector3 vec2 = to_world(lines[i + 1]);
                    Vector3 vec3 = to_world(lines[i + 2]);
                    if (render_lines_1st) DrawQuad(vec1, vec2);
                    if (render_lines_2nd) DrawQuad(vec2, vec3);
                    if (render_lines_3rd) DrawQuad(vec3, vec1);
                }
            }
            GL.End();
        }
    }
}
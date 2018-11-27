using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GTMT
{
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class HexMesh : MonoBehaviour
    {
        /* Public Fields [set through prefab] */
        [SerializeField]
        private bool useCollider;
        

        /* Private Fields */
        private List<Vector3> m_vertices;
        private List<int> m_triangles;
        private List<Vector2> m_uv;
        private List<Color> m_colors;

        private Mesh m_mesh;
        private MeshCollider m_meshCollider;


        /* Awake */
        private void Awake()
        {
            m_mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            m_mesh.name = "Hex Mesh";
            m_mesh.MarkDynamic();   // TODO check if this is an okay thing to do

            // Only add collider if needed
            if (useCollider)
            {
                m_meshCollider = gameObject.AddComponent<MeshCollider>();
            }        
        }


        /* Clear Mesh */
        public void Clear()
        {
            m_mesh.Clear();

            m_vertices = HexObjectPool<Vector3>.Get();
            m_triangles = HexObjectPool<int>.Get();
            m_colors = HexObjectPool<Color>.Get();
            m_uv = HexObjectPool<Vector2>.Get();
        }


        /* Apply Changes to mesh */
        public void Apply()
        {
            m_mesh.SetVertices(m_vertices);
            HexObjectPool<Vector3>.Relese(m_vertices);

            m_mesh.SetTriangles(m_triangles, 0);
            HexObjectPool<int>.Relese(m_triangles);

            m_mesh.SetUVs(0, m_uv);
            HexObjectPool<Vector2>.Relese(m_uv);

            m_mesh.SetColors(m_colors);
            HexObjectPool<Color>.Relese(m_colors);

            m_mesh.RecalculateNormals();
            m_mesh.RecalculateBounds();         // TODO see if these two are necessary
            m_mesh.RecalculateTangents();
            //m_mesh.UploadMeshData();          // TODO see if this makes sense

            if (useCollider)
            {
                m_meshCollider.sharedMesh = m_mesh;
            }
           
        }


        /* Add Traingles / Quads / Colors */
        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_vertices.Count;

            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);

            m_triangles.Add(vertexIndex++);
            m_triangles.Add(vertexIndex++);
            m_triangles.Add(vertexIndex);
        }

        public void AddTriangleColor(Color c)
        {
            m_colors.Add(c);
            m_colors.Add(c);
            m_colors.Add(c);
        }

        public void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
        }

        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            int vertexIndex = m_vertices.Count;

            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);
            m_vertices.Add(v4);

            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 3);
        }

        public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
        {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
            m_colors.Add(c4);
        }

        public void AddQuadColor(Color c1, Color c2)
        {
            m_colors.Add(c1);
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c2);
        }

    }
}


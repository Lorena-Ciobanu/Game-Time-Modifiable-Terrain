﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GTMT
{
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class HexMesh : MonoBehaviour
    {
        private static List<Vector3> m_vertices = new List<Vector3>();
        private static List<int> m_triangles = new List<int>();
        private static List<Vector2> m_uv = new List<Vector2>();
        private static List<Color> m_colors = new List<Color>();

        private Mesh m_mesh;
        private MeshRenderer m_meshRenderer;
        private MeshCollider m_meshCollider;


        /* Awake */
        private void Awake()
        {
            m_mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            m_mesh.name = "Hex Mesh";
            m_mesh.MarkDynamic();   // TODO check if this is an okay thing to do

            m_meshRenderer = GetComponent<MeshRenderer>();

     //       m_vertices = new List<Vector3>();
    //        m_uv = new List<Vector2>();
     //       m_triangles = new List<int>();

     //       m_colors = new List<Color>();

            m_meshCollider = gameObject.AddComponent<MeshCollider>();
        }


        /* Generate mesh */
        public void Generate(ref HexCell[] cells)
        {
            m_mesh.Clear();
            m_vertices.Clear();
            m_triangles.Clear();
            m_colors.Clear();
            m_uv.Clear();


            for (int i = 0; i < cells.Length; i++)
            {
                CreateHex(ref cells[i], i);
            }


            m_mesh.vertices = m_vertices.ToArray();
            m_mesh.triangles = m_triangles.ToArray();
            m_mesh.uv = m_uv.ToArray();
            m_mesh.colors = m_colors.ToArray();


            m_mesh.RecalculateNormals();
            m_mesh.RecalculateBounds();         // TODO see if these two are necessary
            m_mesh.RecalculateTangents();
            //m_mesh.UploadMeshData();          // TODO see if this makes sense

            m_meshCollider.sharedMesh = m_mesh;
        }


        /* Create Hex */
        private void CreateHex(ref HexCell cell, int index)
        {
            for (HexDirection d = HexDirection.TopRight; d <= HexDirection.TopLeft; d++)
            {
                TriangulateSimpleWithTerraces(ref cell, d);
                
            }
        }


        /* Construct fused blending regions with terraces */
        private void TriangulateSimpleWithTerraces(ref HexCell cell, HexDirection direction)
        {
            // Construct Triangle
            Vector3 center = cell.Center;
            Vector3 v1 = center + (HexMeshUtility.GetCornerA(direction) * HexMeshUtility.SolidPercent);
            Vector3 v2 = center + (HexMeshUtility.GetCornerB(direction) * HexMeshUtility.SolidPercent);

            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.Color);

            // Create connection
            if (direction <= HexDirection.BottomRight)
            {
                HexCell neighbor = cell.GetNeighbor(direction);
                if (neighbor != null)
                {
                    // Create quad
                    Vector3 bridge = HexMeshUtility.GetBridgeSimple(direction);
                    Vector3 v3 = v1 + bridge;
                    Vector3 v4 = v2 + bridge;
                    v3.y = v4.y = neighbor.Elevation * HexMeshUtility.ElevationStep;

                    if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
                    {
                        TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
                    }
                    else
                    {
                        AddQuad(v1, v2, v3, v4);
                        AddQuadColor(cell.Color, neighbor.Color);
                    }


                    // Create triangular connection
                    HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
                    if (direction <= HexDirection.Right && nextNeighbor != null)
                    {
                        Vector3 v5 = v2 + HexMeshUtility.GetBridgeSimple(direction.Next());
                        v5.y = nextNeighbor.Elevation * HexMeshUtility.ElevationStep;


                        if (cell.Elevation <= neighbor.Elevation)
                        {
                            if (cell.Elevation <= nextNeighbor.Elevation)
                            {
                                TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                            }
                            else
                            {
                                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                            }
                        }
                        else if (neighbor.Elevation <= nextNeighbor.Elevation)
                        {
                            TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
                        }
                        else
                        {
                            TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                        }

                        //   AddTriangle(v2, v4, v5);
                        //   AddTriangleColor(cell.Color, neighbor.Color, nextNeighbor.Color);
                    }
                }
            }
        }


        /* Triangulate Edge Terraces */
        private void TriangulateEdgeTerraces(Vector3 beginLeft, Vector3 beginRight, HexCell beginCell, Vector3 endLeft, Vector3 endRight, HexCell endCell)
        {
            Vector3 v3 = HexMeshUtility.TerraceLerp(beginLeft, endLeft, 1);
            Vector3 v4 = HexMeshUtility.TerraceLerp(beginRight, endRight, 1);
            Color c2 = HexMeshUtility.TerraceLerp(beginCell.Color, endCell.Color, 1);

            AddQuad(beginLeft, beginRight, v3, v4);
            AddQuadColor(beginCell.Color, c2);

            for (int i = 2; i < HexMeshUtility.TerraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c2;
                v3 = HexMeshUtility.TerraceLerp(beginLeft, endLeft, i);
                v4 = HexMeshUtility.TerraceLerp(beginRight, endRight, i);
                c2 = HexMeshUtility.TerraceLerp(beginCell.Color, endCell.Color, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2);
            }


            AddQuad(v3, v4, endLeft, endRight);
            AddQuadColor(c2, endCell.Color);
        }
    

        /* Triangulate Corner */
        private void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
                else if (rightEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                    return;
                }

                else
                {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
            }
            else
            {
                AddTriangle(bottom, left, right);
                AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
            }
        }


        /* Triangulate Corner Terraces */
        private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMeshUtility.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMeshUtility.TerraceLerp(begin, right, 1);
            Color c3 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, 1);
            Color c4 = HexMeshUtility.TerraceLerp(beginCell.Color, rightCell.Color, 1);

            AddTriangle(begin, v3, v4);
            AddTriangleColor(beginCell.Color, c3, c4);

            for (int i = 2; i < HexMeshUtility.TerraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;
                v3 = HexMeshUtility.TerraceLerp(begin, left, i);
                v4 = HexMeshUtility.TerraceLerp(begin, right, i);
                c3 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, i);
                c4 = HexMeshUtility.TerraceLerp(beginCell.Color, rightCell.Color, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2, c3, c4);
            }

            AddQuad(v3, v4, left, right);
            AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
        }


        /* Triangulate Corner Terraces Cliff */
        private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0) { b = -b; }
            Vector3 boundary = Vector3.Lerp(begin, right, b);
            Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }

        }


        /* Triangulate Boundary Triangle */
        private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColor)
        {
            Vector3 v2 = HexMeshUtility.TerraceLerp(begin, left, 1);
            Color c2 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            AddTriangle(begin, v2, boundary);
            AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for (int i = 2; i < HexMeshUtility.TerraceSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = HexMeshUtility.TerraceLerp(begin, left, i);
                c2 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, i);
                AddTriangle(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangle(v2, left, boundary);
            AddTriangleColor(c2, leftCell.Color, boundaryColor);
        }


        /* Triangulate Corner Cliff Terraces */
        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0) { b = -b; }
            Vector3 boundary = Vector3.Lerp(begin, left, b);
            Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);
            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }

        }


        /* Add Traingles / Quads / Colors */
        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_vertices.Count;

            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);

            m_triangles.Add(vertexIndex++);
            m_triangles.Add(vertexIndex++);
            m_triangles.Add(vertexIndex);
        }

        private void AddTriangleColor(Color c)
        {
            m_colors.Add(c);
            m_colors.Add(c);
            m_colors.Add(c);
        }

        private void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
        }

        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
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

        private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
        {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
            m_colors.Add(c4);
        }

        private void AddQuadColor(Color c1, Color c2)
        {
            m_colors.Add(c1);
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c2);
        }

    }
}


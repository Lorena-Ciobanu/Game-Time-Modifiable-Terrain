using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTMT
{
    public class HexChunk : MonoBehaviour
    {
        /* Meshes [Changed in Prefab] */
        [SerializeField]
        private HexMesh m_terrainMesh;

        [SerializeField]
        private HexMesh m_waterMesh;

        [SerializeField]
        private HexMesh m_waterShoreMesh;


        /* Splat Map Colors */
        private static Color m_red = new Color(1f, 0f, 0f);         // Color 1 - First Texture Chanel
        private static Color m_green = new Color(0f, 1f, 0f);       // Color 2 - Second Texture Chanel
        private static Color m_blue = new Color(0f, 0f, 1f);        // Color 3 - Third Texture Chanel
        


        /* Cells coresponding to this chunk */
        private HexCell[] m_cells;


        /* Awake */
        private void Awake()
        {
            m_cells = new HexCell[HexMeshUtility.ChunkSizeX * HexMeshUtility.ChunkSizeZ];
        }


        /* AddCell */
        public void AddCell(int index, ref HexCell cell)
        {
            m_cells[index] = cell;
            cell.SetChunk(this);
        }


        /* LateUpdate - used to regenerate meshes */
        private void LateUpdate()
        {
            Construct();
            enabled = false;
        }


        /* Reconstruct - enable reconstructio on LateUpdate */
        public void Reconstruct()
        {
            enabled = true;
        }




        #region Triangulation


        private void Construct()
        {
            m_terrainMesh.Clear();
            m_waterMesh.Clear();
            m_waterShoreMesh.Clear();

            for(int i = 0; i < m_cells.Length; i++)
            {
                ConstructHex(ref m_cells[i], i);
            }

            m_terrainMesh.Apply();
            m_waterMesh.Apply();
            m_waterShoreMesh.Apply();
        }


        /* Construct Hex */
        private void ConstructHex(ref HexCell cell, int index)
        {
            for (HexDirection d = HexDirection.TopRight; d <= HexDirection.TopLeft; d++)
            {
                TriangulateSimpleWithTerraces(ref cell, d);

                if (cell.IsUnderwater)
                {
                    TriangulateWater(d, cell, cell.Center);
                }
            }
        }


        /* Construct fused blending regions with terraces */
        private void TriangulateSimpleWithTerraces(ref HexCell cell, HexDirection direction)
        {
            // Construct Triangle
            Vector3 center = cell.Center;
            Vector3 v1 = center + (HexMeshUtility.GetCornerA(direction) * HexMeshUtility.SolidPercent);
            Vector3 v2 = center + (HexMeshUtility.GetCornerB(direction) * HexMeshUtility.SolidPercent);

            m_terrainMesh.AddTriangle(center, v1, v2);

            if (HexMeshUtility.UseTextures)
            {
                m_terrainMesh.AddTriangleColor(m_red);
            }
            else
            {
                m_terrainMesh.AddTriangleColor(cell.Color);
            }
           

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
                        m_terrainMesh.AddQuad(v1, v2, v3, v4);

                        if (HexMeshUtility.UseTextures)
                        {
                            m_terrainMesh.AddQuadColor(m_red, m_green);
                        }
                        else
                        {
                            m_terrainMesh.AddQuadColor(cell.Color, neighbor.Color);
                        }
                       
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
                    }
                }
            }
        }


        /* Triangulate Edge Terraces */
        private void TriangulateEdgeTerraces(Vector3 beginLeft, Vector3 beginRight, HexCell beginCell, Vector3 endLeft, Vector3 endRight, HexCell endCell)
        {
            Vector3 v3 = HexMeshUtility.TerraceLerp(beginLeft, endLeft, 1);
            Vector3 v4 = HexMeshUtility.TerraceLerp(beginRight, endRight, 1);

            Color c2;

            if (HexMeshUtility.UseTextures)
            {
                c2 = HexMeshUtility.TerraceLerp(m_red, m_green, 1);
                m_terrainMesh.AddQuad(beginLeft, beginRight, v3, v4);
                m_terrainMesh.AddQuadColor(m_red, c2);
            }
            else
            {
                c2 = HexMeshUtility.TerraceLerp(beginCell.Color, endCell.Color, 1);
                m_terrainMesh.AddQuad(beginLeft, beginRight, v3, v4);
                m_terrainMesh.AddQuadColor(beginCell.Color, c2);
            }


            for (int i = 2; i < HexMeshUtility.TerraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c2;
                v3 = HexMeshUtility.TerraceLerp(beginLeft, endLeft, i);
                v4 = HexMeshUtility.TerraceLerp(beginRight, endRight, i);
                if (HexMeshUtility.UseTextures)
                {
                    c2 = HexMeshUtility.TerraceLerp(m_red, m_green, i);
                }
                else
                {
                    c2 = HexMeshUtility.TerraceLerp(beginCell.Color, endCell.Color, i);
                }
                
                m_terrainMesh.AddQuad(v1, v2, v3, v4);
                m_terrainMesh.AddQuadColor(c1, c2);
            }


            m_terrainMesh.AddQuad(v3, v4, endLeft, endRight);
            if (HexMeshUtility.UseTextures)
            {
                m_terrainMesh.AddQuadColor(c2, m_red);          // TODO figure out if this should be red
            }
            else
            {
                m_terrainMesh.AddQuadColor(c2, endCell.Color);
            }
           
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
                m_terrainMesh.AddTriangle(bottom, left, right);
                if (HexMeshUtility.UseTextures)
                {
                    m_terrainMesh.AddTriangleColor(m_red, m_green, m_blue);
                }
                else
                {
                    m_terrainMesh.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
                }
                
            }
        }


        /* Triangulate Corner Terraces */
        private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMeshUtility.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMeshUtility.TerraceLerp(begin, right, 1);
            Color c3, c4;

            if (HexMeshUtility.UseTextures)
            {
                c3 = HexMeshUtility.TerraceLerp(m_red, m_green, 1);
                c4 = HexMeshUtility.TerraceLerp(m_red, m_blue, 1);
            }
            else
            {
                c3 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, 1);
                c4 = HexMeshUtility.TerraceLerp(beginCell.Color, rightCell.Color, 1);
            }
            

            m_terrainMesh.AddTriangle(begin, v3, v4);
            m_terrainMesh.AddTriangleColor(beginCell.Color, c3, c4);

            for (int i = 2; i < HexMeshUtility.TerraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;
                v3 = HexMeshUtility.TerraceLerp(begin, left, i);
                v4 = HexMeshUtility.TerraceLerp(begin, right, i);

                if (HexMeshUtility.UseTextures)
                {
                    c3 = HexMeshUtility.TerraceLerp(m_red, m_green, i);
                    c4 = HexMeshUtility.TerraceLerp(m_red, m_blue, i);
                }
                else
                {
                    c3 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, i);
                    c4 = HexMeshUtility.TerraceLerp(beginCell.Color, rightCell.Color, i);
                }
               
                m_terrainMesh.AddQuad(v1, v2, v3, v4);
                m_terrainMesh.AddQuadColor(c1, c2, c3, c4);
            }

            m_terrainMesh.AddQuad(v3, v4, left, right);

            if (HexMeshUtility.UseTextures)
            {
                m_terrainMesh.AddQuadColor(c3, c4, m_green, m_blue);
            }
            else
            {
                m_terrainMesh.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
            }
           
        }


        /* Triangulate Corner Terraces Cliff */
        private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0) { b = -b; }
            Vector3 boundary = Vector3.Lerp(begin, right, b);
            Color boundaryColor;

            if (HexMeshUtility.UseTextures)
            {
                boundaryColor = Color.Lerp(m_red, m_blue, b);
                TriangulateBoundaryTriangle(begin, m_red, left, m_green, boundary, boundaryColor);
            }
            else
            {
                boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);
                TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);
            }
  

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (HexMeshUtility.UseTextures)
                {
                    TriangulateBoundaryTriangle(left, m_green, right, m_blue, boundary, boundaryColor);
                }
                else
                {
                    TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
                }
                
            }
            else
            {
                m_terrainMesh.AddTriangle(left, right, boundary);

                if (HexMeshUtility.UseTextures)
                {
                    m_terrainMesh.AddTriangleColor(m_green, m_blue, boundaryColor);
                }
                else
                {
                    m_terrainMesh.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
                }
               
            }

        }


        /* Triangulate Corner Cliff Terraces */
        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0) { b = -b; }
            Vector3 boundary = Vector3.Lerp(begin, left, b);
            Color boundaryColor;

            if (HexMeshUtility.UseTextures)
            {
                boundaryColor = Color.Lerp(m_red, m_green, b);
                TriangulateBoundaryTriangle(right, m_blue, begin, m_red, boundary, boundaryColor);
            }
            else
            {
                boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);
                TriangulateBoundaryTriangle(right, m_green, begin, m_blue, boundary, boundaryColor);
            }

           

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }
            else
            {
                m_terrainMesh.AddTriangle(left, right, boundary);

                if (HexMeshUtility.UseTextures)
                {
                    m_terrainMesh.AddTriangleColor(m_green, m_blue, boundaryColor);
                }
                else
                {
                    m_terrainMesh.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
                }
               
            }

        }


        /* Triangulate Boundary Triangle (using Color) */
        private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColor)
        {
            Vector3 v2 = HexMeshUtility.TerraceLerp(begin, left, 1);
            Color c2 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            m_terrainMesh.AddTriangle(begin, v2, boundary);
            m_terrainMesh.AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for (int i = 2; i < HexMeshUtility.TerraceSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = HexMeshUtility.TerraceLerp(begin, left, i);
                c2 = HexMeshUtility.TerraceLerp(beginCell.Color, leftCell.Color, i);
                m_terrainMesh.AddTriangle(v1, v2, boundary);
                m_terrainMesh.AddTriangleColor(c1, c2, boundaryColor);
            }

            m_terrainMesh.AddTriangle(v2, left, boundary);
            m_terrainMesh.AddTriangleColor(c2, leftCell.Color, boundaryColor);
        }


        /* Triangulate Boundary Triangle  (using Textures) */
        private void TriangulateBoundaryTriangle(Vector3 begin, Color beginColor, Vector3 left, Color leftColor, Vector3 boundary, Color boundaryColor)
        {
            Vector3 v2 = HexMeshUtility.TerraceLerp(begin, left, 1);
            Color c2 = HexMeshUtility.TerraceLerp(beginColor, leftColor, 1);

            m_terrainMesh.AddTriangle(begin, v2, boundary);
            m_terrainMesh.AddTriangleColor(beginColor, c2, boundaryColor);

            for (int i = 2; i < HexMeshUtility.TerraceSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = HexMeshUtility.TerraceLerp(begin, left, i);
                c2 = HexMeshUtility.TerraceLerp(beginColor, leftColor, i);
                m_terrainMesh.AddTriangle(v1, v2, boundary);
                m_terrainMesh.AddTriangleColor(c1, c2, boundaryColor);
            }

            m_terrainMesh.AddTriangle(v2, left, boundary);
            m_terrainMesh.AddTriangleColor(c2, leftColor, boundaryColor);
        }



        /* Triangulate Water */
        private void TriangulateWater(HexDirection direction, HexCell cell, Vector3 center)
        {
            center.y = cell.WaterSurfaceY;

            HexCell neighbor = cell.GetNeighbor(direction);
            if(neighbor != null && !neighbor.IsUnderwater)
            {
                TriangulateWaterShore(direction, cell, neighbor, center);
            }
            else
            {
                TriangulateOpenWater(direction, cell, neighbor, center);
            } 
        }

        /* Triangulate Open Water */
        private void TriangulateOpenWater(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
        {
            Vector3 c1 = center + (HexMeshUtility.GetCornerA(direction) * HexMeshUtility.SolidPercent);
            Vector3 c2 = center + (HexMeshUtility.GetCornerB(direction) * HexMeshUtility.SolidPercent);

            m_waterMesh.AddTriangle(center, c1, c2);

            if (direction <= HexDirection.BottomRight)
            {

                Vector3 bridge = HexMeshUtility.GetBridgeSimple(direction);
                Vector3 e1 = c1 + bridge;
                Vector3 e2 = c2 + bridge;

                m_waterMesh.AddQuad(c1, c2, e1, e2);

                if (direction <= HexDirection.Right)
                {
                    HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
                    if (nextNeighbor == null || !nextNeighbor.IsUnderwater)
                    {
                        return;
                    }
                    m_waterMesh.AddTriangle(c2, e2, c2 + HexMeshUtility.GetBridgeSimple(direction.Next()));
                }
            }
        }

        /* Triangulate Shore */
        private void TriangulateWaterShore(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
        {
            Vector3 c1 = center + (HexMeshUtility.GetCornerA(direction) * HexMeshUtility.SolidPercent);
            Vector3 c2 = center + (HexMeshUtility.GetCornerB(direction) * HexMeshUtility.SolidPercent);

            HexEdgeVertices e1 = new HexEdgeVertices(c1, c2);

            m_waterShoreMesh.AddTriangle(center, e1.v1, e1.v2);
            m_waterShoreMesh.AddTriangle(center, e1.v2, e1.v3);
            m_waterShoreMesh.AddTriangle(center, e1.v3, e1.v4);
            m_waterShoreMesh.AddTriangle(center, e1.v4, e1.v5);

            Vector3 bridge = HexMeshUtility.GetBridgeSimple(direction);
            HexEdgeVertices e2 = new HexEdgeVertices(e1.v1 + bridge, e1.v5 + bridge);
            m_waterShoreMesh.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            m_waterShoreMesh.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            m_waterShoreMesh.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            m_waterShoreMesh.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if(nextNeighbor != null)
            {
                m_waterShoreMesh.AddTriangle(e1.v5, e2.v5, c2 + HexMeshUtility.GetBridgeSimple(direction.Next()));
            }
        }

        #endregion
    }
}


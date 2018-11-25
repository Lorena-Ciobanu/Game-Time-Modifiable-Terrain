﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTMT
{
    public class HexGrid : MonoBehaviour
    {
        #region Hex Terrain Settup 

        [Header("Hex Cell Settings")]
        [SerializeField]
        private float hexRadius = 10.0f;

        [SerializeField]
        private float blendPercent = 0.25f;

        [SerializeField]
        private float elevationStep = 0.5f;

        [SerializeField]
        private int terracesPerSlope = 2;

        [SerializeField]
        private Color defaultColor = Color.white;



        [Header("Hex Grid Settings")]
        [SerializeField]
        private int chunkSizeX = 5;

        [SerializeField]
        private int chunkSizeZ = 5;

        [SerializeField]
        private int chunkCountX = 4;

        [SerializeField]
        private int chunkCountZ = 3;

        [SerializeField]
        public HexChunk chunkPrefab;


        [Header("Active Modifications")]
        [SerializeField]
        private int elevation = 0;

        [SerializeField]
        private Color color = Color.cyan;

        [SerializeField]
        private bool water = false;

        #endregion

        /* Private Fields  */
        private HexCell[] m_cells;
        private HexChunk[] m_chunks;
        

        private int m_cellCountX;
        private int m_cellCountZ;


        /* Awake */
        private void Awake()
        {
            m_cellCountX = chunkCountX * chunkSizeX;
            m_cellCountZ = chunkCountZ * chunkSizeZ;

            HexMeshUtility.SetUpUtility(hexRadius, blendPercent, elevationStep, terracesPerSlope, chunkSizeX, chunkSizeZ);

            GenerateChunks();
            GenerateCells();
           
        }


        /* Generate Chunks */
        private void GenerateChunks()
        {
            m_chunks = new HexChunk[chunkCountX * chunkCountZ];

            for(int z = 0, i = 0; z < chunkCountZ; z++)
            {
                for(int x = 0; x < chunkCountX; x++)
                {
                    HexChunk chunk = m_chunks[i++] = Instantiate(chunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }
        }


        /* Generate Cells */
        private void GenerateCells()
        {
            m_cells = new HexCell[m_cellCountX * m_cellCountZ];

            for(int z = 0, i = 0; z < m_cellCountZ; z++)
            {
                for(int x = 0; x < m_cellCountX; x++)
                {
                    GenerateCell(x, z, i++);
                }
            }
        }


        /* Generate Cell */
        private void GenerateCell(int x, int z, int index)
        {
            Vector3 center = new Vector3(
                        (x + z * 0.5f - z / 2) * (HexMeshUtility.InnerRadius * 2f),  // Applied z * 0.5 - z/2 to account for row displacement (z * 0.5 - )   (z / 2 - every second row, cells move one additional step)
                        0,
                        z * (HexMeshUtility.OuterRadius * 1.5f)
                    );

            m_cells[index] = new HexCell(this, index, center, HexCoordinate.FromOffsetCoordinates(x, z), defaultColor);

            // Set connections to the Left and Right
            if (x > 0)
            {
                m_cells[index].SetNeighbor(HexDirection.Left, ref m_cells[index - 1]);
            }


            if (z > 0)
            {
                // Even rows 
                if ((z & 1) == 0)      // bitwise and 1 => mask everything but last digit. if 0 => even
                {

                    // Set connections BottomRight and TopLeft
                    m_cells[index].SetNeighbor(HexDirection.BottomRight, ref m_cells[index - m_cellCountX]);

                    // Set connections BottomLeft and TopRight
                    if (x > 0)
                    {
                        m_cells[index].SetNeighbor(HexDirection.BottomLeft, ref m_cells[index - m_cellCountX - 1]);
                    }
                }

                // Odd rows
                else
                {
                    // Set connections BottomLeft and TopRight
                    m_cells[index].SetNeighbor(HexDirection.BottomLeft, ref m_cells[index - m_cellCountX]);

                    // Set connections BottomRight and TopLeft 
                    if (x < m_cellCountX - 1)
                    {
                        m_cells[index].SetNeighbor(HexDirection.BottomRight, ref m_cells[index - m_cellCountX + 1]);
                    }
                }
            }

            AddCellToChunk(x, z, ref m_cells[index]);
        }


        /* Add Cell To Chunk*/
        private void AddCellToChunk(int x, int z, ref HexCell cell)
        {
            int chunkX = x / chunkSizeX;
            int chunkZ = z / chunkSizeZ;
            HexChunk chunk = m_chunks[chunkX + chunkZ * chunkCountX];  

            int localX = x - chunkX * chunkSizeX;
            int localZ = z - chunkZ * chunkSizeZ;
            chunk.AddCell(localX + localZ * chunkSizeX, ref cell);
        }


        /* Handle Input */
        private void HandleInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(inputRay, out hit))
            {
                Vector3 position = transform.InverseTransformPoint(hit.point);
                HexCoordinate coordinate = HexCoordinate.FromPosition(position, HexMeshUtility.InnerRadius, HexMeshUtility.OuterRadius);
                int index = coordinate.X + coordinate.Z * m_cellCountX + coordinate.Z / 2;
                if(index < m_cells.Length)
                {
                    HexCell cell = m_cells[index];
                    cell.Color = color;
                    cell.SetElevation(elevation, elevationStep);
                    cell.Refresh();
                }

               
            }
        }


        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleInput();
            }
        }


        #region Public Functions
        public HexCell[] GetCells()
        {
            return m_cells;
        }

        public HexCell GetHexCell(int index)
        {
            if(index >= 0 && index < m_cells.Length)
            {
                return m_cells[index];
            }

            return null;
           
        }
        #endregion
    }
}


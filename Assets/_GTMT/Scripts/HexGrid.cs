using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        private bool useTextures = true;

        [SerializeField]
        private Color defaultColor = Color.white;



        [Header("Hex Grid Settings")]
        [SerializeField]
        private int hexesPerChunkX = 5;

        [SerializeField]
        private int hexesPerChunkZ = 5;

        [SerializeField]
        private int numberOfChunksX = 4;

        [SerializeField]
        private int numberOfChunksZ = 3;

        [SerializeField]
        public HexChunk chunkPrefab;

        [SerializeField]
        public float waterElevationOffset = -0.5f;


        [Header("Hex Display Settings")]
        [SerializeField]
        private bool showGrid = false;
        [SerializeField]
        private Material terrainMaterial;


        [Header("Pathing")]
        [SerializeField]
        private HexPawn pawnPrefab;


        [Header("Active Modifications")]
      //  [SerializeField]
     //   private bool editMode = true;

        [SerializeField]
        private int elevation = 0;

        [SerializeField]
        private Color color = Color.cyan;

        [SerializeField]
        private int waterLevel = 0;

        [SerializeField]
        private int terrainIndex = 0;


        #endregion


        /* Private Fields  */
        private HexCell[] m_cells;
        private HexChunk[] m_chunks;
        
        private int m_cellCountX;
        private int m_cellCountZ;

        /* Pathing */
        private HexPawn m_pawn;
        private HexCell m_targetCell;


        /* Awake */
        private void Awake()
        {
            // Performance setting
            QualitySettings.vSyncCount = 0;



            m_cellCountX = numberOfChunksX * hexesPerChunkX;
            m_cellCountZ = numberOfChunksZ * hexesPerChunkZ;

            /* Set properties to HexTerrainMaterial*/
            if (showGrid)
            {
                
                terrainMaterial.EnableKeyword("GRID_ON");
            }
            else
            {
                terrainMaterial.DisableKeyword("GRID_ON");
            }

            terrainMaterial.SetFloat(Shader.PropertyToID("_HexSize"), hexRadius);


            HexMeshUtility.SetUpUtility(hexRadius, blendPercent, elevationStep, terracesPerSlope, hexesPerChunkX, hexesPerChunkZ, waterElevationOffset, useTextures);

            GenerateChunks();
            GenerateCells();
            PlacePawn();
        }

        #region Construction
        /* Generate Chunks */
        private void GenerateChunks()
        {
            m_chunks = new HexChunk[numberOfChunksX * numberOfChunksZ];

            for(int z = 0, i = 0; z < numberOfChunksZ; z++)
            {
                for(int x = 0; x < numberOfChunksX; x++)
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
            int chunkX = x / hexesPerChunkX;
            int chunkZ = z / hexesPerChunkZ;
            HexChunk chunk = m_chunks[chunkX + chunkZ * numberOfChunksX];  

            int localX = x - chunkX * hexesPerChunkX;
            int localZ = z - chunkZ * hexesPerChunkZ;
            chunk.AddCell(localX + localZ * hexesPerChunkX, ref cell);
        }
        #endregion


        private void PlacePawn()
        {
            m_pawn = Instantiate(pawnPrefab);
            m_pawn.transform.SetParent(transform, false);
            m_pawn.Cell = m_cells[0];
           
            //m_pawn.transform.position = new Vector3(m_pawn.transform.position.x, m_pawn.transform.position.y + 0.5f, m_pawn.transform.position.z);
        }



        #region Input and Editing


        /* Update Cell */
        private void UpdateCell(HexCell cell)
        {
            if (cell.Color != color || cell.Elevation != elevation || cell.WaterLevel != waterLevel || cell.TerrainTypeIndex != terrainIndex)
            {
                // Edit Cell
                cell.Color = color;
                cell.SetElevation(elevation, elevationStep);
                cell.WaterLevel = waterLevel;
                cell.TerrainTypeIndex = terrainIndex;
                cell.Reconstruct();
            }
        }


        /* Handle Input */
        private HexCell HandleInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                Vector3 position = transform.InverseTransformPoint(hit.point);
                HexCoordinate coordinate = HexCoordinate.FromPosition(position, HexMeshUtility.InnerRadius, HexMeshUtility.OuterRadius);
                int index = coordinate.X + coordinate.Z * m_cellCountX + coordinate.Z / 2;
                if (index < m_cells.Length)
                {
                    return m_cells[index]; 
                }
            }

            return null;
        }

        /* Update */
        private void Update()
        {

            if (Input.GetKey(KeyCode.LeftShift) )
            {
                if (Input.GetMouseButtonDown(0))
                {
                    HexCell cell = HandleInput();
                    if (cell != null && !cell.IsUnderwater)
                    {
                        m_pawn.Travel(ref m_cells, cell);
                    }
                }
               
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    HexCell x = HandleInput();
                    if (x != null)
                    {
                        UpdateCell(x);
                    }

                }
                if (Input.GetMouseButtonUp(0) && m_pawn.isTraveling)
                {
                   m_pawn.UpdateTravel(ref m_cells); 
                }
            }

           
        }
        
        #endregion

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


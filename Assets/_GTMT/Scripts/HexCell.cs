using UnityEngine;

namespace GTMT
{
    [System.Serializable]
    public class HexCell
    {
        private HexGrid m_grid;     // reference to hex grid (in order to get neighbor)
        private HexChunk m_chunk;   // reference to chunk

        [SerializeField]
        private int m_index;

        [SerializeField]
        private Vector3 m_center;

        [SerializeField]
        private HexCoordinate m_hexCoordinate;

        [SerializeField]
        private Color m_color;

        [SerializeField]
        private int m_elevation;

        [SerializeField]
        private int[] m_neighbors;

        [SerializeField]
        private int m_waterLevel;

        [SerializeField]
        private int m_terrainTypeIndex;



        /* Render */
        public ref Vector3 Center
        {
            get { return ref m_center; }
        }

        public Color Color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
            }
        }

        public int Elevation
        {
            get
            {
                return m_elevation;
            }
        }

        public int TerrainTypeIndex
        {
            get
            {
                return m_terrainTypeIndex;
            }
            set
            {
                if(m_terrainTypeIndex != value)
                {
                    m_terrainTypeIndex = value;
                }
            }
        }



        /* Water */
        public int WaterLevel
        {
            get
            {
                return m_waterLevel;
            }
            set
            {
                if(m_waterLevel == value)
                {
                    return;
                }

                m_waterLevel = value;
            }
        }

        public bool IsUnderwater
        {
            get
            {
                return m_waterLevel > m_elevation;
            }
        }

        public float WaterSurfaceY
        {
            get
            {
                return (m_waterLevel + HexMeshUtility.WaterElevationOffset) * HexMeshUtility.ElevationStep; 
            }
        }




        /* Set Chunk */
        public void SetChunk(HexChunk chunk)
        {
            this.m_chunk = chunk;
        }



        /* Constructor */
        public HexCell(HexGrid grid, int index, Vector3 center, HexCoordinate coordinate, Color color)
        {
            m_grid = grid;
            m_index = index;
            m_center = center;
            m_hexCoordinate = coordinate;
            m_color = color;
            m_neighbors = new int[6];       // every hex has 6 neighbors    // TODO only assign as many neighbors as actually exist [?] (corners and margins)

            for (int i = 0; i < m_neighbors.Length; i++)
            {
                m_neighbors[i] = -1;
            }
        }



        #region Public methods

        public void SetNeighbor(HexDirection direction, ref HexCell cell)
        {
            m_neighbors[(int)direction] = cell.m_index;
            cell.m_neighbors[(int)direction.Opposite()] = m_index;
        }


        public HexCell GetNeighbor(HexDirection direction)
        {
            if (m_neighbors[(int)direction] == -1) return null;
            return m_grid.GetHexCell(m_neighbors[(int)direction]);
        }


        public void SetElevation(int elevation, float elevationStep)
        {
            m_elevation = elevation;
            m_center.y = elevation * elevationStep;
        }


        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return HexMeshUtility.GetEdgeType(m_elevation, GetNeighbor(direction).m_elevation);
        }


        public HexEdgeType GetEdgeType(HexCell otherCell)
        {
            return HexMeshUtility.GetEdgeType(Elevation, otherCell.Elevation);
        }


        #endregion


        /* Refresh
         *  - responsable of calling refresh on entire chunk to reconstruct
         */
        public void Refresh()
        {
            if (m_chunk)
            {
                m_chunk.Reconstruct();

                for (int i = 0; i < m_neighbors.Length; i++)
                {
                    HexCell neighbor = m_grid.GetHexCell(m_neighbors[i]);
                    if (neighbor != null && neighbor.m_chunk != m_chunk)
                    {
                        neighbor.m_chunk.Reconstruct();
                    }
                }
            }
        }
    }
}
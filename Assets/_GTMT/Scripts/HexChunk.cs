using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTMT
{
    public class HexChunk : MonoBehaviour
    {
        private HexCell[] m_cells;
        private HexMesh m_mesh;

        private void Awake()
        {
            m_mesh = GetComponentInChildren<HexMesh>();
            m_cells = new HexCell[HexMeshUtility.ChunkSizeX * HexMeshUtility.ChunkSizeZ];
        }

        private void Start()
        {
            m_mesh.Generate(ref m_cells);
        }


        private void LateUpdate()
        {
            m_mesh.Generate(ref m_cells);
            enabled = false;
        }


        public void AddCell(int index, ref HexCell cell)
        {
            m_cells[index] = cell;
            cell.SetChunk(this);
        }


        public void Reconstruct()
        {
            enabled = true;
        }
    }
}


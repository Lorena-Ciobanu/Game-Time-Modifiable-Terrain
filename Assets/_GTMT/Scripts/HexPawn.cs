using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTMT
{
    public class HexPawn : MonoBehaviour
    {
        private HexCell m_cell;
        private static List<HexCell> m_path;
        private const float m_travelSpeed = 3f;

        public bool isTraveling = false;


        private HexCell m_destination;
        private HexCell[] m_cells;

        public HexCell Cell
        {
            get
            {
                return m_cell;
            }
            set
            {
                m_cell = value;
               // transform.localPosition = value.Position + new Vector3(0, 0.2f, 0);
            }
        }


        private IEnumerator TravelPath()
        {
            if(m_destination == Cell || m_destination.IsUnderwater)
            {
                yield return null;
            }

            m_path = HexAStar.Search(ref m_cells, Cell, ref m_destination);

            if (m_path != null && m_path.Count > 1)
            {
                m_destination = m_path[m_path.Count - 1];

                isTraveling = true;

                for (int i = 1; i < m_path.Count; i++)
                {
                    Vector3 a = m_path[i - 1].Position;
                    Vector3 b = m_path[i].Position;
                    Cell = m_path[i];
                    for (float t = 0f; t < 1f; t += Time.deltaTime * m_travelSpeed)
                    {
                        transform.localPosition = Vector3.Lerp(a, b, t) + new Vector3(0f, 0.2f, 0f);
                        yield return null;
                    }
                }
            }

            isTraveling = false;
            yield break;
        }

        public void Travel(ref HexCell[] cells, HexCell destination)
        {
            StopAllCoroutines();
            m_cells = cells;
            m_destination = destination;
            StartCoroutine(TravelPath());
        }

        public void UpdateTravel(ref HexCell[] cells)
        {
            Travel(ref cells, m_destination);
        }
    }


}

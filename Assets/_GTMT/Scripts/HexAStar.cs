using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTMT {

    public static class HexAStar
    {
        private static bool m_avoidWater = true;
        private static bool m_avoidCliff = true;

        private static List<HexCell> m_path = new List<HexCell>();

        private static bool m_computing = false;

        private static int[] m_terrainCosts = new int[4] { 1, 1, 1, 1 };
        private static int m_slopeCost = 5;

        private static List<HexCell> m_open = new List<HexCell>();  //TODO should implement PriorityQueue

        /* ComputeDistances */
        private static bool ComputeDistances(ref HexCell[] cells, ref HexCell startCell, ref HexCell endCell)
        {

            if (!m_computing)
            {
                m_computing = true;
                m_open.Clear();

                foreach (HexCell c in cells)
                {
                    c.Distance = int.MaxValue;
                }

                startCell.Distance = 0;
                m_open.Add(startCell);

                while (m_open.Count > 0)
                {
                    HexCell current = m_open[0];
                    m_open.RemoveAt(0);

                    if (current == endCell)
                    {
                        break;
                    }

                    // Add neighbors in all directions

                    for (HexDirection d = HexDirection.TopRight; d < HexDirection.TopLeft; d++)
                    {
                        HexCell neighbor = current.GetNeighbor(d);


                        if (neighbor == null)
                        {
                            continue;
                        }

                        // Avoid Water
                        if (m_avoidWater && neighbor.IsUnderwater)
                        {
                            continue;
                        }

                        // Avoid Cliffs
                        if (m_avoidCliff && current.GetEdgeType(neighbor) == HexEdgeType.Cliff)
                        {
                            continue;
                        }

                        int newDistance = 0;


                        // Add cost of crossing Slope
                    /*    if (current.GetEdgeType(neighbor) == HexEdgeType.Slope)
                        {
                            newDistance += m_slopeCost;
                        } */

                        // Add cost of terrain movement
                        newDistance += m_terrainCosts[current.TerrainTypeIndex];

                        // If it's an untraversed hex
                        if (neighbor.Distance == int.MaxValue)
                        {
                            neighbor.Distance = newDistance;
                            neighbor.PathDirection = current;
                            neighbor.Heuristic = neighbor.GetCoordinate().DistanceTo(endCell.GetCoordinate());
                            m_open.Add(neighbor);
                        }
                        // If we've already visited the hex but found a better value
                        else if (newDistance < neighbor.Distance)
                        {
                            neighbor.Distance = newDistance;
                            neighbor.PathDirection = current;
                        }

                        m_open.Sort((hex1, hex2) => hex1.SearchPriority.CompareTo(hex2.SearchPriority));

                    }
                }
            }
            else
            {
                Debug.Log("still computing");
                return false;
            }

            
            m_computing = false;
            return true;
        }

        public static ref List<HexCell> Search(ref HexCell[] cells,  HexCell start, ref HexCell end)
        {
            bool c = ComputeDistances(ref cells, ref start, ref end);
            if (c)
            {
               bool pathAvailable = GetPath(ref start, ref end);  
            }
            return ref m_path;
        }


        private static bool GetPath(ref HexCell start, ref HexCell end)
        {
            m_path.Clear();
            if (end.PathDirection != null)
            {
                for (HexCell c = end; c != start; c = c.PathDirection)
                {
                    m_path.Add(c);
                    Debug.Log(c);
                }
                m_path.Add(start);
                m_path.Reverse();
                return true;
            }
            else
            {
                Debug.Log("No path found");
                return false;
            }
        }
    }
}


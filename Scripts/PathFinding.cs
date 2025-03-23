using Com.IsartDigital.SokoVolt.GameObjects;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

// Author : A. Dylan Montenegro Utrela

namespace Com.IsartDigital.ProjectName {

    public partial class PathFinding
    {
        public static List<Vector2> FindPath(Vector2 pStart, Vector2 pTarget, Cell[,] pGrid) 
        {
            PriorityQueue<Vector2, float> lNewCell = new PriorityQueue<Vector2, float>(); // cells to explore
            HashSet<Vector2> lExploredCell = new HashSet<Vector2>(); // explored cells

            Dictionary<Vector2, Vector2> lFromPos = new Dictionary<Vector2, Vector2>(); // 
            Dictionary<Vector2, float> lGScore = new Dictionary<Vector2, float>(); // distance from start
            Dictionary<Vector2, float> lFScore = new Dictionary<Vector2, float>(); // total score

            lNewCell.Enqueue(pStart, 0);
            lGScore[pStart] = 0;
            lFScore[pStart] = DistanceCost(pStart, pTarget);

            while (lNewCell.Count > 0)
            {
                Vector2 lCurrent = lNewCell.Dequeue();

                if (lCurrent == pTarget)
                    return ReconstructPath(lFromPos, lCurrent);

                lExploredCell.Add(lCurrent);

                foreach (Vector2 pNextCell in GetNextCell(lCurrent, pGrid))
                {
                    if (lExploredCell.Contains(pNextCell)) continue;

                    float lTentativeGScore = lGScore[lCurrent] + 1;

                    if (!lGScore.ContainsKey(pNextCell) || lTentativeGScore < lGScore[pNextCell])
                    {
                        lFromPos[pNextCell] = lCurrent;
                        lGScore[pNextCell] = lTentativeGScore;
                        lFScore[pNextCell] = lTentativeGScore + DistanceCost(pNextCell, pTarget);

                        if (!lNewCell.UnorderedItems.Any(e => e.Element == pNextCell))
                            lNewCell.Enqueue(pNextCell, lFScore[pNextCell]);
                    }
                }
            }
            return null;
        }

        private static float DistanceCost(Vector2 pStart, Vector2 pEnd) // distance between two cells
        {
            return Mathf.Abs(pStart.X - pEnd.X) + Mathf.Abs(pStart.Y - pEnd.Y);
        }

        private static List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> pFromPos, Vector2 lCurrentPos)
        {
            List<Vector2> lTotalPath = new List<Vector2> { lCurrentPos };

            while (pFromPos.ContainsKey(lCurrentPos))
            {
                lCurrentPos = pFromPos[lCurrentPos];
                lTotalPath.Add(lCurrentPos);
            }

            lTotalPath.Reverse();
            return lTotalPath;
        }

        private static IEnumerable<Vector2> GetNextCell(Vector2 pCell, Cell[,] pGrid) // returns walkable cells 
        {
            List<Vector2> lDirections = new List<Vector2>
            {
                new Vector2(pCell.X + 1, pCell.Y),
                new Vector2(pCell.X - 1, pCell.Y),
                new Vector2(pCell.X, pCell.Y + 1),
                new Vector2(pCell.X, pCell.Y - 1)
            };

            foreach (Vector2 pNextCell in lDirections)
            {
                int lPosX = (int)pNextCell.X;
                int lPosY = (int)pNextCell.Y;

                if (lPosX >= 0 && lPosY >= 0 && lPosX < pGrid.GetLength(0) && lPosY < pGrid.GetLength(1))
                {
                    GameObject lContent = pGrid[lPosX, lPosY].GetContent();
                    if (lContent == null || (lContent is Door lDoor && lDoor.isOpen)) // once the door is open the player can travel in it once clicked
                        yield return pNextCell;
                }
            }
        }
    }
}

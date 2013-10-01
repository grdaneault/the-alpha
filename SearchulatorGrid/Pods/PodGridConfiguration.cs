using System;
using System.Collections.Generic;

namespace SearchulatorGrid.Pods
{
    internal class PodGridConfiguration
    {
        public PodGridConfiguration(int maxRows, int maxCols)
        {
            MaxRows = maxRows;
            MaxCols = maxCols;
            Grid = new Pod[MaxRows,MaxCols];
            PodList = new List<Pod>();
            _curRow = 0;
        }

        public PodGridConfiguration(PodGridConfiguration starting)
        {
            Row = starting.Row;
            Col = starting.Col;
            MaxRows = starting.MaxRows;
            MaxCols = starting.MaxCols;
            Grid = starting.Grid.Clone() as Pod[,];
            PodList = starting.PodList;
        }

        private int _curRow;
        public int Row { get; set; }
        public int Col { get; set; }

        public int MaxCols { get; set; }
        public int MaxRows { get; set; }

        public Pod[,] Grid { get; set; }
        public List<Pod> PodList { get; set; }

        /// <summary>
        ///     Fills a pod into the grid
        /// </summary>
        /// <param name="p">The pod to fill</param>
        /// <param name="row">The starting row</param>
        /// <param name="col">The starting col</param>
        /// <param name="numRows">The number of rows</param>
        /// <param name="numCols">The number of cols</param>
        private void Fill(Pod p, int row, int col, int numRows, int numCols)
        {
            p.Row = row;
            p.Col = col;
            PodList.Add(p);
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    Grid[row + r, col + c] = p;
                }
            }
        }

        /// <summary>
        ///     Places a pod with a given size into the grid
        /// </summary>
        /// <param name="p">The pod to place</param>
        /// <param name="numRows">Desired number of rows</param>
        /// <param name="numCols">Desired number of cols</param>
        /// <returns>True if the pod was placed</returns>
        public bool Place(Pod p, int numRows, int numCols)
        {
            for (Row = _curRow; Row < MaxRows; Row++)
            {
                for (Col = 0; Col < MaxCols; Col++)
                {
                    if (CanPlace(numRows, numCols))
                    {
                        Fill(p, Row, Col, numRows, numCols);
                        p.RowSpan = numRows;
                        p.ColSpan = numCols;
                        _curRow = Row;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if a pod can be places at the current position
        /// </summary>
        /// <param name="numRows"></param>
        /// <param name="numCols"></param>
        /// <returns></returns>
        private bool CanPlace(int numRows, int numCols)
        {
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    if (Row + r >= MaxRows || Col + c >= MaxCols || Grid[Row + r, Col + c] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Sets the RowSpan and ColSpan of a pod from the 2D array.
        /// </summary>
        /// <param name="p">Pod to set the info for</param>
        /// <param name="row">Starting row location</param>
        /// <param name="col">Starting col location</param>
        private void SetRowColSpans(Pod p, int row, int col)
        {
            p.ColSpan = 0;
            p.RowSpan = 0;
            do
            {
                p.RowSpan++;
                row++;
            } while (row < MaxRows && p == Grid[row, col]);

            row--;

            do
            {
                p.ColSpan++;
                col++;
            } while (col < MaxCols && p == Grid[row, col]);
        }

        /// <summary>
        ///     Generates the final list of pods as used by the GridView
        /// </summary>
        /// <returns>The ordered list</returns>
        public List<Pod> GenerateFinalList()
        {
            List<Pod> final = new List<Pod>();
            for (int col = 0; col < MaxCols; col++)
            {
                for (int row = 0; row < MaxRows; row++)
                {
                    if (Grid[row, col] != null && !final.Contains(Grid[row, col]))
                    {
                        //setRowColSpans(Grid[row, col], row, col);
                        final.Add(Grid[row, col]);
                    }
                }
            }

            return final;
        }

        /// <summary>
        ///     Checks if the grid is full
        /// </summary>
        /// <returns>true, if full</returns>
        public bool Full()
        {
            for (int r = 0; r < MaxRows; r++)
            {
                for (int c = 0; c < MaxCols; c++)
                {
                    if (Grid[r, c] == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Fills a grid to use all available space
        /// </summary>
        public void FillOut(bool last)
        {
            int max = LastFilledRow();
            if (tryFillSingleColumn())
            {
                return;
            }
            
            for (Row = 0; Row <= max; Row++)
            {
                for (Col = 0; Col < MaxCols; Col++)
                {
                    if (Grid[Row, Col] == null)
                    {
                        if (!((InBounds(Row, Col - 1) && CanStetch(Row, Col - 1, Row, Col)) ||
                                (InBounds(Row, Col + 1) && CanStetch(Row, Col + 1, Row, Col)) ||
                                (InBounds(Row - 1, Col) && CanStetch(Row - 1, Col, Row, Col)) ||
                                (InBounds(Row + 1, Col) && CanStetch(Row + 1, Col, Row, Col))))
                        {
                            // Could not stretch... Add an ad?
                        }
                    }
                }
            }

            if (!last)
            {
                PadBottom();
            }
        }

        private bool tryFillSingleColumn()
        {
            List<Pod> singles = new List<Pod>();
            for (int r = 0; r < MaxRows; r++)
            {
                for (int c = 0; c < MaxCols; c++)
                {
                    if (Grid[r, c] != null)
                    {

                        if (Grid[r, c].ColSpan == 1)
                        {
                            if (!singles.Contains(Grid[r, c]))
                            {
                                singles.Add(Grid[r, c]);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            int rows = 0;
            foreach (Pod p in singles)
            {
                rows += p.RowSpan;
            }

            if (rows > MaxRows) return false;

            while (rows < MaxRows)
            {
                for (int i = 0; i < singles.Count; i++)
                {
                    singles[i].RowSpan++;
                    rows++;
                    if (rows == MaxRows) return true;
                }
            }

            return true;
        }

        private void PadBottom()
        {
            int padding = MaxRows - LastFilledRow() - 1;

            while (padding > 0)
            {
                List<Pod> padded = new List<Pod>();
                for (int r = MaxRows - 1; r >= 0; r--)
                {
                    bool paddedRow = false;

                    for (int c = 0; c < MaxCols; c++)
                    {
                        if (Grid[r, c] != null && !padded.Contains(Grid[r, c]))
                        {
                            padded.Add(Grid[r, c]);
                            Grid[r, c].RowSpan++;
                            paddedRow = true;
                        }
                    }

                    if (paddedRow) padding--;
                    if (padding == 0) return;
                }
            }
        }

        private int LastFilledRow()
        {
            for (int r = MaxRows - 1; r >= 0; r--)
            {
                for (int c = MaxCols - 1; c >= 0; c--)
                {
                    if (Grid[r, c] != null)
                    {
                        return r;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        ///     Checks if the pod at (rS, cS) can stretch to (rE, cE)
        /// </summary>
        /// <param name="rS">Starting Row</param>
        /// <param name="cS">Starting Col</param>
        /// <param name="rE">Ending Row</param>
        /// <param name="cE">Ending Col</param>
        /// <returns>True if the space is available</returns>
        private bool CanStetch(int rS, int cS, int rE, int cE)
        {
            if (Grid[rS, cS] == null || Grid[rE, cE] != null)
            {
                return false;
            }

            Pod start = Grid[rS, cS];

            for (int r = start.Row; r <= rE; r++)
            {
                for (int c = start.Col; c <= cE; c++)
                {
                    if (Grid[r, c] != null && Grid[r, c] != start)
                    {
                        return false;
                    }
                }
            }

            // Can stretch, fill in the grid
            int actualRowEnd = start.Row + start.RowSpan - 1;
            int actualColEnd = start.Col + start.ColSpan - 1;

            // update the Row and Col stored by the pod if moved up
            if (rE < start.Row)
            {
                start.Row = rE;
            }

            if (cE < start.Col)
            {
                start.Col = cE;
            }

            for (int r = start.Row; r <= Math.Max(rE, actualRowEnd); r++)
            {
                for (int c = start.Col; c <= Math.Max(cE, actualColEnd); c++)
                {
                    Grid[r, c] = start;
                }
            }


            SetRowColSpans(start, start.Row, start.Col);

            return true;
        }

        /// <summary>
        ///     Checks if a given (Row, Col) is inside the grid
        /// </summary>
        /// <param name="r">Row</param>
        /// <param name="c">Col</param>
        /// <returns>True if the point is inside the grid</returns>
        private bool InBounds(int r, int c)
        {
            return r >= 0 && r < MaxRows && c >= 0 && c < MaxCols;
        }
    }
}
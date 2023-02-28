using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuModel
{
    public class Board
    {
        // the size of the board
        public int Size { get; set; }

        public string[,] Sudoku;

        // constructor
        public Board(int s)
        {
            // initial size of the board is defined bu s
            Size = s;
            Sudoku = new string[Size, Size];
        }
    }
}
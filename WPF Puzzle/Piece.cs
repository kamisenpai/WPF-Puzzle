using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Controls;
using System.Windows;

namespace WPF_Puzzle
{
    class Piece:Button
    {
        public Piece(double pieceSizeWidth, double pieceSizeHeight)
        {
            Width = pieceSizeWidth;
            Height = pieceSizeHeight;
            
            FontSize = 32;
            //Font = new Font("Verdana", pieceSize / 4);
        }
        public void Pos(Point pos)
        {
            PositionX = pos.X;
            PositionY = pos.Y;
        }

        public  double PositionX { get; set; }
        public  double PositionY { get; set; }

    }
}

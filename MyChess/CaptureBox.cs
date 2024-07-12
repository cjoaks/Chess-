using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyChess
{
    class CaptureBox : GroupBox
    {
        public static readonly int LOCATION_X = 27;
        public static readonly int TOP_LOCATION_Y = 120; 
        public static readonly int BOTTOM_LOCATION_Y = 440;

        private const int PIECES_PER_ROW = 8;
        private const int WIDTH = 551; 
        private const int HEIGHT = 310;

        private int piecesCaptured;
        private readonly Chess.PieceColor color; 
        public CaptureBox(Chess.PieceColor color)
        {
            this.color = color;
            Text = color.ToString(); 
            Location = new Point(LOCATION_X, (color == Chess.PieceColor.WHITE) ? BOTTOM_LOCATION_Y : TOP_LOCATION_Y); 
            Width = WIDTH;
            Height = HEIGHT;

            piecesCaptured = 0; 
        }

        public void AddPiece(Piece p)
        {
            Controls.Add(GetPieceImageControl(p.Image));
            Invalidate(); 
            ++piecesCaptured;
        }

        public void UpdateMaterial(List<Piece> activePieces)
        {
            Text = color.ToString();
            int diff = 0;
            foreach(Piece p in activePieces)
            {
                diff += Chess.GetMaterialValue(p) * (p.WhichColor == color ? 1 : -1); 
            }
            if(diff > 0)
            {
                Text += " (+" + diff + ")"; 
            }
        }

        private PictureBox GetPieceImageControl(Image img) 
        {
            PictureBox pb = new PictureBox
            {
                Image = img,
                Size = new Size(img.Width, img.Height),
                Location = new Point(40 + piecesCaptured % PIECES_PER_ROW * 50, 40 + piecesCaptured / PIECES_PER_ROW * 90),
                BackColor = Color.Transparent
            };
            return pb; 
        }

        public void Reset()
        {
            Text = color.ToString();
            Controls.Clear();
            piecesCaptured = 0; 
        }

        public void Flip()
        {
            Location = new Point(LOCATION_X, (Location.Y == TOP_LOCATION_Y) ? BOTTOM_LOCATION_Y : TOP_LOCATION_Y); 
        }
    }
}

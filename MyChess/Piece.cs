using MyChess.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyChess
{
    class Piece : PictureBox
    {
        #region Private Fields

        private Point square;
        private Chess.PieceType type; 
        
        #endregion
        #region Public Fields

        public readonly Chess.PieceColor WhichColor; 
        
        public bool HasMoved;
        public bool ToMove; 

        #endregion
        #region Accessors
        public Point Square
        {
            get
            {
                return square;
            }
            set
            {
                square = value; 
            }
        }

        public Chess.PieceType Type
        {
            get
            {
                return type; 
            }
        }

        #endregion
        public Piece(Chess.PieceType type, Chess.PieceColor color, Point square) 
        {
            this.type = type;
            WhichColor = color;
            this.square = square;
            HasMoved = false;
            ToMove = false;

            /* PictureBox properties */
            LoadImage(); 
            BackColor = Color.Transparent; 
            DoubleBuffered = true;
            
        }

        /**
         * Load an image for this piece based on the piece type and color
         */
        private void LoadImage()
        {
            Image = (Bitmap)Resources.ResourceManager.GetObject(type.ToString().ToLower() + '_' + WhichColor.ToString().ToLower());
            Size = new Size(Image.Width, Image.Height);
        }

        #region Drag & Drop Functionality

        private bool mouseDown = false;
        private Point startingLocation; 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if(ToMove && e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                startingLocation = new Point(e.X, e.Y); 
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(mouseDown)
            {
                Location = new Point(Location.X + (e.X - startingLocation.X), Location.Y + (e.Y - startingLocation.Y)); 
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e); 
            mouseDown = false; 
        }
        #endregion

        /**
         * Gets the grid location of the piece on the board.
         */
        public Point GetCurrentGrid()
        {
            return new Point((Location.X + 25) / Chess.SQUARE_SIZE, 7 - (Location.Y + 40) / Chess.SQUARE_SIZE); 
        }

        /**
         * Returns the location a piece on the board in pixels based on the grid location.
         * @param grid - the grid location of a piece.
         * @return - the location a piece should be placed in pixels
         */
        public void RefreshBoardLocation(Point newSquare)
        {
            square = newSquare; 
            Location = new Point(Chess.SQUARE_SIZE / 2 - Width / 2 + newSquare.X * 90, Chess.BOARD_SIZE - 89 - (newSquare.Y * 90));
        }

        /**
         * Promote a pawn if it makes it to the other side of the board.
         * @param to - the type of piece to promote to 
         */
        public void Promote(Chess.PieceType to)
        {
            if (Type == Chess.PieceType.PAWN)
            {
                type = to;
                LoadImage();
            }
        }

    }
}

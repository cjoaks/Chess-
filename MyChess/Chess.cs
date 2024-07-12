using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace MyChess
{
    class Chess : Panel
    {
        public enum PieceType
        {
            ROOK, KNIGHT, BISHOP, QUEEN, KING, PAWN 
        }

        public enum PieceColor
        {
            WHITE, BLACK
        }

        #region Public Fields

        public const int BOARD_SIZE = 720;
        public const int SQUARE_SIZE = 90;
        public static PieceColor Perspective;

        #endregion
        #region Private Fields

        private Moves moves;

        private List<Piece> activePieces;

        private List<Point> selectedMoves;

        private Color darkSquareColor;
        private Color lightSquareColor;
        private Color selectedMovesColor;

        private readonly Dictionary<PieceColor, CaptureBox> captureBoxes;
        private readonly Dictionary<PieceColor, int> materialCaptured;

        private PieceColor toMove;

        private readonly ComboBox promotionSelector;
        private Piece toPromote;  

        #endregion
        #region Accessors

        public Color DarkSquareColor
        {
            get
            {
                return darkSquareColor;
            }
            set
            {
                darkSquareColor = value;
                Invalidate();
            }
        }
        public Color LightSquareColor
        {
            get
            {
                return lightSquareColor;
            }
            set
            {
                lightSquareColor = value; 
                BackColor = value;
                Invalidate();
            }
        }
        public Color LegalMovesColor
        {
            get 
            { 
                return selectedMovesColor; 
            }
            set 
            { 
                selectedMovesColor = value; 
            }
        }

        #endregion

        public Chess(CaptureBox whiteCaptureBox, CaptureBox blackCaptureBox)
        {
            /* Panel properties */
            Size = new Size(720, 720);
            Location = new Point(652, 12);
            Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            LightSquareColor = Color.FromArgb(153, 232, 227);
            DarkSquareColor = Color.FromArgb(35, 52, 204);
            LegalMovesColor = Color.FromArgb(18, 19, 89);
 
            selectedMoves = new List<Point>();

            captureBoxes = new Dictionary<PieceColor, CaptureBox>
            {
                [PieceColor.WHITE] = whiteCaptureBox,
                [PieceColor.BLACK] = blackCaptureBox
            };

            promotionSelector = new ComboBox
            {
                FormattingEnabled = true,
                Size = new Size(SQUARE_SIZE, 24),
                Visible = false
            };
            promotionSelector.Items.AddRange(new object[] 
            {
                PieceType.KNIGHT, 
                PieceType.BISHOP, 
                PieceType.ROOK, 
                PieceType.QUEEN
            });
            promotionSelector.SelectedIndexChanged += PromotePawn;

            materialCaptured = new Dictionary<PieceColor, int>();
            
            NewGame(); 
        }

        public void NewGame()
        {
            Controls.Clear();
            Controls.Add(promotionSelector);

            activePieces = new List<Piece>
            {
                new Piece(PieceType.KING, PieceColor.WHITE, new Point(4, 0)),
                new Piece(PieceType.KING, PieceColor.BLACK, new Point(4, 7)),
                new Piece(PieceType.QUEEN, PieceColor.WHITE, new Point(3, 0)),
                new Piece(PieceType.QUEEN, PieceColor.BLACK, new Point(3, 7)),
                new Piece(PieceType.BISHOP, PieceColor.WHITE, new Point(5, 0)),
                new Piece(PieceType.BISHOP, PieceColor.WHITE, new Point(2, 0)),
                new Piece(PieceType.BISHOP, PieceColor.BLACK, new Point(2, 7)),
                new Piece(PieceType.BISHOP, PieceColor.BLACK, new Point(5, 7)),
                new Piece(PieceType.KNIGHT, PieceColor.WHITE, new Point(6, 0)),
                new Piece(PieceType.KNIGHT, PieceColor.WHITE, new Point(1, 0)),
                new Piece(PieceType.KNIGHT, PieceColor.BLACK, new Point(6, 7)),
                new Piece(PieceType.KNIGHT, PieceColor.BLACK, new Point(1, 7)),
                new Piece(PieceType.ROOK, PieceColor.WHITE, new Point(7, 0)),
                new Piece(PieceType.ROOK, PieceColor.WHITE, new Point(0, 0)),
                new Piece(PieceType.ROOK, PieceColor.BLACK, new Point(0, 7)),
                new Piece(PieceType.ROOK, PieceColor.BLACK, new Point(7, 7))
            };
            for (int i = 0; i < 8; ++i)
            {
                activePieces.Add(new Piece(PieceType.PAWN, PieceColor.WHITE, new Point(i, 1)));
                activePieces.Add(new Piece(PieceType.PAWN, PieceColor.BLACK, new Point(i, 6)));
            }

            /* Setup pieces on board */
            foreach (Piece p in activePieces)
            {
                p.RefreshBoardLocation(p.Square);
                p.MouseDown += Piece_MouseDown;
                p.MouseUp += Piece_MouseUp;
                if (Window.FreeMove || p.WhichColor == PieceColor.WHITE)
                {
                    p.ToMove = true;
                }
                Controls.Add(p);
            }

            moves = new Moves(activePieces);

            /* Reset captures and material */
            materialCaptured[PieceColor.WHITE] = 0;
            materialCaptured[PieceColor.BLACK] = 0;
            captureBoxes[PieceColor.WHITE].Reset();
            captureBoxes[PieceColor.BLACK].Reset();

            Perspective = PieceColor.WHITE;
            toMove = PieceColor.WHITE;
        }

        #region Event Handlers

        /**
         * Paints the squares and legal moves onto the board.
         */
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics; 
            using (Pen p = new Pen(darkSquareColor))
            using (SolidBrush br = new SolidBrush(darkSquareColor))
            {
                int square = 0;
                for (int r = 0; r < 8; ++r, ++square)
                {
                    for (int c = 0; c < 8; ++c, ++square)
                    {
                        if (square % 2 == 1)
                        {
                            Rectangle rec = new Rectangle(c * SQUARE_SIZE, r * SQUARE_SIZE, SQUARE_SIZE, SQUARE_SIZE);
                            g.DrawRectangle(p, rec);
                            g.FillRectangle(br, rec);
                        }
                    }
                }
            }
            if (Window.ShowLegalMoves && selectedMoves.Count > 0)
            {
                using (Pen p = new Pen(selectedMovesColor))
                using (SolidBrush br = new SolidBrush(selectedMovesColor))
                {
                    foreach (Point move in selectedMoves)
                    {
                        Rectangle rec = new Rectangle(15 + move.X * SQUARE_SIZE, Height - 75 - move.Y * SQUARE_SIZE, 60, 60);
                        g.DrawEllipse(p, rec);
                        g.FillEllipse(br, rec);
                    }
                }
            }
        }

        /**
         * Event handler for when an active piece is clicked
         */
        private void Piece_MouseDown(object sender, EventArgs e)
        {
            Piece p = (Piece)sender; 
            if(Window.FreeMove || p.WhichColor == toMove)
            {
                selectedMoves = moves.CalculatePieceMoves(p, false);
                Invalidate(); 
            }
        }

        
        /**
         * Event handler for when an active piece is released.
         */
        private void Piece_MouseUp(object sender, EventArgs e)
        {
            MakeMove((Piece)sender);
            selectedMoves.Clear();
            Invalidate(); 
        }

        /**
         * Called after a selection is made in the promotion selector.
         */
        private void PromotePawn(object sender, EventArgs e)
        {
            promotionSelector.Visible = false;
            toPromote.Promote((PieceType)((ComboBox)sender).SelectedItem);
            captureBoxes[PieceColor.WHITE].UpdateMaterial(activePieces);
            captureBoxes[PieceColor.BLACK].UpdateMaterial(activePieces);
        }
        

        /**
         * Called by event handler for the Flip button in the control box.
         * Flips the board from a white to black perspective or vice versa.
         */
        public void FlipBoard()
        {
            activePieces.ForEach(ap => ap.RefreshBoardLocation(new Point(Math.Abs(7 - ap.Square.X), Math.Abs(7 - ap.Square.Y))));
            if(moves.EnPassant.HasValue)
            {
                Point sq = moves.EnPassant.Value;
                moves.EnPassant = new Point(Math.Abs(7 - sq.X), Math.Abs(7 - sq.Y)); 
            }
            Perspective = OppositeColor(Perspective);
            captureBoxes[PieceColor.WHITE].Flip();
            captureBoxes[PieceColor.BLACK].Flip();
            Invalidate();
        }

        /**
         * Called by the event handler for the Free move checkbox when it is checked.
         * Allows all pieces to be moved around the board without implementing turns. 
         */
        public void AllowFreeMove()
        {
            foreach(Piece ap in activePieces)
            {
                ap.MouseDown += Piece_MouseDown;
                ap.MouseUp += Piece_MouseUp;
                ap.ToMove = true;
            } 
        }

        /**
         * Called by the event handler for the Free move checkbox when it is checked.
         * Allows only pieces of the passed color to be moved around the board and sets toMove to said color. 
         * @param color - the color whose turn it is to move.
         */
        public void DisallowFreeMove(PieceColor color)
        {
            SetPiecesToMove(color); 
        }

        #endregion

        /**
         * Called after the mouseUp event is raised on a piece. Tries to make the intended move if it is legal.
         * @param p - The piece moved
         */
        private void MakeMove(Piece p)
        {
            Point oldSquare = p.Square;
            Point newSquare = p.GetCurrentGrid();
            Piece toCapture = moves.PieceAt(newSquare);
            p.RefreshBoardLocation(selectedMoves.Contains(newSquare) ? newSquare : p.Square);
            if (!oldSquare.Equals(p.Square))
            {
                p.HasMoved = true;
                /* castling */
                int distanceMoved = newSquare.X - oldSquare.X;
                if (p.Type == PieceType.KING && Math.Abs(distanceMoved) == 2)
                {
                    Piece rook = moves.PieceAt(new Point((distanceMoved > 0) ? 7 : 0, p.Square.Y));
                    rook.RefreshBoardLocation(new Point(p.Square.X + distanceMoved * -1 / 2, p.Square.Y)); 
                }
                /* pawn promotion */
                if(p.Type == PieceType.PAWN && (p.Square.Y == 0 || p.Square.Y == 7))
                {
                    toPromote = p;
                    promotionSelector.Location = GetSquareLocation(p.Square); 
                    promotionSelector.Visible = true; 
                }
                /* en passant */
                if(p.Type == PieceType.PAWN && newSquare.Equals(moves.EnPassant))
                {
                    toCapture = moves.PieceAt(new Point(newSquare.X, oldSquare.Y)); 
                }
                distanceMoved = newSquare.Y - oldSquare.Y;
                if (p.Type == PieceType.PAWN && Math.Abs(distanceMoved) == 2)
                {
                    moves.EnPassant = new Point(oldSquare.X, oldSquare.Y + (distanceMoved > 0 ? 1 : -1));
                }
                else
                {
                    moves.EnPassant = null; 
                } 
                if (toCapture != null)
                {
                    CapturePiece(p, toCapture);
                }
                if(!Window.FreeMove)
                {
                    SetPiecesToMove(OppositeColor(p.WhichColor)); 
                    if(Window.AutoFlip)
                    {
                        // prevent board from flipping before the move has actually been made.
                        FlipBoard();
                    }
                    if(moves.IsCheckMate(OppositeColor(p.WhichColor)))
                    {
                        MessageBox.Show("Checkmate, " + p.WhichColor + " wins!", "Chess#", MessageBoxButtons.OK); 
                    }
                }
            }
        }

       

        /**
         * Allow the pieces on the side of the color to move to be moved, disallow all others.
         * @param color - the side whose turn it is to move
         */
        private void SetPiecesToMove(PieceColor color)
        {
            foreach(Piece ap in activePieces)
            {
                if(ap.WhichColor == color)
                {
                    ap.MouseDown += Piece_MouseDown;
                    ap.MouseUp += Piece_MouseUp;
                    ap.ToMove = true;
                }
                else
                {
                    ap.MouseDown -= Piece_MouseDown;
                    ap.MouseUp -= Piece_MouseUp;
                    ap.ToMove = false; 
                }
            }
            toMove = color; 
        }


        /**
         * @param c- a piece color
         * @return the opposite piece color
         */
        public static PieceColor OppositeColor(PieceColor c)
        {
            return (c == PieceColor.WHITE) ? PieceColor.BLACK : PieceColor.WHITE;
        }

        /**
         * @param p - the piece to be valued
         * @return the material value based on the type of the passed piece
         */
        public static int GetMaterialValue(Piece p) 
        {
            switch(p.Type)
            {
                case PieceType.PAWN: return 1;
                case PieceType.KNIGHT: return 3;
                case PieceType.BISHOP: return 3;
                case PieceType.KING: return 4;
                case PieceType.ROOK: return 5;
                case PieceType.QUEEN: return 9; 
            }
            return 0; 
        }

        /**
         * Capture a piece on the board and add it to the captured material.
         * @param captor - the caturing piece
         * @param captive - the piece getting captured
         */
        private void CapturePiece(Piece captor, Piece captive)
        {
            Controls.Remove(captive);
            activePieces.Remove(captive);
            materialCaptured[captive.WhichColor] += GetMaterialValue(captive);
            captureBoxes[captor.WhichColor].AddPiece(captive);
            captureBoxes[PieceColor.WHITE].UpdateMaterial(activePieces);
            captureBoxes[PieceColor.BLACK].UpdateMaterial(activePieces); 
        }    

        public static Point GetSquareLocation(Point square)
        {
            return new Point(square.X * SQUARE_SIZE, BOARD_SIZE - SQUARE_SIZE - square.Y * SQUARE_SIZE); 
        } 
    }
}

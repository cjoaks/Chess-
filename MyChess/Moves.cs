using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing; 
using System.Text;
using System.Threading.Tasks;

namespace MyChess
{
    class Moves
    {
        private readonly List<Piece> activePieces;
        private readonly Dictionary<Chess.PieceType, List<Point>> patterns;

        public Point? EnPassant; 

        public Moves(List<Piece> activePieces)
        {
            patterns = new Dictionary<Chess.PieceType, List<Point>>
            {
                // [Chess.PieceType.PAWN] - Pawns moves are all determined without a predefined pattern.
                [Chess.PieceType.KNIGHT] = new List<Point>
                {
                        new Point(1, 2),      new Point(-1, 2),
                    new Point(-2, 1),             new Point(2, 1),
                    
                    new Point(-2, -1),            new Point(2, -1),
                        new Point(-1, -2),    new Point(1, -2)
                },
                [Chess.PieceType.BISHOP] = new List<Point>
                {
                    new Point(-1, 1), new Point(1, 1),  
                    new Point(-1, -1), new Point(1, -1)
                },
                [Chess.PieceType.ROOK] = new List<Point>
                {
                             new Point(0, 1), 
                    new Point(-1, 0), new Point(1, 0),
                             new Point(0, -1) 
                },
                //[Chess.PieceType.QUEEN] - The queen pattern is the result of combining the bishop and rook patterns.
                [Chess.PieceType.KING] = new List<Point> 
                {
                    new Point(-1, 1), new Point(0, 1), new Point(1, 1),
                    new Point(-1, 0),                  new Point(1, 0),
                    new Point(-1, -1), new Point(0, -1), new Point(1, -1)
                }
            };
            this.activePieces = activePieces; 
        }
 

        #region Piece Logic
        //tracy wuz here
        /**
       * Find all of the legal moves for the passed piece.
       * @param p - the piece to be examined 
       * @param defending - whether the passed piece is defending
       * @return - A list of moves attacking moves or the sqaures covered for the given piece.
       */
        public List<Point> CalculatePieceMoves(Piece p, bool defending)
        { 
            List<Point> pieceMoves = new List<Point>();
        
            if (p.Type == Chess.PieceType.PAWN)
            {
                int direction = (p.WhichColor == Chess.Perspective) ? 1 : -1;
                Point move = new Point(p.Square.X, p.Square.Y + direction);
                if (defending)
                {
                    pieceMoves.Add(new Point(move.X - 1, move.Y));
                    pieceMoves.Add(new Point(move.X + 1, move.Y));
                }
                else
                {
                    for (int i = -1; i <= 1; ++i)
                    {
                        move.X += i;
                        Piece pa = PieceAt(move);
                        if (move.Equals(EnPassant) || i == 0 && pa == null || i != 0 && pa != null && pa.WhichColor != p.WhichColor)
                        {
                            pieceMoves.Add(move);
                            Point firstMove = new Point(move.X, move.Y + direction);
                            if (i == 0 && !p.HasMoved && PieceAt(firstMove) == null)
                            {
                                pieceMoves.Add(firstMove);
                            }
                        }
                        move.X = p.Square.X;
                    }
                }

            }
            else if(p.Type == Chess.PieceType.KNIGHT || p.Type == Chess.PieceType.KING)
            {
                foreach (Point offset in patterns[p.Type])
                {
                    Point move = new Point(p.Square.X + offset.X, p.Square.Y + offset.Y);
                    if (MoveIsOnBoard(move) && (defending || PieceAt(move)?.WhichColor != p.WhichColor))
                    {
                        pieceMoves.Add(move);
                    }
                }
                // Castling 
                if(p.Type == Chess.PieceType.KING && !p.HasMoved && !defending)
                {
                    List<Point> protectedSquares = CalculateProtectedSquares(Chess.OppositeColor(p.WhichColor));
                    foreach (Piece rook in new Piece[] { 
                        PieceAt(new Point(0, p.Square.Y)), 
                        PieceAt(new Point(7, p.Square.Y)) })
                    { 
                        if (rook != null && !rook.HasMoved)
                        {
                            int direction = (rook.Square.X == 0) ? -1 : 1;
                            bool canCastle = true;
                            for (int x = p.Square.X; x != rook.Square.X; x += direction)
                            {
                                Point square = new Point(x, p.Square.Y);
                                if((x != p.Square.X && PieceAt(square) != null) || protectedSquares.Contains(square))
                                {
                                    canCastle = false;
                                    break; 
                                }
                            }
                            if(canCastle)
                            {
                                pieceMoves.Add(new Point(p.Square.X + (direction * 2), p.Square.Y)); 
                            }
                        }
                    }
                }
            }
            else if (p.Type == Chess.PieceType.BISHOP || p.Type == Chess.PieceType.ROOK)
            {
                pieceMoves.AddRange(AddDirectionalPattern(p, p.Type, defending));
            }
            else if (p.Type == Chess.PieceType.QUEEN)
            {
                pieceMoves.AddRange(AddDirectionalPattern(p, Chess.PieceType.BISHOP, defending));
                pieceMoves.AddRange(AddDirectionalPattern(p, Chess.PieceType.ROOK, defending));
            }
          
            return (defending) ? pieceMoves : AccountForChecks(p, pieceMoves);
        }
        //macheen gunners annonymoose
        /**
         * Add legal moves based on the passed directional pattern (rook or bishop, queen has both)
         * @param p - the piece to get moves for
         * @param pattern - the pattern to add
         * @param defending - true if the passed piece is defending, flase if attacking.
         * @return - a list of potential moves
         */
        private List<Point> AddDirectionalPattern(Piece p, Chess.PieceType pattern, bool defending)
        {
            List<Point> moves = new List<Point>();
            void AddDirection(Point sofar, Point offset)
            {
                Point move = new Point(sofar.X + offset.X, sofar.Y + offset.Y);
                Piece pa = PieceAt(move);
                if (MoveIsOnBoard(move) && (defending || pa?.WhichColor != p.WhichColor))
                {
                    moves.Add(move);
                    if (pa == null)
                    {
                        AddDirection(move, offset);
                    }
                }
            }
            patterns[pattern].ForEach(dir => AddDirection(p.Square, dir));
            return moves;
        }

        /**
         * Account for checks after all moves have been determined for a piece.
         * @param p - the piece the moves belong to
         * @param pieceMoves - the moves to check 
         */
        private List<Point> AccountForChecks(Piece p, List<Point> pieceMoves)
        {
            List<Point> potentialMoves = new List<Point>(pieceMoves);
            Point current = p.Square;
            foreach (Point move in pieceMoves)
            {
                /* Remove piece before checking for check to ensure a pinned piece can capture it's attacker if possible */
                Piece counterAttack = PieceAt(move);
                if (counterAttack != null)
                {
                    activePieces.Remove(counterAttack);
                }
                p.Square = move;
                if (IsCheck(p.WhichColor))
                {
                    potentialMoves.Remove(move);
                }
                if (counterAttack != null)
                {
                    activePieces.Add(counterAttack);
                }
            }
            p.Square = current;
            return potentialMoves;
        }

        /**
         * Calculate and return a list of the squares protected by the passed color.
         * @param active pieces - all of the pieces currently on the board.
         * @param defender - the color of the defending pieces.
         * @return - a list of the squares protected by the defender.
         */
        public List<Point> CalculateProtectedSquares(Chess.PieceColor color)
        {
            List<Point> defended = new List<Point>();
            foreach (Piece p in activePieces.FindAll((piece) => piece.WhichColor == color))
            {
                defended.AddRange(CalculatePieceMoves(p, true));
            }
            return defended;
        }

        /**
         * Determine if it is checkmate for the passed color by checking for legal offesnive moves.
         * @param color - The color to be examined
         */
        public bool IsCheckMate(Chess.PieceColor mated)
        {
            List<Point> legalMoves = new List<Point>();
            foreach (Piece p in activePieces.FindAll((piece) => piece.WhichColor == mated))
            {
                legalMoves.AddRange(CalculatePieceMoves(p, false));
            }
            return legalMoves.Count == 0;
        }

        /**
         * Get the piece occupying the passed sqaure.
         * @param sqaure - The sqaure to check
         * @return - The piece occupying the passed sqaure, or null.
         */
        public Piece PieceAt(Point square)
        {
            return activePieces.Find((piece) => piece.Square.Equals(square));
        }
        #endregion
        #region Helper Methods

        /**
         * Check if a given move is on the board.
         * @param move - The move to be checked.
         */
        private static bool MoveIsOnBoard(Point move)
        {
            return (move.X >= 0 && move.X <= 7 && move.Y >= 0 && move.Y <= 7);
        }

        /**
         * Determines if the passed color is in check.
         * @param squares - the list of squares protected by the defending color
         * @param color - the color to be examined
         * @return - whether the passed color is in check
         */
        private bool IsCheck(Chess.PieceColor color)
        {
            foreach (Point square in CalculateProtectedSquares(Chess.OppositeColor(color)))
            {
                Piece p = PieceAt(square);
                if (p?.Type == Chess.PieceType.KING && p.WhichColor == color)
                {
                    return true;
                }
            }
            return false;
        }

      

        #endregion
    }
}

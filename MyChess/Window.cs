using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyChess
{
    public partial class Window : Form
    {
        private readonly Chess chess;
        private readonly List<Control> text, buttons;

        public static bool ShowLegalMoves = true;
        public static bool FreeMove = false;
        public static bool AutoFlip = false; 

        private readonly Label[] ranks, files;
        private const int RANK_LOCATION_X = 620, FILE_LOCATION_Y = 735;
        private int[] rankLocationY, fileLocationX;

        public Window()
        {
            InitializeComponent();
            Visible = true;

            CaptureBox whiteCaptureBox = new CaptureBox(Chess.PieceColor.WHITE);
            Controls.Add(whiteCaptureBox);
            CaptureBox blackCaptureBox = new CaptureBox(Chess.PieceColor.BLACK);
            Controls.Add(blackCaptureBox);
            chess = new Chess(whiteCaptureBox, blackCaptureBox);
            Controls.Add(chess);

            text = new List<Control>
            {
                controlGroupBox,
                whiteCaptureBox,
                blackCaptureBox,
                showLegalMovesCheckBox
            };
            foreach (Control c in this.Controls)
            {
                if (c.GetType() == typeof(Label))
                {
                    text.Add(c);
                }
            }

            buttons = new List<Control>
            {
                resetButton, flipButton
            };

            windowColorButton.BackColor = this.BackColor;
            lightSquareColorButton.BackColor = this.chess.LightSquareColor;
            darkSquareColorButton.BackColor = this.chess.DarkSquareColor;
            legalMovesColorButton.BackColor = this.chess.LegalMovesColor;

            ranks = new Label[]
            {
                rank1, rank2, rank3, rank4, rank5, rank6, rank7, rank8
            };
            rankLocationY = ranks.Select((rank) => rank.Location.Y).ToArray();
            files = new Label[]
            {
                fileA, fileB, fileC, fileD, fileE, fileF, fileG, fileH
            };
            fileLocationX = files.Select((file) => file.Location.X).ToArray();
        }

        /**
         * Allows the user to select a custom color from the dialog, then returns that value.
         */
        private Color SelectColorFromDialog()
        {
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowDialog(this);
            cd.Dispose();
            return cd.Color;
        }

        #region Event Handlers
        private void windowColorButton_Click(object sender, EventArgs e)
        {
            Color newColor = SelectColorFromDialog();
            ((Button)sender).BackColor = newColor;
            BackColor = newColor;
            Color inverted = Color.FromArgb(255 - newColor.R, 255 - newColor.G, 255 - newColor.B);
            text.ForEach(c => c.ForeColor = inverted);
            Color offset;
            try
            {
                offset = Color.FromArgb(newColor.R + 30, newColor.G + 30, newColor.B + 30);
            }
            catch
            {
                offset = newColor;
            }
            buttons.ForEach(b => b.BackColor = offset);
        }

        private void lightSquareColorButton_Click(object sender, EventArgs e)
        {
            Color newColor = SelectColorFromDialog();
            ((Button)sender).BackColor = newColor;
            this.chess.LightSquareColor = newColor;
        }

        private void darkSquareColorButton_Click(object sender, EventArgs e)
        {
            Color newColor = SelectColorFromDialog();
            ((Button)sender).BackColor = newColor;
            this.chess.DarkSquareColor = newColor;
        }

        private void legalMovesColorButton_Click(object sender, EventArgs e)
        {
            Color newColor = SelectColorFromDialog();
            ((Button)sender).BackColor = newColor;
            this.chess.LegalMovesColor = newColor;
        }

        private void autoFlipCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AutoFlip = ((CheckBox)sender).Checked; 
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to start a new game?", "Chess#", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (Chess.Perspective == Chess.PieceColor.BLACK)
                {
                    flipButton_Click(sender, e);
                }
                chess.NewGame();
            }
        }

        private void flipButton_Click(object sender, EventArgs e)
        {
            chess.FlipBoard();
            rankLocationY = rankLocationY.Reverse().ToArray();
            fileLocationX = fileLocationX.Reverse().ToArray();
            for (int i = 0; i < 8; ++i)
            {
                ranks[i].Location = new Point(RANK_LOCATION_X, rankLocationY[i]);
                files[i].Location = new Point(fileLocationX[i], FILE_LOCATION_Y);
            }
        }

        private void showLegalMovesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowLegalMoves = ((CheckBox)sender).Checked;
        }

        private void freeMoveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FreeMove = ((CheckBox)sender).Checked;
            if (FreeMove)
            {
                chess.AllowFreeMove();
            }
            else
            {
                chess.DisallowFreeMove(MessageBox.Show("White to move?", "Chess#", MessageBoxButtons.YesNo) == DialogResult.Yes ? Chess.PieceColor.WHITE : Chess.PieceColor.BLACK);
            }
        }
        #endregion
    }
}

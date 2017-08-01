using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Collections;
using CircleCrossWinApp.GameModel.CircleCross;

namespace CircleCrossWinApp
{
    public partial class Form1 : Form
    {
        private static readonly List<List<Label>> GameLabelBoard = new List<List<Label>>();

        public Form1()
        {
            InitializeComponent();
            for (int row = 0; row < tableLayoutPanel1.RowCount; row++)
            {
                var currentRow = new List<Label>();
                for (int column = 0; column < tableLayoutPanel1.ColumnCount; column++)
                {
                    Control currentCotrol = tableLayoutPanel1.GetControlFromPosition(column, row);
                    if (currentCotrol != null)
                    {
                        if (currentCotrol is Label)
                        {
                            currentRow.Add((Label)currentCotrol);
                            currentCotrol.Click += new EventHandler(cellLabelClicked);
                        }
                    }
                }
                GameLabelBoard.Add(currentRow);
            }

            GameLabelBoard.Reverse();

            ToolStripMenuItem tsItem = new ToolStripMenuItem();
            tsItem.Alignment = ToolStripItemAlignment.Right;
            menuStrip1.Items.Add(tsItem);
        }

        private void tsmI1Child1_Click(object sender, EventArgs e)
        {
            CircleCrossBoard.Current.Dispose();
            CircleCrossBoard.Current.Start();

            lblTurn.Text = CircleCrossBoard.Current.CurrentChessType.HasFlag(ChessType.None) ? String.Empty : CircleCrossBoard.Current.CurrentChessType.ToString();

            foreach (List<Label> gameLabelRow in GameLabelBoard)
            {
                foreach (Label gameLabel in gameLabelRow)
                {
                    gameLabel.Text = String.Empty;
                    gameLabel.Enabled = CircleCrossBoard.Current.IsStarted;
                }
            }

            timer2.Enabled = CircleCrossBoard.Current.IsStarted;

            if (timer2.Enabled)
            {
                timer2.Start();
            }
        }

        private void tsmI1Child2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cellLabelClicked(object sender, EventArgs e)
        {
            Label targetLabel = (Label)sender;

            for (int column = 0; column < GameLabelBoard.Count; column++)
            {
                for (int row = 0; row < GameLabelBoard[column].Count; row++)
                {
                    if (targetLabel == GameLabelBoard[column][row])
                    {

                        targetLabel.Text = CircleCrossBoard.Current.CurrentChessType == ChessType.Circle ? "O" : "X";
                        CircleCrossBoard.Current.TryAddChess(new Point(row + 1, column + 1));
                        targetLabel.Enabled = false;
                        break;
                    }
                }
            }
            lblTurn.Text = CircleCrossBoard.Current.CurrentChessType.HasFlag(ChessType.None) ? String.Empty : CircleCrossBoard.Current.CurrentChessType.ToString();

            if (CircleCrossBoard.Current.IsEnd)
            {
                for (int i = 0; i < GameLabelBoard.Count; i++)
                {
                    for (int j = 0; j < GameLabelBoard[i].Count; j++)
                    {
                        GameLabelBoard[i][j].Enabled = false;
                    }
                }
                lblTurn.Text = (int)CircleCrossBoard.Current.WinType > 0 ? String.Format("{0} Win", CircleCrossBoard.Current.WinType.ToString()) : "Draw Game";
                MessageBox.Show((int)CircleCrossBoard.Current.WinType > 0 ? String.Format("{0} Win", CircleCrossBoard.Current.WinType.ToString()) : "Draw Game", "End Game");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            menuStrip1.Items[menuStrip1.Items.Count - 1].Text = String.Format("Current Time : {0}", DateTime.Now.ToLongTimeString());
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            lblPlayTime.Text = new DateTime((DateTime.Now - CircleCrossBoard.Current.CreateTime.Value).Ticks).ToLongTimeString();
            if (CircleCrossBoard.Current.IsEnd)
            {
                timer2.Enabled = false;
                timer2.Stop();
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}

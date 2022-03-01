using Caro.Model;

namespace Caro.Helper
{
	public class EvalBoard
    {
        #region Fields
        const int col = 20, row = 20;
        int[,] _board;
        #endregion

        #region Properties
        public int[,] Board
        {
            get => _board;
            set
            {
                int n = Board.Length;
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                    {
                        _board[i, j] = value[i, j];
                    }
            }
        }
        #endregion

        #region Constructor
        public EvalBoard()
        {
            _board = new int[col, row];
            ResetBoard();
        }
        #endregion

        #region Method
        public void ResetBoard()
        {
            for (int i = 0; i < col; i++)
                for (int j = 0; j < row; j++)
                {
                    _board[i, j] = 0;
                }
        }

        public Point MaxPos()
        {
            int max = 0;
            Point p = new Point();

            for (int i = 0; i < col; i++)
                for (int j = 0; j < row; j++)
                {
                    if (_board[i, j] > max)
                    {
                        p.X = i;
                        p.Y = j;
                        max = _board[i, j];
                    }
                }

            return p;
        }
        #endregion
    }
}
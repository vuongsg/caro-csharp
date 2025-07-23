namespace Caro.Model
{
	public struct Point
    {
        #region Fields
        int _x;
        int _y;
        #endregion

        #region Properties
        public int X
        {
            get => _x;
            set
            {
                if (_x != value)
                    _x = value;
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                if (_y != value)
                    _y = value;
            }
        }
        #endregion

        #region Constructors

        public Point(int x, int y)
        {
            this._x = x;
            this._y = y;
        }
        #endregion
    }
}
namespace Caro.Model
{
	public class Player
    {
        #region Fields
        string _symbol;
        #endregion

        #region Properties
        public string Symbol
        {
            get => _symbol;
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                }
            }
        }
        #endregion

        #region Constructor
        public Player(string symbol)
        {
            this._symbol = symbol;
        }
        #endregion
    }
}
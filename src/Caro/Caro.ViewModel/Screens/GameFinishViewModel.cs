using Caro.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Caro.ViewModel
{
    public class GameFinishViewModel
    {
        #region Fields
        Window win;
        #endregion

        #region Properties
        public ICommand LoadCommand { get; set; }
        public ICommand TitleBar_MouseDownCommand { get; set; }
        public ICommand accessMessage_LoadCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        #endregion

        #region Constructor
        public GameFinishViewModel()
        {
            LoadCommand = new RelayCommand<Window>(m => m != null, m => Load(m));
            TitleBar_MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(m => m != null, m => TitleBar_MouseDown(m));
            accessMessage_LoadCommand = new RelayCommand<AccessText>(m => m != null, m => accessMessage_Load(m));
            ExitCommand = new RelayCommand<string>(m => true, m => Exit());
        }
        #endregion

        #region Methods
        private void Load(Window w)
        {
            win = w;
            win.Left = win.Top = 2;
        }

        private void TitleBar_MouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                win.DragMove();
        }

        private void accessMessage_Load(AccessText at)
        {
            switch (GameFinishStatus.Status)
			{
                case GameState.XWin:
                    at.Text = "You win. Congratulations!";
                    break;
                case GameState.OWin:
                    at.Text = "You lose. Don't be sad!";
                    break;
                default:
                    at.Text = "Game draws!";
                    break;
			}
        }

        private void Exit()
        {
            win?.Close();
        }
        #endregion
    }
}
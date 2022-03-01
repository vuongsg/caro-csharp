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
    public class ScoreViewModel
    {
        #region Fields
        Window win;
        #endregion

        #region Properties
        public ICommand LoadCommand { get; set; }
        public ICommand TitleBar_MouseDownCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        #endregion

        #region Constructor
        public ScoreViewModel()
        {
            LoadCommand = new RelayCommand<Window>(m => m != null, m => Load(m));
            TitleBar_MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(m => m != null, m => TitleBar_MouseDown(m));
            ExitCommand = new RelayCommand<string>(m => m != null, m => Exit());
        }
        #endregion

        #region Methods
        private void Load(Window w)
        {
            win = w;
        }

        private void TitleBar_MouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                win.DragMove();
            }
        }

        private void Exit()
        {
            win?.Close();
        }
        #endregion
    }
}
using Caro.Models;
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
    public class AskExitViewModel
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
        public AskExitViewModel()
        {
            LoadCommand = new RelayCommand<Window>(m => m != null, m => Load(m));
            TitleBar_MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(m => m != null, m => TitleBar_MouseDown(m));
            ExitCommand = new RelayCommand<Button>(m => true, m => Exit(m));
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

        private void Exit(Button sender)
        {
            Button btn = sender as Button;
            if (btn.Name == "btnYes")
            {
                WantToExit.Yes = true;
            }
            else    // btnNo is clicked or user press "Alt+F4"
            {
                WantToExit.Yes = false;
            }

            if (win != null)
            {
                win.Close();
            }
        }
        #endregion
    }
}
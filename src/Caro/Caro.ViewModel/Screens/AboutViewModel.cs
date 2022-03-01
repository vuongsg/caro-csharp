using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Caro.ViewModel
{
    public class AboutViewModel
    {
        #region Fields
        Window win;
        #endregion

        #region Properties
        public ICommand LoadCommand { get; set; }
        public ICommand TitleBar_MouseDownCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand Link_RequestNavigateCommand { get; set; }
        #endregion

        #region Constructor
        public AboutViewModel()
        {
            LoadCommand = new RelayCommand<Window>(m => m != null, m => Load(m));
            TitleBar_MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(m => m != null, m => TitleBar_MouseDown(m));
            ExitCommand = new RelayCommand<string>(m => m != null, m => Exit());
            Link_RequestNavigateCommand = new RelayCommand<Hyperlink>(m => m != null, m => Link_RequestNavigate(m));
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
            if (win != null)
            {
                win.Close();
            }
        }

        private void Link_RequestNavigate(Hyperlink link)
        {
            //for .NET Core need to add UseShellExecute = true
            //see https://docs.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(link.NavigateUri.ToString()) { UseShellExecute = true });
        }
        #endregion
    }
}
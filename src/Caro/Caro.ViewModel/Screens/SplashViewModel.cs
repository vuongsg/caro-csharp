using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;       // Visibility
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;

namespace Caro.ViewModel
{
    public class SplashViewModel
    {
        #region Fields
        public static event EventHandler ShowMainWindow;
        List<Label> lstLabels;
        DispatcherTimer timer;
        int index;
        int countLabels;
        #endregion

        #region Properties
        public ICommand LoadCommand { get; set; }
        #endregion

        #region Constructor
        public SplashViewModel()
        {
            LoadCommand = new RelayCommand<UIElementCollection>(m => m != null, m => Load(m));
        }
        #endregion

        #region Methods
        private void Load(UIElementCollection collection)
        {
            lstLabels = new List<Label>();
            foreach (var element in collection)
            {
                Label label = element as Label;     // use this cast, it will return null if casting incorrectly
                if (label != null)
                {
                    label.Visibility = Visibility.Hidden;
                    lstLabels.Add(label);
                }
            }

            countLabels = lstLabels.Count;
            lstLabels = lstLabels.OrderBy(m => int.Parse(m.Name.Substring(3))).ToList();    // because name of each label starts with "lbl",
                                                                                            // hence we need to trim three first letters
            index = 0;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(400);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (index < countLabels)     // use "if", can not use "while"
            {
                lstLabels[index++].Visibility = Visibility.Visible;
            }
            else if (index == countLabels)
            {
                Thread.Sleep(500);
                ShowMainWindow?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}
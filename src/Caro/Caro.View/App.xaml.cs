using Caro.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Caro.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SplashWindow splash = new SplashWindow();

            #region SplashViewModel events
            SplashViewModel.ShowMainWindow += delegate
            {
                MainWindow main = new MainWindow();
                splash.Close();
                main.ShowDialog();
            };
            #endregion

            #region MainViewModel events
            MainViewModel.ShowQuestionPlayWindow += delegate
            {
                QuestionPlayWindow questionPlay = new QuestionPlayWindow();
                // When set child window ShowInTaskbar="False", if this one is activating and user click on Taskbar, this one will hide, LOL
                // Hence, must set this child window is owner of all other windows appearing
                foreach (Window w in App.Current.Windows)
                {
                    if (!(w is QuestionPlayWindow))
                    {
                        questionPlay.Owner = w;
                    }
                }
                questionPlay.ShowDialog();
            };

            MainViewModel.ShowGameFinishWindow += delegate
            {
                GameFinishWindow gameFinish = new GameFinishWindow();
                foreach (Window w in App.Current.Windows)
                {
                    if (!(w is GameFinishWindow))
                    {
                        gameFinish.Owner = w;
                    }
                }
                gameFinish.ShowDialog();
            };

            MainViewModel.ShowScoreWindow += delegate
            {
                ScoreWindow score = new ScoreWindow();
                foreach (Window w in App.Current.Windows)
                {
                    if (!(w is ScoreWindow))
                    {
                        score.Owner = w;
                    }
                }
                score.ShowDialog();
            };

            MainViewModel.ShowIntroduceWindow += delegate
            {
                AboutWindow introduce = new AboutWindow();
                foreach (Window w in App.Current.Windows)
                {
                    if (!(w is AboutWindow))
                    {
                        introduce.Owner = w;
                    }
                }
                introduce.ShowDialog();
            };

            MainViewModel.ShowAskExitWindow += delegate
            {
                AskExitWindow askExit = new AskExitWindow();
                foreach (Window w in App.Current.Windows)
                {
                    if (!(w is AskExitWindow))
                    {
                        askExit.Owner = w;
                    }
                }
                askExit.ShowDialog();
            };
            #endregion

            splash.ShowDialog();
        }
    }
}

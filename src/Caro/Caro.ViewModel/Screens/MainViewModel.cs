using Caro.Helper;
using Caro.Model;
using Caro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;    // CancelEventArgs
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Caro.ViewModel
{
    public class MainViewModel
    {
        #region Fields
        Window win;
        Grid grid;
        const int left = 33, top = 33, cellSize = 33, col = 20, row = 20;
        const int right = left + col * cellSize;
        const int bottom = top + row * cellSize;
        const int X = 1, O = -1, empty = 0;
        const int maxDepth = 11, maxMove = 3;
        int[] AScore = new int[] { 0, 2, 18, 162, 1458 };   // mảng điểm tấn công
        int[] DScore = new int[] { 0, 1, 9, 81, 179 };      // mảng điểm chặn
        int[,] board; // main board
		Model.Point[] winMove = new Model.Point[maxDepth + 1];
		Model.Point[] XMove = new Model.Point[maxMove];
        Model.Point[] OMove = new Model.Point[maxMove];
        Model.Point currentSelectedCell;
        int emptyCell = 400;    // var emptyCell does not relate to FindMove method, so in method FindMove when trying not need to
                                          // lessen emptyCell
        int depth;
        EvalBoard evalBoard;
        BitmapImage bitX, bitO;
        bool isX;
        bool isStart;
        bool isEnd;
        bool isFirst;
        bool isWin;
        static bool _isChoiVanMoi;
        Player human = new Player("X");
        Player computer = new Player("O");
        int ComputerWin;    // xác định ván trước ai thắng = 1: computer win,  = 2: humanwin
        int soVandau = 0, nguoiThang = 0, mayThang = 0;
        string pathX = @"..\..\Resources\imgX.png";
        string pathO = @"..\..\Resources\imgO.png";
        #endregion

        #region Properties
        public static bool ChoiVanMoi   // C# ko có biến toàn cục nên phải sử dụng thuộc tính
        {
            get
            {
                return _isChoiVanMoi;
            }
            set
            {
                if (_isChoiVanMoi != value)
                {
                    _isChoiVanMoi = value;
                }
            }
        }
        public static event EventHandler ShowQuestionPlayWindow;
        public static event EventHandler ShowGameFinishWindow;
        public static event EventHandler ShowScoreWindow;
        public static event EventHandler ShowIntroduceWindow;
        public static event EventHandler ShowAskExitWindow;
        public ICommand LoadCommand { get; set; }
        public ICommand MinimizeCommand { get; set; }
        public ICommand ClosingCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand TitleBar_MouseDownCommand { get; set; }
        public ICommand ActivateCommand { get; set; }
        public ICommand StateChangeCommand { get; set; }
        public ICommand MainGrid_LoadCommand { get; set; }
        public ICommand PlayCommand { get; set; }
        public ICommand Window_MouseDownCommand { get; set; }
        public ICommand OpenScoreWindowCommand { get; set; }
        public ICommand OpenIntroduceWindowCommand { get; set; }
        public ICommand OpenAskExitWindowCommand { get; set; }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            isX = true;
            isStart = isEnd = false;
            isFirst = true;     // not play any time
            isWin = false;
            LoadCommand = new RelayCommand<Window>(m => m != null, m => Load(m));
            MinimizeCommand = new RelayCommand<string>(m => m != null, m => Minimize());
            ClosingCommand = new RelayCommand<CancelEventArgs>(m => m != null, m => Closing(m));
            ExitCommand = new RelayCommand<string>(m => m != null, m => Exit());
            TitleBar_MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(m => m != null, m => TitleBar_MouseDown(m));
            StateChangeCommand = new RelayCommand<string>(m => m != null, m => StateChange());
            MainGrid_LoadCommand = new RelayCommand<Grid>(m => m != null, m => MainGrid_Load(m));
            PlayCommand = new RelayCommand<string>(m => m != null, m => Play());
            Window_MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(m => m != null, m => Window_MouseDown(m));
            OpenScoreWindowCommand = new RelayCommand<string>(m => m != null, m => OpenScoreWindow());
            OpenIntroduceWindowCommand = new RelayCommand<string>(m => m != null, m => OpenIntroduceWindow());
        }
        #endregion

        #region Methods
        private void Load(Window w)
        {
            this.win = w;
        }

        private void Minimize()
        {
            // to appear animation when minimizing window
            win.Dispatcher.BeginInvoke(new Action(() => 
            {
                win.WindowStyle = WindowStyle.SingleBorderWindow;
                win.WindowState = WindowState.Minimized;
            }), null);
        }

        // When a window fires event Closed, eventhough anything occurs, it always close window, so in this case we must use
        // event Closing of window. We can not write anything for event Closed of window
        private void Closing(CancelEventArgs e)
        {
            // De Morgan law
            // not (a and b) = not a or not b
            // not (a or b) = not a and not b
            if (!(isFirst || isEnd))
            {
                if (ShowAskExitWindow != null)
                {
                    ShowAskExitWindow(this, EventArgs.Empty);
                    if (!WantToExit.Yes)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        // to appear animation when exit window
                        win.Dispatcher.BeginInvoke(new Action(() => win.WindowStyle = WindowStyle.SingleBorderWindow), null);
                    }
                }
            }
            else
            {
                win.Dispatcher.BeginInvoke(new Action(() => win.WindowStyle = WindowStyle.SingleBorderWindow), null);
            }
        }
        
        private void Exit()
        {
            win.Close();    // before event Closed of a window is fired, that window alway fires event Closing first
        }

        private void TitleBar_MouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                win.DragMove();
            }
        }

        // StateChanged event fires after WindowState changed
        private void StateChange()
        {
            if (win.WindowState != WindowState.Minimized)
            {
                if (win.WindowStyle != WindowStyle.None)
                {
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    win.WindowStyle = WindowStyle.None;
                    //}, DispatcherPriority.Normal);
                    win.Dispatcher.BeginInvoke(new Action(() => win.WindowStyle = WindowStyle.None), null);
                }
            }
        }

        private void MainGrid_Load(Grid grid)
        {
            this.grid = grid;
            board = new int[col, row];
            ResetBoard();
            evalBoard = new EvalBoard();
            VeBanCo();
            Score.NumberOfGames = Score.HumanWin = Score.ComputerWin = 0;
            // Nạp bitX
            bitX = new BitmapImage();
            bitX.BeginInit();
            bitX.UriSource = new Uri(pathX, UriKind.Relative);
            bitX.EndInit();

            // Nạp bitO
            bitO = new BitmapImage();
            bitO.BeginInit();
            bitO.UriSource = new Uri(pathO, UriKind.Relative);
            bitO.EndInit();
        }

        private void ResetBoard()
        {
            for (int i = 0; i < col; i++)
                for (int j = 0; j < row; j++)
                    board[i, j] = empty;

            currentSelectedCell = new Model.Point(-1, -1);
        }

        private void DrawLines()
        {
            // Vẽ đường kẻ ngang, 20 ô thì sẽ có 21 đường kẻ
            for (int i = 0; i < 21; i++)
            {
                Line line = new Line();
                line.X1 = left;
                line.Y1 = (i + 1) * cellSize + 30;   // thêm + 30 để cách thanh tiêu đề
                line.X2 = right;
                line.Y2 = line.Y1;
                line.StrokeThickness = 2;
                line.Stroke = Brushes.Black;
                grid.Children.Add(line);
            }

            // Vẽ đường kẻ dọc
            for (int i = 0; i < 21; i++)
            {
                Line line = new Line();
                line.X1 = (i + 1) * cellSize;
                line.Y1 = top + 30;      // thêm + 30 để cách thanh tiêu đề
                line.X2 = line.X1;
                line.Y2 = bottom + 30;   // thêm + 30 để cách thanh tiêu đề
                line.StrokeThickness = 2;
                line.Stroke = Brushes.Black;
                grid.Children.Add(line);
            }
        }

        private Rectangle CreateCell(Thickness margin)
		{
            Rectangle rec = new Rectangle();
            rec.Width = rec.Height = cellSize;

            // Bắt buộc phải có 3 dòng sau để khỏi dùng Canvas
            rec.HorizontalAlignment = HorizontalAlignment.Left;
            rec.VerticalAlignment = VerticalAlignment.Top;
            rec.Margin = margin;

            return rec;
        }

        // Vẽ lại ô cờ khi vẽ X hoặc O
        private void DrawCell(int x, int y)
        {
            Rectangle cell = CreateCell(new Thickness((x + 1) * cellSize, (y + 1) * cellSize + 30, 0, 0));
            cell.Stroke = Brushes.Black;
            grid.Children.Add(cell);
        }

        private void DrawCurrentSelectedCell()
        {
            if (currentSelectedCell.X == -1)
                return;

            Rectangle cell = CreateCell(new Thickness((currentSelectedCell.X + 1) * cellSize, (currentSelectedCell.Y + 1) * cellSize + 30, 0, 0));
            cell.StrokeThickness = 3.5;
            cell.Stroke = Brushes.DarkSlateBlue;
            cell.StrokeDashArray = DoubleCollection.Parse("1 1");
            grid.Children.Add(cell);
        }

        private void VeBanCo()
        {
            // Vẽ hình chữ nhật bao quanh
            Rectangle rec = CreateCell(new Thickness(left, top + 30, 0, 0));    //thêm + 30 để cách thanh tiêu đề
            //Canvas.SetLeft(rec, left);
            //Canvas.SetTop(rec, top + 30);    // thêm + 30 để cách thanh tiêu đề
            rec.Width = col * cellSize;
            rec.Height = row * cellSize;
            rec.Fill = Brushes.GreenYellow;
            grid.Children.Add(rec);

            DrawLines();
        }

        private void Draw_X(int x, int y)
        {
            Image imgX = new Image();
            imgX.Source = bitX;
            imgX.HorizontalAlignment = HorizontalAlignment.Left;
            imgX.VerticalAlignment = VerticalAlignment.Top;
            imgX.Width = imgX.Height = cellSize;
            imgX.Margin = new Thickness((x + 1) * cellSize, (y + 1) * cellSize + 30, 0, 0);
            grid.Children.Add(imgX);
            DrawCell(x, y);   // vì mỗi bàn cờ bao quanh bởi đường kẻ đen nên phải vẽ lại các đgkẻ
        }

        private void Draw_O(int x, int y)
        {
            Image imgO = new Image();
            imgO.Source = bitO;
            imgO.HorizontalAlignment = HorizontalAlignment.Left;
            imgO.VerticalAlignment = VerticalAlignment.Top;
            imgO.Width = imgO.Height = cellSize;
            imgO.Margin = new Thickness((x + 1) * cellSize, (y + 1) * cellSize + 30, 0, 0);
            grid.Children.Add(imgO);
            DrawCell(x, y);
        }

        // Vẽ đường kết thúc xác định 5 quân ăn
        private void DrawLineWin(string str, int colWin, int rowWin, int colWin2, int rowWin2)
        {
            Line line = new Line();

            if (str == "row")
            {
                line.X1 = (colWin + 1) * cellSize;
                line.Y1 = (rowWin + 1) * cellSize + 30 + cellSize / 2;
                line.X2 = (colWin2 + 1) * cellSize + cellSize;
                line.Y2 = (rowWin2 + 1) * cellSize + 30 + cellSize / 2;
            }
            else if (str == "col")
            {
                line.X1 = (colWin + 1) * cellSize + cellSize / 2;
                line.Y1 = (rowWin + 1) * cellSize + 30;
                line.X2 = (colWin2 + 1) * cellSize + cellSize / 2;
                line.Y2 = (rowWin2 + 1) * cellSize + 30 + cellSize;
            }
            else if (str == "cheo xuong")
            {
                line.X1 = (colWin + 1) * cellSize;
                line.Y1 = (rowWin + 1) * cellSize + 30;
                line.X2 = (colWin2 + 1) * cellSize + cellSize;
                line.Y2 = (rowWin2 + 1) * cellSize + 30 + cellSize;
            }
            else if (str == "cheo len")
            {
                line.X1 = (colWin + 1) * cellSize;
                line.Y1 = (rowWin + 1) * cellSize + 30 + cellSize;
                line.X2 = (colWin2 + 1) * cellSize + cellSize;
                line.Y2 = (rowWin2 + 1) * cellSize + 30;
            }
            line.Stroke = Brushes.Yellow;
            line.StrokeThickness = 10;
            grid.Children.Add(line);
        }

        // Kiểm tra kết thúc, các giá trị str, colWin, rowWin, colWin2, rowWin2 để xác định
        // vẽ line chiến thắng. Áp dụng luật chặn kín hai đầu, hai đầu phải bị chặn sát
        private GameState CheckEnd(int cl, int rw, ref string str, ref int colWin, ref int rowWin, ref int colWin2, ref int rowWin2)
        {
            int r = 0, c = 0;
            int score;

            // Check hàng ngang, dựa vào tọa độ hàng rw
            while (c < col - 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[c + i, rw];
                if (score == 5)
                {
                    if (c == 0 || (c + 4 == col - 1) || (c > 0 && c + 4 < col - 1 &&
                                    (board[c - 1, rw] != O || board[c + 5, rw] != O)))
                    {
                        str = "row";
                        rowWin = rowWin2 = rw;
                        colWin = c;
                        colWin2 = c + 4;
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (c == 0 || (c + 4 == col - 1) || (c > 0 && c + 4 < col - 1 &&
                                        (board[c - 1, rw] != X || board[c + 5, rw] != X)))
                    {
                        str = "row";
                        rowWin = rowWin2 = rw;
                        colWin = c;
                        colWin2 = c + 4;
                        return GameState.OWin;
                    }
                }
                c++;
            }

            // Check hàng dọc, dựa vào tọa độ cột cl
            while (r < row - 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[cl, r + i];
                if (score == 5)
                {
                    if (r == 0 || (r + 4 == row - 1) || (r > 0 && r + 4 < row - 1 &&
                                            (board[cl, r - 1] != O || board[cl, r + 5] != O)))
                    {
                        str = "col";
                        colWin = colWin2 = cl;
                        rowWin = r;
                        rowWin2 = r + 4;
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (r == 0 || (r + 4 == row - 1) || (r > 0 && r + 4 < row - 1 &&
                                            (board[cl, r - 1] != X || board[cl, r + 5] != X)))
                    {
                        str = "col";
                        colWin = colWin2 = cl;
                        rowWin = r;
                        rowWin2 = r + 4;
                        return GameState.OWin;
                    }
                }
                r++;
            }

            // Check đường chéo xuống (đường chéo dấu huyền)
            r = rw;
            c = cl;
            while (r > 0 && c > 0)
            {
                r--;
                c--;
            }
            while (r < row - 4 && c < col - 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[c + i, r + i];
                if (score == 5)
                {
                    if (c == 0 || r == 0 || (c + 4 == col - 1) || (r + 4 == row - 1) ||
                                (c > 0 && r > 0 && c + 4 < col - 1 && r + 4 < row - 1 &&
                                (board[c - 1, r - 1] != O || board[c + 5, r + 5] != O)))
                    {
                        str = "cheo xuong";
                        colWin = c;
                        colWin2 = c + 4;
                        rowWin = r;
                        rowWin2 = r + 4;
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (c == 0 || r == 0 || (c + 4 == col - 1) || (r + 4 == row - 1) ||
                                (c > 0 && r > 0 && c + 4 < col - 1 && r + 4 < row - 1 &&
                                (board[c - 1, r - 1] != X || board[c + 5, r + 5] != X)))
                    {
                        str = "cheo xuong";
                        colWin = c;
                        colWin2 = c + 4;
                        rowWin = r;
                        rowWin2 = r + 4;
                        return GameState.OWin;
                    }
                }
                r++;
                c++;
            }

            // Check đường chéo lên (đường chéo dấu sắc)
            r = rw;
            c = cl;
            while (c > 0 && r < row - 1)
            {
                c--;
                r++;
            }
            while (c < col - 4 && r >= 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[c + i, r - i];
                if (score == 5)
                {
                    if (c == 0 || r == row - 1 || (c + 4 == col - 1) || (r - 4 == 0) ||
                            (c > 0 && r < row - 1 && c + 4 < col - 1 && r - 4 > 0 &&
                            (board[c - 1, r + 1] != O || board[c + 5, r - 5] != O)))
                    {
                        str = "cheo len";
                        colWin = c;
                        rowWin = r;
                        colWin2 = c + 4;
                        rowWin2 = r - 4;
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (c == 0 || r == row - 1 || (c + 4 == col - 1) || (r - 4 == 0) ||
                            (c > 0 && r < row - 1 && c + 4 < col - 1 && r - 4 > 0 &&
                            (board[c - 1, r + 1] != X || board[c + 5, r - 5] != X)))
                    {
                        str = "cheo len";
                        colWin = c;
                        rowWin = r;
                        colWin2 = c + 4;
                        rowWin2 = r - 4;
                        return GameState.OWin;
                    }
                }
                c++;
                r--;
            }

            return GameState.Draw;
        }

        // Hàm CheckEnd_2 dùng trong việc tìm nước đi, ko cần xác định tọa độ
        // để vẽ LineWin
        private GameState CheckEnd_2(int cl, int rw)
        {
            int r = 0, c = 0;
            int score;

            // Check hàng ngang, dựa vào tọa độ hàng rw
            while (c < col - 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[c + i, rw];
                if (score == 5)
                {
                    if (c == 0 || (c + 4 == col - 1) || (c > 0 && c + 4 < col - 1 &&
                                        (board[c - 1, rw] != O || board[c + 5, rw] != O)))
                    {
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (c == 0 || (c + 4 == col - 1) || (c > 0 && c + 4 < col - 1 &&
                                        (board[c - 1, rw] != X || board[c + 5, rw] != X)))
                    {
                        return GameState.OWin;
                    }
                }
                c++;
            }

            // Check hàng dọc, dựa vào tọa độ cột cl
            while (r < row - 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[cl, r + i];
                if (score == 5)
                {
                    if (r == 0 || (r + 4 == row - 1) || (r > 0 && r + 4 < row - 1 &&
                                            (board[cl, r - 1] != O || board[cl, r + 5] != O)))
                    {
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (r == 0 || (r + 4 == row - 1) || (r > 0 && r + 4 < row - 1 &&
                                            (board[cl, r - 1] != X || board[cl, r + 5] != X)))
                    {
                        return GameState.OWin;
                    }
                }
                r++;
            }

            // Check đường chéo xuống (đường chéo dấu huyền)
            r = rw;
            c = cl;
            while (r > 0 && c > 0)
            {
                r--;
                c--;
            }
            while (r < row - 4 && c < col - 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[c + i, r + i];
                if (score == 5)
                {
                    if (c == 0 || r == 0 || (c + 4 == col - 1) || (r + 4 == row - 1) ||
                                (c > 0 && r > 0 && c + 4 < col - 1 && r + 4 < row - 1 &&
                                (board[c - 1, r - 1] != O || board[c + 5, r + 5] != O)))
                    {
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (c == 0 || r == 0 || (c + 4 == col - 1) || (r + 4 == row - 1) ||
                                (c > 0 && r > 0 && c + 4 < col - 1 && r + 4 < row - 1 &&
                                (board[c - 1, r - 1] != X || board[c + 5, r + 5] != X)))
                    {
                        return GameState.OWin;
                    }
                }
                r++;
                c++;
            }

            // Check đường chéo lên (đường chéo dấu sắc)
            r = rw;
            c = cl;
            while (c > 0 && r < row - 1)
            {
                c--;
                r++;
            }
            while (c < col - 4 && r >= 4)
            {
                score = 0;
                for (int i = 0; i < 5; i++)
                    score += board[c + i, r - i];
                if (score == 5)
                {
                    if (c == 0 || r == row - 1 || (c + 4 == col - 1) || (r - 4 == 0) ||
                            (c > 0 && r < row - 1 && c + 4 < col - 1 && r - 4 > 0 &&
                            (board[c - 1, r + 1] != O || board[c + 5, r - 5] != O)))
                    {
                        return GameState.XWin;
                    }
                }
                else if (score == -5)
                {
                    if (c == 0 || r == row - 1 || (c + 4 == col - 1) || (r - 4 == 0) ||
                            (c > 0 && r < row - 1 && c + 4 < col - 1 && r - 4 > 0 &&
                            (board[c - 1, r + 1] != X || board[c + 5, r - 5] != X)))
                    {
                        return GameState.OWin;
                    }
                }
                c++;
                r--;
            }

            return GameState.Draw;
        }

        // Lượng giá bàn cờ
        // Tiến hành quét một block 5 ô theo 4 hướng là dọc, ngang, chéo xuống, chéo lên,
        // đếm số quân cờ mỗi bên và tiến hành cộng điểm cho các ô trống khi trong block
        // 5 ô đó chỉ có toàn quân ta, hoặc toàn quân địch.
        // Nếu có n quân ta thì các ô trống được cộng Ascore[n] điểm.
        // Nếu có n quân địch thì các ô trống được cộng Dscore[n] điểm.
        // Như thế điểm các ô trống sẽ được cộng dồn (do một ô được quét 5 lần mỗi chiều).
        // Điều này có ý nghĩa trong việc xác định các nước đi ăn đôi, ăn ba.
        private void Eval(Player player, EvalBoard evalBoard)
        {
            int eX, eO;
            int r, c;

            evalBoard.ResetBoard();
            // Scan rows
            for (r = 0; r < row; r++)
                for (c = 0; c < col - 4; c++)
                {
                    eX = eO = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (board[c + i, r] == X)
                            eX++;
                        else if (board[c + i, r] == O)
                            eO++;
                    }
                    if (eX * eO == 0 && eX != eO)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (board[c + i, r] == empty)    // cộng điểm cho ô trống
                            {
                                if (eX == 0)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                            c + 4 < col - 1 && (board[c - 1, r] != X ||
                                            board[c + 5, r] != X)))
                                        {
                                            evalBoard.Board[c + i, r] += DScore[eO];
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                            c + 4 < col - 1 && (board[c - 1, r] != X ||
                                            board[c + 5, r] != X)))
                                        {
                                            evalBoard.Board[c + i, r] += AScore[eO];
                                        }
                                    }
                                }
                                else    // eO == 0
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                            c + 4 < col - 1 && (board[c - 1, r] != O ||
                                            board[c + 5, r] != O)))
                                        {
                                            evalBoard.Board[c + i, r] += AScore[eX];
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                            c + 4 < col - 1 && (board[c - 1, r] != O ||
                                            board[c + 5, r] != O)))
                                        {
                                            evalBoard.Board[c + i, r] += DScore[eX];
                                        }
                                    }
                                }

                                if (eX == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                                c + 4 < col - 1 && (board[c - 1, r] != O ||
                                                board[c + 5, r] != O)))
                                        {
                                            evalBoard.Board[c + i, r] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                            c + 4 < col - 1 && (board[c - 1, r] != O ||
                                            board[c + 5, r] != O)))
                                        {
                                            evalBoard.Board[c + i, r] *= 2;
                                        }
                                    }
                                }
                                else if (eO == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                            c + 4 < col - 1 && (board[c - 1, r] != X ||
                                            board[c + 5, r] != X)))
                                        {
                                            evalBoard.Board[c + i, r] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || c + 4 == col - 1 || (c > 0 &&
                                                c + 4 < col - 1 && (board[c - 1, r] != X ||
                                                board[c + 5, r] != X)))
                                        {
                                            evalBoard.Board[c + i, r] *= 2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            // Scan columns
            for (c = 0; c < col; c++)
                for (r = 0; r < row - 4; r++)
                {
                    eX = eO = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (board[c, r + i] == X)
                            eX++;
                        else if (board[c, r + i] == O)
                            eO++;
                    }
                    if (eX * eO == 0 && eX != eO)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (board[c, r + i] == empty)
                            {
                                if (eX == 0)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 && r + 4 < row - 1
                                                && (board[c, r - 1] != X || board[c, r + 5] != X)))
                                        {
                                            evalBoard.Board[c, r + i] += DScore[eO];
                                        }
                                    }
                                    else
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 && r + 4 < row - 1
                                                && (board[c, r - 1] != X || board[c, r + 5] != X)))
                                        {
                                            evalBoard.Board[c, r + i] += AScore[eO];
                                        }
                                    }
                                }
                                else
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 && r + 4 < row - 1
                                                && (board[c, r - 1] != O || board[c, r + 5] != O)))
                                        {
                                            evalBoard.Board[c, r + i] += AScore[eX];
                                        }
                                    }
                                    else
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 && r + 4 < row - 1
                                                && (board[c, r - 1] != O || board[c, r + 5] != O)))
                                        {
                                            evalBoard.Board[c, r + i] += DScore[eX];
                                        }
                                    }
                                }

                                if (eX == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 &&
                                                r + 4 < row - 1 && (board[c, r - 1] != O ||
                                                board[c, r + 5] != O)))
                                        {
                                            evalBoard.Board[c, r + i] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 &&
                                            r + 4 < row - 1 && (board[c, r - 1] != O ||
                                            board[c, r + 5] != O)))
                                        {
                                            evalBoard.Board[c, r + i] *= 2;
                                        }
                                    }
                                }
                                else if (eO == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 &&
                                            r + 4 < row - 1 && (board[c, r - 1] != X ||
                                            board[c, r + 5] != X)))
                                        {
                                            evalBoard.Board[c, r + i] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (r == 0 || r + 4 == row - 1 || (r > 0 &&
                                                r + 4 < row - 1 && (board[c, r - 1] != X ||
                                                board[c, r + 5] != X)))
                                        {
                                            evalBoard.Board[c, r + i] *= 2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            // Scan forward diagonals
            for (c = 0; c < col - 4; c++)
                for (r = 0; r < row - 4; r++)
                {
                    eX = eO = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (board[c + i, r + i] == X)
                            eX++;
                        else if (board[c + i, r + i] == O)
                            eO++;
                    }
                    if (eX * eO == 0 && eX != eO)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (board[c + i, r + i] == empty)
                            {
                                if (eX == 0)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                                r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                                r > 0 && r + 4 < row - 1 &&
                                                (board[c - 1, r - 1] != X ||
                                                board[c + 5, r + 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r + i] += DScore[eO];
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                                r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                                r > 0 && r + 4 < row - 1 &&
                                                (board[c - 1, r - 1] != X ||
                                                board[c + 5, r + 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r + i] += AScore[eO];
                                        }
                                    }
                                }
                                else
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                                r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                                r > 0 && r + 4 < row - 1 &&
                                                (board[c - 1, r - 1] != O ||
                                                board[c + 5, r + 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r + i] += AScore[eX];
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                                r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                                r > 0 && r + 4 < row - 1 &&
                                                (board[c - 1, r - 1] != O ||
                                                board[c + 5, r + 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r + i] += DScore[eX];
                                        }
                                    }
                                }

                                if (eX == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                            r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                                r > 0 && r + 4 < row - 1 &&
                                                (board[c - 1, r - 1] != O ||
                                                board[c + 5, r + 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r + i] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                            r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                            r > 0 && r + 4 < row - 1 &&
                                            (board[c - 1, r - 1] != O ||
                                            board[c + 5, r + 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r + i] *= 2;
                                        }
                                    }
                                }
                                else if (eO == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                            r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                            r > 0 && r + 4 < row - 1 &&
                                            (board[c - 1, r - 1] != X ||
                                            board[c + 5, r + 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r + i] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == 0 || c + 4 == col - 1 ||
                                            r + 4 == row - 1 || (c > 0 && c + 4 < col - 1 &&
                                            r > 0 && r + 4 < row - 1 &&
                                            (board[c - 1, r - 1] != X ||
                                            board[c + 5, r + 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r + i] *= 2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            // Scan backward diagonals
            for (c = 0; c < col - 4; c++)
                for (r = 4; r < row; r++)
                {
                    eX = eO = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (board[c + i, r - i] == X)
                            eX++;
                        else if (board[c + i, r - i] == O)
                            eO++;
                    }
                    if (eX * eO == 0 && eX != eO)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (board[c + i, r - i] == empty)
                            {
                                if (eX == 0)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                            r < row - 1 && r - 4 > 0 && (board[c - 1, r + 1] != X
                                            || board[c + 5, r - 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r - i] += DScore[eO];
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                            r < row - 1 && r - 4 > 0 && (board[c - 1, r + 1] != X
                                            || board[c + 5, r - 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r - i] += AScore[eO];
                                        }
                                    }
                                }
                                else
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                            r < row - 1 && r - 4 > 0 && (board[c - 1, r + 1] != O
                                            || board[c + 5, r - 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r - i] += AScore[eX];
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                            r < row - 1 && r - 4 > 0 && (board[c - 1, r + 1] != O
                                            || board[c + 5, r - 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r - i] += DScore[eX];
                                        }
                                    }
                                }

                                if (eX == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                               r < row - 1 && r - 4 > 0 &&
                                                (board[c - 1, r + 1] != O ||
                                                board[c + 5, r - 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r - i] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                            r < row - 1 && r - 4 > 0 &&
                                            (board[c - 1, r + 1] != O ||
                                            board[c + 5, r - 5] != O)))
                                        {
                                            evalBoard.Board[c + i, r - i] *= 2;
                                        }
                                    }
                                }
                                else if (eO == 4)
                                {
                                    if (player.Symbol == "X")
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                            r < row - 1 && r - 4 > 0 &&
                                            (board[c - 1, r + 1] != X ||
                                            board[c + 5, r - 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r - i] *= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (c == 0 || r == row - 1 || c + 4 == col - 1 ||
                                            r - 4 == 0 || (c > 0 && c + 4 < col - 1 &&
                                            r < row - 1 && r - 4 > 0 &&
                                            (board[c - 1, r + 1] != X ||
                                            board[c + 5, r - 5] != X)))
                                        {
                                            evalBoard.Board[c + i, r - i] *= 2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
        }

        // Tìm nước đi cho máy tính
        // Đầu tiên đánh giá bàn cờ và chọn ra 3 ô trống có điểm cao nhất, tiến hành đánh thử.
        // Trong mỗi lượt đánh thử cũng tiến hành đánh giá bàn cờ và chọn ra 3 ô trống
        // cao điểm nhất của người để đánh trả lại. Độ sâu tối đa là 11. Nếu tìm thấy
        // nước đi dẫn tới chiến thắng thì đánh theo nước đó. Không thì đánh vào ô có
        // điểm cao nhất.
        private void FindMove(Player player)
        {
            if (depth > maxDepth)
                return;
            depth++;
            isWin = false;
            Model.Point xx = new Model.Point();
            Model.Point oo = new Model.Point();
            int count = 0;

            Eval(player, evalBoard);
            // Chọn ra 3 ô trống có điểm cao nhất của máy tính
            for (int i = 0; i < maxMove; i++)
            {
                if (player.Symbol == "X")
                {
                    xx = evalBoard.MaxPos();
                    evalBoard.Board[xx.X, xx.Y] = 0;
                    XMove[i] = xx;
                }
                else
                {
                    oo = evalBoard.MaxPos();
                    evalBoard.Board[oo.X, oo.Y] = 0;
                    OMove[i] = oo;
                }
            }

            // Lấy từng nước đi trong các nước đi của máy tính ra đánh thử
            if (player.Symbol == "X")
            {
                count = 0;
                while (count < maxMove)
                {
                    xx = XMove[count++];
                    board[xx.X, xx.Y] = X;
                    winMove.SetValue(xx, depth - 1);

                    // Tìm 3 ô trống có điểm cao nhất(nước đi tối ưu của đối thủ)
                    Player player1 = new Player("O");
                    Eval(player1, evalBoard);
                    for (int i = 0; i < maxMove; i++)
                    {
                        oo = evalBoard.MaxPos();
                        OMove[i] = oo;
                        evalBoard.Board[oo.X, oo.Y] = 0;
                    }

                    // Đánh thử các nước đi của đối thủ
                    for (int i = 0; i < maxMove; i++)
                    {
                        oo = OMove[i];
                        board[oo.X, oo.Y] = O;
                        if (CheckEnd_2(oo.X, oo.Y) == GameState.XWin)
                        {
                            isWin = true;
                            board[oo.X, oo.Y] = board[xx.X, xx.Y] = 0;
                            return;     // return luôn vì đây là nước đi dẫn đến chiến thắng
                        }
                        else if (CheckEnd_2(oo.X, oo.Y) == GameState.OWin)
                        {
                            board[oo.X, oo.Y] = board[xx.X, xx.Y] = 0;
                            break;  // bỏ nước đi thua này để tìm nước đi khác
                        }
                        FindMove(player);
                        board[oo.X, oo.Y] = 0;
                    }
                    board[xx.X, xx.Y] = 0;
                }
            }
            else
            {
                count = 0;
                while (count < maxMove)
                {
                    oo = OMove[count++];
                    board[oo.X, oo.Y] = O;
                    winMove.SetValue(oo, depth - 1);

                    // Tìm 3 ô trống có điểm cao nhất(nước đi tối ưu của đối thủ)
                    Player player1 = new Player("X");
                    Eval(player1, evalBoard);
                    for (int i = 0; i < maxMove; i++)
                    {
                        xx = evalBoard.MaxPos();
                        XMove[i] = xx;
                        evalBoard.Board[xx.X, xx.Y] = 0;
                    }

                    // Đánh thử các nước đi của đối thủ
                    for (int i = 0; i < maxMove; i++)
                    {
                        xx = XMove[i];
                        board[xx.X, xx.Y] = X;
                        if (CheckEnd_2(xx.X, xx.Y) == GameState.OWin)
                        {
                            isWin = true;
                            board[xx.X, xx.Y] = board[oo.X, oo.Y] = 0;
                            return; // return luôn vì đây là nước đi dẫn đến chiến thắng
                        }
                        else if (CheckEnd_2(xx.X, xx.Y) == GameState.XWin)
                        {
                            board[xx.X, xx.Y] = board[oo.X, oo.Y] = 0;
                            break;  // bỏ nước đi thua này để tìm nước đi khác
                        }
                        FindMove(player);
                        board[xx.X, xx.Y] = 0;
                    }
                    board[oo.X, oo.Y] = 0;
                }
            }
        }

        private void Minimax(Player player)
        {
            for (int i = 0; i < maxMove; i++)
            {
                XMove[i] = new Model.Point();
                OMove[i] = new Model.Point();
            }
            for (int i = 0; i < maxDepth + 1; i++)
                winMove[i] = new Model.Point();
            depth = 0;
            FindMove(player);
        }

        private void Play()
        {
            if (isFirst)     // let's get started for the first game
            {
                isFirst = false;
                isStart = true;
                ComputerWin = 2;    // nếu người chơi muốn chơi mới khi chưa kết thúc ván thì ở ván tiếp theo sẽ dựa vào để xác định
                                                // người đi trước. Ván đầu tiên máy sẽ chơi trước, chọn ô ngẫu nhiên ở khoảng giữa bàn cờ để đánh
                Random randCls = new Random(Guid.NewGuid().GetHashCode());
                int i = randCls.Next(7, 13);
                Thread.Sleep(250);
                int j = randCls.Next(7, 13);
                currentSelectedCell = new Model.Point(i, j);
                Draw_O(i, j);
                DrawCurrentSelectedCell();
                board[i, j] = O;
                isX = true;
                emptyCell--;
            }
            else
            {
                if (isEnd == true)       // the current game ended
                {
                    isStart = true;
                    isEnd = false;
                    ResetBoard();
                    evalBoard = new EvalBoard();
                    VeBanCo();
                    emptyCell = 400;
                    if (ComputerWin == 2)    // computer goes first
                    {
                        Random randCls = new Random();
                        int i = randCls.Next(7, 13);
                        Thread.Sleep(250);
                        int j = randCls.Next(7, 13);
                        currentSelectedCell = new Model.Point(i, j);
                        Draw_O(i, j);
                        DrawCurrentSelectedCell();
                        board[i, j] = O;
                        isX = true;
                        emptyCell--;
                    }
                }
                else    // the current game not ending
                {
                    if (emptyCell < 400)        // at least non-empty cell in the board
                    {
                        if (ShowQuestionPlayWindow != null)
                        {
                            ShowQuestionPlayWindow(this, EventArgs.Empty);
                        }
                        if (ChoiVanMoi)
                        {
                            isStart = true;
                            isEnd = false;
                            ResetBoard();
                            evalBoard = new EvalBoard();
                            VeBanCo();
                            emptyCell = 400;
                            if (ComputerWin == 2)    // computer goes first
                            {
                                Random randCls = new Random();
                                int i = randCls.Next(7, 13);
                                Thread.Sleep(250);
                                int j = randCls.Next(7, 13);
                                currentSelectedCell = new Model.Point(i, j);
                                Draw_O(i, j);
                                DrawCurrentSelectedCell();
                                board[i, j] = O;
                                isX = true;
                                emptyCell--;
                            }
                        }
                    }
                }
            }
        }

        // Apply for mainGrid
        private void Window_MouseDown(MouseButtonEventArgs e)
        {
            if (isStart && !isEnd && e.LeftButton == MouseButtonState.Pressed &&
                e.GetPosition(win).X > left && e.GetPosition(win).X < right &&
                e.GetPosition(win).Y > top + 30 && e.GetPosition(win).Y < bottom + 30)
            {
                string str = " ";
                int colWin, rowWin, colWin2, rowWin2;
                colWin = rowWin = colWin2 = rowWin2 = -1;
                GameState state;

                int x = (int)(e.GetPosition(win).X / cellSize) - 1;
                int y = (int)((e.GetPosition(win).Y - 30) / cellSize) - 1;
                if (board[x, y] == empty)
                {
                    if (isX)
                    {
                        //Clear border of previous selected cell
                        if (currentSelectedCell.X != -1 && board[currentSelectedCell.X, currentSelectedCell.Y] == X)
                            Draw_X(currentSelectedCell.X, currentSelectedCell.Y);
                        else if (currentSelectedCell.X != -1 && board[currentSelectedCell.X, currentSelectedCell.Y] == O)
                            Draw_O(currentSelectedCell.X, currentSelectedCell.Y);

                        currentSelectedCell = new Model.Point(x, y);
                        Draw_X(x, y);
                        DrawCurrentSelectedCell();
                        board[x, y] = X;
                        emptyCell--;
                        isX = !isX;

                        state = CheckEnd(x, y, ref str, ref colWin, ref rowWin, ref colWin2, ref rowWin2);
                        if (state == GameState.XWin)
                        {
                            DrawLineWin(str, colWin, rowWin, colWin2, rowWin2);
                            Thread.Sleep(120);
                            GameFinishStatus.Status = GameState.XWin;
                            if (ShowGameFinishWindow != null)
                            {
                                ShowGameFinishWindow(this, EventArgs.Empty);
                            }
                            isStart = false;
                            isEnd = true;
                            ComputerWin = 2;    // human win, ván sau computer đi trước
                            Score.NumberOfGames = ++soVandau;
                            Score.HumanWin = ++nguoiThang;
                            return;
                        }
                        else if (state == GameState.Draw && emptyCell == 0)
                        {
                            Thread.Sleep(120);
                            GameFinishStatus.Status = GameState.Draw;
                            if (ShowGameFinishWindow != null)
                            {
                                ShowGameFinishWindow(this, EventArgs.Empty);
                            }
                            isStart = false;
                            isEnd = true;
                            Score.NumberOfGames = ++soVandau;
                            return;
                        }

                        Minimax(computer);
                        if (isWin)
                        {
                            x = winMove[0].X;
                            y = winMove[0].Y;
                        }
                        else
                        {
                            Eval(computer, evalBoard);
							Model.Point p = evalBoard.MaxPos();
                            x = p.X;
                            y = p.Y;
                        }

                        //Clear border of previous selected cell
                        if (currentSelectedCell.X != -1 && board[currentSelectedCell.X, currentSelectedCell.Y] == X)
                            Draw_X(currentSelectedCell.X, currentSelectedCell.Y);
                        else if (currentSelectedCell.X != -1 && board[currentSelectedCell.X, currentSelectedCell.Y] == O)
                            Draw_O(currentSelectedCell.X, currentSelectedCell.Y);

                        currentSelectedCell = new Model.Point(x, y);
                        Draw_O(x, y);
                        DrawCurrentSelectedCell();
                        board[x, y] = O;
                        emptyCell--;
                        isX = !isX;

                        state = CheckEnd(x, y, ref str, ref colWin, ref rowWin, ref colWin2, ref rowWin2);
                        if (state == GameState.OWin)
                        {
                            DrawLineWin(str, colWin, rowWin, colWin2, rowWin2);
                            Thread.Sleep(120);
                            GameFinishStatus.Status = GameState.OWin;
                            if (ShowGameFinishWindow != null)
                            {
                                ShowGameFinishWindow(this, EventArgs.Empty);
                            }
                            isStart = false;
                            isEnd = true;
                            ComputerWin = 1;    // computer win, human will go first in next game
                            Score.NumberOfGames = ++soVandau;
                            Score.ComputerWin = ++mayThang;
                            return;
                        }
                        else if (state == GameState.Draw && emptyCell == 0)
                        {
                            Thread.Sleep(120);
                            GameFinishStatus.Status = GameState.Draw;
                            if (ShowGameFinishWindow != null)
                            {
                                ShowGameFinishWindow(this, EventArgs.Empty);
                            }
                            isStart = false;
                            isEnd = true;
                            Score.NumberOfGames = ++soVandau;
                            return;
                        }
                    }
                }
            }
        }

        private void OpenScoreWindow()
        {
            ShowScoreWindow?.Invoke(this, EventArgs.Empty);
        }

        private void OpenIntroduceWindow()
        {
            ShowIntroduceWindow?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
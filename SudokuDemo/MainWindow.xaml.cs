using SudokuModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SudokuDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region static fields

        private static GameLevel[] gameLevels = new GameLevel[4] { new GameLevel("Easy", "5 : 00", 50), new GameLevel("Medium", "8 : 00", 38), new GameLevel("Hard", "12 : 00", 36), new GameLevel("Expert", "15 : 00", 22) };

        private static Board myBoard = new Board(9);

        public static TextBox[,] txtGrid = new TextBox[myBoard.Size, myBoard.Size];
        private static int size = 450;
        private static Button[] btn = new Button[3];
        private static TextBlock calTime = new TextBlock();
        private static string currentLevel;
        private DispatcherTimer timer = new DispatcherTimer();

        #endregion static fields

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CreateDynamicBorder();
            timer.Interval += TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            stopwatch.Children.Add(calTime);
            CalTimeSetup();
        }

        /// <summary>
        /// populateGrid helper
        /// </summary>
        private void populateGrid()
        {
            sudoku.Children.Clear();
            sudoku.Width = size;
            sudoku.Height = sudoku.Width;

            //create textbox and place them into panel
            double textBoxSize = sudoku.Width / myBoard.Size;
            // make the panel a perfact

            //nested loops. create text and print them to the screen
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    txtGrid[i, j] = new TextBox();
                    txtGrid[i, j].Height = textBoxSize;
                    txtGrid[i, j].Width = textBoxSize;
                    txtGrid[i, j].Style = (Style)Resources["txtTemplate"];
                    txtGrid[i, j].TextChanged += OnlyDigit;
                }
            }
        }

        private void OnlyDigit(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (System.Text.RegularExpressions.Regex.IsMatch(textBox.Text, "[^0-9]"))
            {
                textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
                
            }
            else if (textBox.Text.Length > 1)
            {
                textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
            }
            else if(textBox.Text != "")
            {
                textBox.Background = Brushes.LightPink;
            }
            else
            {
                textBox.Background = Brushes.White;
            }
        }

        /// <summary>
        /// New Game button + event
        /// </summary>

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            clearBoard();
            CreateDynamicBorder();
            timer.Tag = "";
            calTime.Text = "0 : 00";
            DisplaySudoku();
            DisplayRestart("0 : 00");
        }

        /// <summary>
        /// Generate button + event
        /// </summary>

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            clearBoard();
            features.Children.Clear();
            DisplayFeatures();
            DisplayInstructor("Choose the level you want to play");
            features.Width = size * 2 / 3;
            features.Height = size * 2 / 3;
            int shuffleLevel = 20;
            Init(myBoard.Sudoku);

            Update(myBoard.Sudoku, shuffleLevel);
            Button[] levelButtons = new Button[4];

            for (int i = 0; i < levelButtons.Length; i++)
            {
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Images/" + gameLevels[i].Level + ".png"));
                levelButtons[i] = CreateButton(features.Width - 20, features.Height / 4 - 20, image, 25, "PinkRoundCorner", 10, Cursors.Hand, Create);
                levelButtons[i].Tag = gameLevels[i].Level;
                features.Children.Add(levelButtons[i]);
            }
        }

        /// <summary>
        /// check button + event
        /// </summary>

        private void checkButton_click(object sender, RoutedEventArgs e)
        {
            if (sudoku.Visibility == Visibility.Hidden)
            {
                MessageBox.Show("You have to play to use this feature!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string[,] chWin = new string[myBoard.Size, myBoard.Size];
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    chWin[i, j] = txtGrid[i, j].Text;
                }
            }
            int count = checkWin(chWin);
            if (count == 0)
            {
                WinDisplay();
            }
            else if (count == 1)
            {
                MessageBox.Show($"You have 1 wrong box. Almost there!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (count == 81)
            {
                if (CheckNull(txtGrid))
                {
                    MessageBox.Show($"Sudoku is empty!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                } else
                {
                    MessageBox.Show($"Invalid input!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            }
            else
            {
                if(MarkWrongBoxes(myBoard.Sudoku, txtGrid) > 0)
                {
                    MessageBox.Show($"You have {MarkWrongBoxes(myBoard.Sudoku, txtGrid)} wrong boxes");
                }
                else
                {
                    if(count == 1)
                    {
                        MessageBox.Show($"You have {count} unfill box. Try harder!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"You have {count} unfill boxes. Try harder!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                   
                }
                
            }
        }

        /// <summary>
        /// Solve button + event
        /// </summary>

        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sudoku.Visibility == Visibility.Hidden)
            {
                MessageBox.Show("You have to play to use this feature!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            timer.Stop();
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    if (txtGrid[i, j].Text != "")
                    {
                        myBoard.Sudoku[i, j] = txtGrid[i, j].Text;
                    }
                    else myBoard.Sudoku[i, j] = "";
                }
            }
            if (CheckNull(txtGrid))
            {
                MessageBox.Show("Sudoku is empty", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                SudokuValidator(myBoard.Sudoku);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                timer.Start();
                return;
            }
            SolveSudoku(myBoard.Sudoku);
            DisplaySudoku(myBoard.Sudoku);
        }

        /// <summary>
        /// load buttons container + event
        /// </summary>

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            features.Children.Clear();
            DisplayFeatures();

            StreamReader[] txts = new StreamReader[btn.Length];
            features.Width = size / 1.5;
            features.Height = size / 2;
            DisplayInstructor("Load game");
            for (int i = 0; i < btn.Length; i++)
            {
                string loadPlace = "Save/Save " + i;
                txts[i] = new StreamReader(loadPlace + ".txt");
                string btnName = txts[i].ReadLine();
                btn[i] = CreateButton(features.Width - 20, features.Height / btn.Length - 20, btnName == "" ? loadPlace : btnName, 25, "PinkRoundCorner", 10, Cursors.Hand, loadButtons);
                btn[i].Tag = loadPlace;
                btn[i].Click -= SaveButtons;
                features.Children.Add(btn[i]);
                txts[i].Close();
            }
        }

        /// <summary>
        /// load buttons
        /// </summary>

        private void loadButtons(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string loadFile = clickedButton.Tag + ".txt";
            StreamReader txt = new StreamReader(loadFile);

            txt.ReadLine();
            currentLevel = txt.ReadLine();
            calTime.Text = txt.ReadLine();
            DisplayRestart("0 : 00");
            CreateDynamicBorder();
            LoadFromFile(txt);
            txt.Close();
            DisplaySudoku();
        }

        /// <summary>
        /// Load from file
        /// </summary>

        private void LoadFromFile(StreamReader txt)
        {
            for (int i = 0; i < myBoard.Size; i++)
            {
                try
                {
                    string[] temp = new string[myBoard.Size];
                    string str = txt.ReadLine();
                    if (str != null)
                    {
                        temp = str.Split(" ");
                    }

                    for (int j = 0; j < myBoard.Size; j++)
                    {
                        if (temp[j] == "0")
                        {
                            myBoard.Sudoku[i, j] = "";
                        }
                        else
                        {
                            myBoard.Sudoku[i, j] = temp[j];
                        }
                    }
                    temp = null;
                }
                catch (Exception)
                {
                    MessageBox.Show("This box is empty!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }
            }

            timer.Tag = currentLevel;
            timer.Start();

            DisplaySudoku(myBoard.Sudoku);
        }

        /// <summary>
        /// saveButtons Container + Event
        /// </summary>

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sudoku.Visibility == Visibility.Hidden)
            {
                MessageBox.Show("You have to play to use this feature!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            timer.Stop();
            features.Children.Clear();
            DisplayInstructor("Save game");
            StreamReader[] txts = new StreamReader[btn.Length];

            features.Width = size / 1.5;
            features.Height = size / 2;
            for (int i = 0; i < btn.Length; i++)
            {
                string savePlace = "Save/Save " + i;
                txts[i] = new StreamReader(savePlace + ".txt");
                btn[i] = new Button();
                btn[i].Height = features.Height / btn.Length - 20;
                btn[i].Width = features.Width - 20;

                string btnName = txts[i].ReadLine();
                if (btnName == "")
                {
                    btn[i].Content = savePlace;
                }
                else
                {
                    btn[i].Content = btnName;
                }

                btn[i].Tag = savePlace;
                btn[i].FontSize = 25;
                btn[i].Click += SaveButtons;
                btn[i].Style = (Style)Resources["PinkRoundCorner"];
                btn[i].Margin = new Thickness(10);
                features.Children.Add(btn[i]);
                txts[i].Close();
            }
            DisplayFeatures();
        }

        /// <summary>
        /// Save buttons
        /// </summary>

        private void SaveButtons(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            string saveFile = clickedButton.Tag + ".txt";
            StreamWriter txt = new StreamWriter(saveFile);
            clickedButton.Content = DateTime.Now.ToString("g");
            txt.WriteLine(clickedButton.Content);
            txt.WriteLine(currentLevel);

            txt.WriteLine(calTime.Text);

            SaveToFile(txt);

            MessageBox.Show("Saved successfully!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            txt.Close();
        }

        /// <summary>
        /// Save to file
        /// </summary>

        private static void SaveToFile(StreamWriter txt)
        {
            string temp = "";

            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    if (txtGrid[i, j].Text == "")
                    {
                        temp += "0 ";
                    }
                    else
                    {
                        temp += txtGrid[i, j].Text + " ";
                    }
                }
                temp = temp.TrimEnd();
                txt.WriteLine(temp);
                temp = "";
            }
        }

        /// <summary>
        /// quitButton
        /// </summary>

        private void quitButton_Click(object sender, RoutedEventArgs e)
        {
            if (sudoku.Visibility == Visibility.Visible)
            {
                var result = MessageBox.Show("Save your game?", "Application Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    saveButton_Click(sender, e);
                }
                else if (result == MessageBoxResult.No)
                {
                    Close();
                }
            }
            else
            {
                var result = MessageBox.Show("Are you sure want to quit?", "Application Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Close();
                }
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (features.Visibility == Visibility.Visible || (string)timer.Tag == "")
            {
                cancelButton.Visibility = Visibility.Hidden;
                features.Children.Clear();
                displayImage("image.png");
                if (features.Children.Count == 1)
                {
                    HiddenInstructor();
                }
            }
            if (sudoku.Visibility == Visibility.Visible)
            {
                sudoku.Visibility = Visibility.Hidden;
                restart.Visibility = Visibility.Hidden;
                stopwatch.Visibility = Visibility.Hidden;
                features.Visibility = Visibility.Visible;
                if (features.Children.Count > 1)
                {
                    borderInstructor.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Display sudoku to monitor
        /// </summary>

        public static void DisplaySudoku(string[,] board)
        {
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    if (myBoard.Sudoku[i, j] != "")
                    {
                        txtGrid[i, j].Text = myBoard.Sudoku[i, j];
                        txtGrid[i, j].IsReadOnly = true;
                    }
                    else
                    {
                        txtGrid[i, j].Text = myBoard.Sudoku[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Solve sudoku
        /// </summary>

        public static void SolveSudoku(string[,] board)
        {
            if (board == null || board.Length == 0)
                return;

            Solve(board);
        }

        /// <summary>
        /// Solve
        /// </summary>

        private static bool Solve(string[,] board)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == "")
                    {
                        for (int c = 1; c <= 9; c++)
                        {
                            if (isValid(board, i, j, c))
                            {
                                board[i, j] = c.ToString();

                                if (Solve(board))
                                    return true;
                                else
                                    board[i, j] = "";
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }



        private static int MarkWrongBoxes(string[,] board, TextBox[,] txtGrid)
        {
            int wrongBoxs =0;
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    if(board[i, j] != txtGrid[i,j].Text && !isValid(board, i, j, int.Parse(txtGrid[i, j].Text)))
                    {
                        txtGrid[i, j].Background = Brushes.Red;
                        wrongBoxs++;
                    }
                }
            }
            return wrongBoxs;
        }
        /// <summary>
        /// Constraints
        /// </summary>

        private static bool isValid(string[,] board, int row, int col, int c)
        {
            for (int i = 0; i < 9; i++)
            {
                //check row
                if (board[i, col] != "" && board[i, col] == c.ToString())
                    return false;
                //check column
                if (board[row, i] != "" && board[row, i] == c.ToString())
                    return false;
                //check 3*3 block
                if (board[3 * (row / 3) + i / 3, 3 * (col / 3) + i % 3] != "" && board[3 * (row / 3) + i / 3, 3 * (col / 3) + i % 3] == c.ToString())
                    return false;
            }
            return true;
        }

        /// <summary>
        ///  create a permanent sudoku
        /// </summary>

        private static void Init(string[,] board)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    board[i, j] = ((i * 3 + i / 3 + j) % 9 + 1).ToString();
                }
            }
        }

        /// <summary>
        /// change two cell
        /// </summary>

        private static void ChangeTwoCell(string[,] board, string findValue1, string findValue2)
        {
            int xParm1, yParm1, xParm2, yParm2;
            xParm1 = yParm1 = xParm2 = yParm2 = 0;
            for (int i = 0; i < 9; i += 3)
            {
                for (int k = 0; k < 9; k += 3)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            if (board[i + j, k + z] == findValue1)
                            {
                                xParm1 = i + j;
                                yParm1 = k + z;
                            }
                            if (board[i + j, k + z] == findValue2)
                            {
                                xParm2 = i + j;
                                yParm2 = k + z;
                            }
                        }
                    }
                    board[xParm1, yParm1] = findValue2;
                    board[xParm2, yParm2] = findValue1;
                }
            }
        }

        /// <summary>
        /// shuffle matrix
        /// </summary>

        private static void Update(string[,] board, int shuffleLevel)
        {
            for (int repeat = 0; repeat < shuffleLevel; repeat++)
            {
                Random rand = new Random(Guid.NewGuid().GetHashCode());
                Random rand2 = new Random(Guid.NewGuid().GetHashCode());
                ChangeTwoCell(board, rand.Next(1, 10).ToString(), rand2.Next(1, 10).ToString());
            }
        }

        /// <summary>
        ///  Check Win helper
        /// </summary>

        private static int checkWin(string[,] board)
        {
            int count = 0;
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    if (board[i, j] != "")
                    {
                        int tmp = int.Parse(board[i, j]);
                        board[i, j] = "";
                        if (!isValid(board, i, j, int.Parse(txtGrid[i,j].Text))&&txtGrid[i, j].IsReadOnly == false)
                            count++;
                        board[i, j] = tmp.ToString();
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Set all value of a matrix to ""
        /// </summary>

        private static void Clear(string[,] board)
        {
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    board[i, j] = "";
                }
            }
        }

        /// <summary>
        /// Generate a new sudoku
        /// </summary>

        private void Create(object sender, RoutedEventArgs e)
        {
            CreateDynamicBorder();
            DisplaySudoku();
            Button clickedButton = (Button)sender;
            currentLevel = clickedButton.Tag.ToString();
            myBoard.Sudoku = CreatedSudoku(myBoard.Sudoku, currentLevel);
            DisplaySudoku(myBoard.Sudoku);
            DisplayRestart("0 : 00");
            calTime.Text = "0 : 00";
            timer.Tag = currentLevel;
            timer.Start();
        }

        /// <summary>
        /// Create a stop match
        /// </summary>

        private void timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            string temp = calTime.Text;
            if (temp == "")
            {
                timer.Stop();
                return;
            }
            string[] arrayTimer = temp.Split(":", StringSplitOptions.TrimEntries);
            int seconds = int.Parse(arrayTimer[1]) + 1;
            if (seconds == 60)
            {
                arrayTimer[1] = "00";
                arrayTimer[0] = (int.Parse(arrayTimer[0]) + 1).ToString();
            }
            else if (seconds < 10)
            {
                arrayTimer[1] = "0" + seconds.ToString();
            }
            else
            {
                arrayTimer[1] = seconds.ToString();
            }
            calTime.Text = arrayTimer[0] + " : " + arrayTimer[1];
            foreach (var item in gameLevels)
            {
                if (timer.Tag.ToString() == item.Level)
                {
                    if (calTime.Text.ToString() == item.TimeLimit)
                    {
                        timer.Stop();
                        LostDisplay();
                    }
                }
            }
            if (timer.Tag.ToString() == "")
            {
                timer.Stop();
                calTime.Text = "0 : 00";
            }
        }

        /// <summary>
        /// Create a new sudoku board
        /// </summary>

        private static string[,] CreatedSudoku(string[,] board, string level)
        {
            string[,] result = new string[myBoard.Size, myBoard.Size];
            Clear(result);
            Random random = new Random();
            int lv = Array.IndexOf(gameLevels, level);
            int revealBox = 0;
            while (revealBox < gameLevels[lv].RevealBox)
            {
                int Random1 = random.Next(0, myBoard.Size);
                int Random2 = random.Next(0, myBoard.Size);
                if (result[Random1, Random2] == "")
                {
                    result[Random1, Random2] = board[Random1, Random2];
                    revealBox++;
                }
            }
            return result;
        }

        /// <summary>
        /// Create a Dynamic Border
        /// </summary>
        private void CreateDynamicBorder()
        {
            populateGrid();
            timer.Stop();
            borderInstructor.Visibility = Visibility.Hidden;
            Border[,] border = new Border[3, 3];

            WrapPanel[,] wrapPanel = new WrapPanel[3, 3];

            for (int i = 0; i < myBoard.Size; i += 3)
            {
                for (int j = 0; j < myBoard.Size; j += 3)
                {
                    border[i / 3, j / 3] = new Border();
                    border[i / 3, j / 3].Background = new SolidColorBrush(Colors.White);
                    border[i / 3, j / 3].BorderThickness = new Thickness(2);
                    border[i / 3, j / 3].CornerRadius = new CornerRadius(2);
                    border[i / 3, j / 3].BorderBrush = new SolidColorBrush(Colors.DeepPink);
                    border[i / 3, j / 3].Width = size / 3;
                    border[i / 3, j / 3].Height = size / 3;
                    wrapPanel[i / 3, j / 3] = new WrapPanel();
                    wrapPanel[i / 3, j / 3].Width = border[i / 3, j / 3].Width;
                    wrapPanel[i / 3, j / 3].Height = border[i / 3, j / 3].Height;

                    border[i / 3, j / 3].Child = wrapPanel[i / 3, j / 3];
                    sudoku.Children.Add(border[i / 3, j / 3]);
                    for (int a = i; a < i + 3; a++)
                    {
                        for (int b = j; b < j + 3; b++)
                        {
                            wrapPanel[i / 3, j / 3].Children.Add(txtGrid[a, b]);
                        }
                    }
                }
            }
        }

        private void LostDisplay()
        {
            features.Children.Clear();
            DisplayFeatures();
            displayImage("lost.png");
        }

        private void WinDisplay()
        {
            features.Children.Clear();
            displayImage("win.jpg");
            DisplayFeatures();
            try
            {
                GameLevel currentLevel = gameLevels[Array.IndexOf(gameLevels, timer.Tag)];
                MessageBox.Show($"You played: {timer.Tag} mode\nYour score: {currentLevel.Point(Seconds(calTime.Text))}", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                return;
            }
        }

        private static bool CheckNull(TextBox[,] board)
        {
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    if (board[i, j].Text != "")
                        return false;
                }
            }
            return true;
        }

        private void DisplaySudoku()
        {
            cancelButton.Visibility = Visibility.Visible;
            if (sudoku.Visibility == Visibility.Hidden)
            {
                sudoku.Visibility = Visibility.Visible;
                restart.Visibility = Visibility.Visible;
                stopwatch.Visibility = Visibility.Visible;
                features.Visibility = Visibility.Hidden;
            }
        }

        private void DisplayFeatures()
        {
            cancelButton.Visibility = Visibility.Visible;
            if (sudoku.Visibility == Visibility.Visible)
            {
                sudoku.Visibility = Visibility.Hidden;
                restart.Visibility = Visibility.Hidden;
                stopwatch.Visibility = Visibility.Hidden;
                features.Visibility = Visibility.Visible;
            }
        }

        private void clearBoard()
        {
            timer.Stop();
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    txtGrid[i, j].Text = "";
                    myBoard.Sudoku[i, j] = "";
                }
            }
        }

        private void displayImage(string imageName)
        {
            features.Children.Clear();
            features.Width = size;
            features.Height = size;
            Image image = new Image();
            image.Width = size;
            image.Height = size;
            image.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/images/" + imageName));
            features.Children.Add(image);
        }

        private void DisplayInstructor(string message)
        {
            borderInstructor.Visibility = Visibility.Visible;
            txtInstructor.Content = message;
        }

        private void HiddenInstructor()
        {
            borderInstructor.Visibility = Visibility.Hidden;
        }

        private void DisplayRestart(string time)
        {
            restart.Visibility = Visibility.Visible;
            Button restartButton = new Button();
            restartButton.Style = (Style)Resources["YellowRoundCorner"];
            restartButton.Width = restart.Width - 25;
            restartButton.Height = restart.Height;
            restartButton.Cursor = Cursors.Hand;
            restartButton.Content = "Restart";

            restartButton.Click += (s, e) =>
            {
                var result = MessageBox.Show("Are you sure?", "Restart", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DisplaySudoku(myBoard.Sudoku);
                    calTime.Text = time;
                }
            };

            restart.Children.Add(restartButton);
        }

        private static void SudokuValidator(string[,] board)
        {
            for (int i = 0; i < myBoard.Size; i++)
            {
                for (int j = 0; j < myBoard.Size; j++)
                {
                    if (board[i, j] != "")
                    {
                        if (!int.TryParse(board[i, j], out int n) || n < 1 || n > 9) throw new FormatException();
                        int tmp = int.Parse(board[i, j]);
                        board[i, j] = "";
                        if (!isValid(board, i, j, tmp))
                            throw new Exception();
                        board[i, j] = tmp.ToString();
                    }
                }
            }
        }

        private Button CreateButton(double width, double height, object contentElement, int fontSize, string resources, int thickness, Cursor cursor, RoutedEventHandler routedEventHandler)
        {
            Button button = new Button();
            button.Width = width;
            button.Height = height;
            button.Content = contentElement;
            button.FontSize = fontSize;
            button.Style = (Style)Resources[resources];
            button.Margin = new Thickness(thickness);
            button.Cursor = cursor;
            button.Click += routedEventHandler;
            return button;
        }

        private void CalTimeSetup()
        {
            calTime.FontWeight = FontWeights.Bold;
            calTime.FontSize = 25;
            calTime.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private int Seconds(string time)
        {
            string[] arrayTimer = time.Split(":", StringSplitOptions.TrimEntries);
            int seconds = int.Parse(arrayTimer[0]) * 60 + int.Parse(arrayTimer[1]);
            return seconds;
        }

        
    }
}
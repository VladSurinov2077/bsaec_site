using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using WpfAnimatedGif;
using static System.Data.Entity.Infrastructure.Design.Executor;
using static AdminPanel.MainWindow;
namespace AdminPanel
{
    public partial class MainWindow : Window
    {
        bool Full_Window = false;
        bool IsDelete = false, IsUpdate = false, IsError = false, IsChangeData = false, IsSaveSchedule = false, IsChangeSchedule = false;
        byte[] _imageData;
        private string connectingString = App.ConnectionString;
        private List<string> DayWeekList = new List<string> { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };

        // для записи 
        TempSchedule tempSchedule;

        int NumberList = 1;
        public MainWindow()
        {
            InitializeComponent();
            ComboBoxSelectDay.ItemsSource = DayWeekList;
            ComboBoxSelectDay.SelectedIndex = 0;
            this.MouseMove += MainWindow_MouseMove;
            this.MouseDown += MainWindow_MouseDown;
            this.MouseUp += MainWindow_MouseUp;
            this.StateChanged += OnWindowStateChanged;
            this.SizeChanged += SizeChangeds;
            ResizePlace();
            TeacherButton.IsEnabled = false;
            ShowListTeacher();
        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ ЭЛЕМЕНТОВ

        private bool isDragging = false;
        private Point clickPosition;
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                !(e.OriginalSource is TextBox) &&
                !(e.OriginalSource is PasswordBox) &&
                !(e.OriginalSource is ScrollBar) &&
                !(e.OriginalSource is ScrollViewer))
            {
                isDragging = true;
                clickPosition = e.GetPosition(this);
                Mouse.Capture(this);
            }
        }
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                Vector offset = currentPosition - clickPosition;
                this.Left += offset.X;
                this.Top += offset.Y;
            }
        }
        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                Mouse.Capture(null);
            }
        }
        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                BitmapImage bitmap = new BitmapImage(new Uri("/Image/fullyes.png", UriKind.RelativeOrAbsolute));
                Full_Window = true;
                ImgFull.Source = bitmap;
            }
            ResizePlace();
        }
        private void SizeChangeds(object sender, EventArgs e)
        {
            ResizePlace();
        }
        private void ResizePlace()
        {
            PullMenu.Width = Width / 3.5;
            ScrollMenu.Height = Height / 1.3;
            BlockEdit.Width = Width - PullMenu.Width;
            EditBorder.Height = Height / 1.3;
            EditBorder.Width = BlockEdit.Width - 50;

            TeacherWindow.Height = EditBorder.Height;
            TeacherWindow.Width = EditBorder.Width - 10;
            TeacherScroll.Height = EditBorder.Height / 1.5;

            JobWindow.Height = EditBorder.Height;
            JobWindow.Width = EditBorder.Width - 10;
            JobScroll.Height = JobScroll.Height / 1.5;

            SpecWindow.Height = EditBorder.Height;
            SpecWindow.Width = EditBorder.Width - 10;
            SpecScroll.Height = JobScroll.Height / 1.5;

            AdminTypeWindow.Height = EditBorder.Height;
            AdminTypeWindow.Width = EditBorder.Width - 10;
            AdminTypeScroll.Height = JobScroll.Height / 1.5;

            ScoreWindow.Height = EditBorder.Height;
            ScoreWindow.Width = EditBorder.Width - 10;
            ScoreScroll.Height = JobScroll.Height / 1.5;

            SubjectWindow.Height = EditBorder.Height;
            SubjectWindow.Width = EditBorder.Width - 10;
            SubjectScroll.Height = JobScroll.Height / 1.5;

            GroupWindow.Height = EditBorder.Height;
            GroupWindow.Width = EditBorder.Width - 10;
            GroupScroll.Height = JobScroll.Height / 1.5;

            ScheduleWindow.Height = EditBorder.Height;
            ScheduleWindow.Width = EditBorder.Width - 10;
            ComboBoxSelectDay.Width = EditBorder.Width - 350;
            ScheduleScroll.Height = JobScroll.Height / 1.5;

            PermScheduleWindow.Height = EditBorder.Height;
            PermScheduleWindow.Width = EditBorder.Width - 10;
            PermScheduleScroll.Height = JobScroll.Height / 1.5;

            MyCanvas.Width = this.ActualWidth;
            MyCanvas.Height = this.ActualHeight;
            Canvas.SetLeft(DialogBox, (Width - DialogBox.Width) / 2);
            Canvas.SetTop(DialogBox, (Height - DialogBox.Height) / 2);
        }
        private void FullWindow(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap1 = new BitmapImage(new Uri("/Image/fullyes.png", UriKind.RelativeOrAbsolute));
            BitmapImage bitmap2 = new BitmapImage(new Uri("/Image/fullno.png", UriKind.RelativeOrAbsolute));
            if (Full_Window == false)
            {
                Full_Window = true;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;
                ImgFull.Source = bitmap1;
            }
            else
            {
                Full_Window = false;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowState = WindowState.Normal;
                this.Width = 800;
                this.Height = 450;
                ImgFull.Source = bitmap2;
            }
            UpDateList(sender, e);
        }
        private void CollapseWindow(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void ButtonYes(object sender, EventArgs e)
        {

            if (IsDelete)
            {
                Button button = sender as Button;
                int button_id = Convert.ToInt32(button.Tag);
                if (NumberList == 2)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"DELETE FROM Specialties WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@id", button_id);
                        command.ExecuteNonQuery();
                        IsDelete = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        UpDateList(sender, e);
                        connection.Close();
                    }
                }
                else if (NumberList == 3)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"DELETE FROM AdminType WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@id", button_id);
                        command.ExecuteNonQuery();
                        IsDelete = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        UpDateList(sender, e);
                        connection.Close();
                    }
                }
                else if (NumberList == 4)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"DELETE FROM AdminJob WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@id", button_id);
                        command.ExecuteNonQuery();
                        IsDelete = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        UpDateList(sender, e);
                        connection.Close();
                    }
                }
                else if (NumberList == 6)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"DELETE FROM Subject WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@id", button_id);
                        command.ExecuteNonQuery();
                        IsDelete = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        UpDateList(sender, e);
                        connection.Close();
                    }
                }
                else if (NumberList == 7)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"DELETE FROM GroupList WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@id", button_id);
                        command.ExecuteNonQuery();
                        command = new SQLiteCommand($"DELETE FROM SchedulePermanent WHERE GroupID = @id", connection);
                        command.Parameters.AddWithValue("@id", button_id);
                        command.ExecuteNonQuery();
                        IsDelete = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        UpDateList(sender, e);
                        connection.Close();
                    }
                }
                else if (NumberList == 8)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"DELETE FROM ScheduleGroup WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@id", button_id);
                        command.ExecuteNonQuery();
                        IsDelete = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        UpDateList(sender, e);
                        connection.Close();
                    }
                }
            }
            else
            {
                if (IsChangeData)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"UPDATE PassingScores SET Years = {Convert.ToInt32(ScoreEdit.Text)} WHERE Number = 1", connection);
                        command.ExecuteNonQuery();
                        IsChangeData = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        connection.Close();
                    }
                }
                if(IsSaveSchedule)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        string json = JsonConvert.SerializeObject(tempSchedule);
                        SQLiteCommand command = new SQLiteCommand($"UPDATE ScheduleTemporary SET List = @list WHERE Number = 1", connection);
                        command.Parameters.AddWithValue("@list", json);
                        command.ExecuteNonQuery();
                        IsSaveSchedule = false;
                        IsChangeSchedule = false;
                        DialogBox.Visibility = Visibility.Collapsed;
                        MyCanvas.Visibility = Visibility.Collapsed;
                        connection.Close();
                    }
                }
                if(IsChangeSchedule)
                {
                    IsChangeSchedule = false;
                    DialogBox.Visibility = Visibility.Collapsed;
                    MyCanvas.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void ButtonNo(object sender, EventArgs e)
        {
            if (IsDelete)
            {
                IsDelete = false;
                DialogBox.Visibility = Visibility.Collapsed;
            }
            if (IsError)
            {
                IsError = false;
                YesButton.Visibility = Visibility.Visible;
                DialogBox.Visibility = Visibility.Collapsed;
            }
            if (IsChangeData)
            {
                IsChangeData = false;
                DialogBox.Visibility = Visibility.Collapsed;
            }
            MyCanvas.Visibility = Visibility.Collapsed;
        }

        // ФУНКЦИОНАЛ МЕНЮ

        private void ClickButtonMenu(object sender, EventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            toggleButton.IsChecked = false;
            int button_id = Convert.ToInt32(toggleButton.Tag);
            if (!IsUpdate)
            {
                if (!IsChangeSchedule)
                {
                    if (NumberList == 5 || NumberList == 8 || NumberList == 9) UpDateButton.Visibility = Visibility.Visible;
                    TeacherButton.IsEnabled = true; JobButton.IsEnabled = true; SpecButton.IsEnabled = true;
                    AdminTypeButton.IsEnabled = true; ScoreButton.IsEnabled = true; SubjectButton.IsEnabled = true; GroupButton.IsEnabled = true;
                    ScheduleButton.IsEnabled = true; SchedulePermButton.IsEnabled = true;
                    TeacherWindow.Visibility = Visibility.Collapsed;
                    JobWindow.Visibility = Visibility.Collapsed;
                    SpecWindow.Visibility = Visibility.Collapsed;
                    AdminTypeWindow.Visibility = Visibility.Collapsed;
                    ScoreWindow.Visibility = Visibility.Collapsed;
                    SubjectWindow.Visibility = Visibility.Collapsed;
                    GroupWindow.Visibility = Visibility.Collapsed;
                    ScheduleWindow.Visibility = Visibility.Collapsed;
                    PermScheduleWindow.Visibility = Visibility.Collapsed;
                    if (button_id == 1)
                    {
                        TeacherButton.IsEnabled = false;
                        NumberList = 1;
                    }
                    else if (button_id == 2)
                    {
                        SpecButton.IsEnabled = false;
                        NumberList = 2;
                    }
                    else if (button_id == 3)
                    {
                        AdminTypeButton.IsEnabled = false;
                        NumberList = 3;
                    }
                    else if (button_id == 4)
                    {
                        JobButton.IsEnabled = false;
                        NumberList = 4;
                    }
                    else if (button_id == 5)
                    {
                        ScoreButton.IsEnabled = false;
                        NumberList = 5;
                    }
                    else if (button_id == 6)
                    {
                        SubjectButton.IsEnabled = false;
                        NumberList = 6;
                    }
                    else if (button_id == 7)
                    {
                        GroupButton.IsEnabled = false;
                        NumberList = 7;
                    }
                    else if (button_id == 8)
                    {
                        ScheduleButton.IsEnabled = false;
                        NumberList = 8;
                    }
                    else if (button_id == 9)
                    {
                        SchedulePermButton.IsEnabled = false;
                        NumberList = 9;
                    }
                    UpDateList(sender, e);
                } else
                {
                    MyCanvas.Visibility = Visibility.Visible;
                    DialogBox.Visibility = Visibility.Visible;
                    CanvasMain.Content = "Отменить";
                    CanvasDescription.Text = "Вы действительно хотите отменить изменение?";
                }
            }
        }

        // ФУНКЦИОНАЛ КНОПКИ ПЕРЕЗАГРУЗКИ

        private async void UpDateList(object sender, EventArgs e)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/Image/loading.gif", UriKind.Absolute);
            image.EndInit();
            IsUpdate = true;
            Image loading = new Image
            {
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100,
                Height = 100
            };
            ImageBehavior.SetAnimatedSource(loading, image);
            if (NumberList == 1)
            {
                TeacherWindow.Visibility = Visibility.Visible;
                TeacherList.Children.Clear();
                TeacherScroll.ScrollToVerticalOffset(0);
                TeacherList.HorizontalAlignment = HorizontalAlignment.Center;
                TeacherList.Children.Add(loading);
                await Task.Delay(800);
                TeacherList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListTeacher();
            }
            else if (NumberList == 2)
            {
                SpecWindow.Visibility = Visibility.Visible;
                SpecList.Children.Clear();
                SpecScroll.ScrollToVerticalOffset(0);
                SpecList.HorizontalAlignment = HorizontalAlignment.Center;
                SpecList.Children.Add(loading);
                await Task.Delay(800);
                SpecList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListSpec();
            }
            else if (NumberList == 3)
            {
                AdminTypeWindow.Visibility = Visibility.Visible;
                AdminTypeList.Children.Clear();
                AdminTypeScroll.ScrollToVerticalOffset(0);
                AdminTypeList.HorizontalAlignment = HorizontalAlignment.Center;
                AdminTypeList.Children.Add(loading);
                await Task.Delay(800);
                AdminTypeList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListAdminType();
            }
            else if (NumberList == 4)
            {
                JobWindow.Visibility = Visibility.Visible;
                JobList.Children.Clear();
                JobScroll.ScrollToVerticalOffset(0);
                JobList.HorizontalAlignment = HorizontalAlignment.Center;
                JobList.Children.Add(loading);
                await Task.Delay(800);
                JobList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListJob();
            }
            else if (NumberList == 5)
            {
                UpDateButton.Visibility = Visibility.Collapsed;
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand("SELECT Years FROM PassingScores WHERE Number = 1", connection);
                    using (var reader = command.ExecuteReader()) if (reader.Read()) ScoreEdit.Text = $"{reader.GetInt32(0)}";
                    connection.Close();
                }
                ScoreWindow.Visibility = Visibility.Visible;
                ScoreList.Children.Clear();
                ScoreScroll.ScrollToVerticalOffset(0);
                ScoreList.HorizontalAlignment = HorizontalAlignment.Center;
                ScoreList.Children.Add(loading);
                await Task.Delay(800);
                ScoreList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListScore();
            }
            else if (NumberList == 6)
            {
                SubjectWindow.Visibility = Visibility.Visible;
                SubjectList.Children.Clear();
                SubjectScroll.ScrollToVerticalOffset(0);
                SubjectList.HorizontalAlignment = HorizontalAlignment.Center;
                SubjectList.Children.Add(loading);
                await Task.Delay(800);
                SubjectList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListSubject();
            }
            else if (NumberList == 7)
            {
                GroupWindow.Visibility = Visibility.Visible;
                GroupList.Children.Clear();
                GroupScroll.ScrollToVerticalOffset(0);
                GroupList.HorizontalAlignment = HorizontalAlignment.Center;
                GroupList.Children.Add(loading);
                await Task.Delay(800);
                GroupList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListGroup();
            }
            else if (NumberList == 8)
            {
                UpDateButton.Visibility = Visibility.Collapsed;
                ScheduleWindow.Visibility = Visibility.Visible;
                ScheduleList.Children.Clear();
                ScheduleScroll.ScrollToVerticalOffset(0);
                ScheduleList.HorizontalAlignment = HorizontalAlignment.Center;
                ScheduleList.Children.Add(loading);
                await Task.Delay(800);
                ScheduleList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListSchedule();
            }
            else if (NumberList == 9)
            {
                UpDateButton.Visibility = Visibility.Collapsed;
                PermScheduleWindow.Visibility = Visibility.Visible;
                PermScheduleList.Children.Clear();
                PermScheduleScroll.ScrollToVerticalOffset(0);
                PermScheduleList.HorizontalAlignment = HorizontalAlignment.Center;
                PermScheduleList.Children.Add(loading);
                await Task.Delay(800);
                PermScheduleList.HorizontalAlignment = HorizontalAlignment.Left;
                ShowListPermSchedule();
            }
            IsUpdate = false;
        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "ПРЕПОДОВАТЕЛИ"
        private void ShowListTeacher()
        {
            TeacherWindow.Visibility = Visibility.Visible;
            TeacherList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Teacher", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            _imageData = (byte[])reader[6];
                            Button button = new Button
                            {
                                Margin = new Thickness(5, 0, 0, 5),
                                Width = 159,
                                Style = this.TryFindResource("HoverButtonStyle") as Style
                            };
                            StackPanel panel = new StackPanel
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Width = button.Width,
                                Height = button.Height
                            };
                            Image image = new Image
                            {
                                Source = ConvertByteArrayToImage(_imageData),
                                Stretch = Stretch.Fill,
                                Width = 100,
                                Height = 100

                            };
                            Label text1 = new Label
                            {
                                Content = $"{reader.GetString(1)}",
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                Padding = new Thickness(0),
                                FontSize = 18
                            };
                            Label text2 = new Label
                            {
                                Content = $"{reader.GetString(2)}",
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                Padding = new Thickness(0),
                                FontSize = 16
                            };
                            Label text3 = new Label
                            {
                                Content = $"{reader.GetString(3)}",
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                Padding = new Thickness(0),
                                FontSize = 16
                            };
                            panel.Children.Add(image);
                            panel.Children.Add(text1);
                            panel.Children.Add(text2);
                            panel.Children.Add(text3);
                            button.Content = panel;
                            button.Tag = $"{reader.GetInt32(0)}";
                            button.Click += ButtonEditTeacher;
                            TeacherList.Children.Add(button);
                        }
                    }
                    else
                    {
                        TeacherList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        TeacherList.Children.Add(not_list);
                    }
                }
                connection.Close();
            }
        }
        private void ButtonEditTeacher(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            AddEditTeacher addEditTeacher = new AddEditTeacher(1, id);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditTeacher.Owner = this;
                    addEditTeacher.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditTeacher.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonAddTeacher(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            AddEditTeacher addEditTeacher = new AddEditTeacher(0, 0);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditTeacher.Owner = this;
                    addEditTeacher.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditTeacher.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "ДОЛЖНОСТИ"

        private void ShowListJob()
        {
            JobWindow.Visibility = Visibility.Visible;
            JobList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM AdminJob", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditJob;
                            Button button_delete = new Button
                            {
                                Margin = new Thickness(10, 8, 0, 8),
                                VerticalAlignment = VerticalAlignment.Top,
                                Style = this.TryFindResource("ButtonClotting") as Style,
                                Height = 30,
                                Width = 30
                            };
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            Image image_delete = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Image/trash.png")),
                                Width = 30
                            }; button_delete.Content = image_delete;
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            button_delete.Click += ButtonDeleteJob;
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 10),
                                Width = border.Width - 200
                            };
                            panel_button.Children.Add(button_edit);
                            panel_button.Children.Add(button_delete);
                            grid.Children.Add(text);
                            Grid.SetColumn(text, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            JobList.Children.Add(border);

                        }
                    }
                    else
                    {
                        JobList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        JobList.Children.Add(not_list);
                    }
                }
                connection.Close();
            }
        }
        private void ButtonAddJob(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            AddEditJob addEditJob = new AddEditJob(0, 0);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditJob.Owner = this;
                    addEditJob.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditJob.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonEditJob(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            AddEditJob addEditJob = new AddEditJob(1, button_id);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditJob.Owner = this;
                    addEditJob.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditJob.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonDeleteJob(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            DialogBox.Visibility = Visibility.Visible;
            CanvasMain.Content = "Удаление";
            CanvasDescription.Text = "Вы действительно хотите удалить должность?";
            IsDelete = true;
            YesButton.Tag = $"{button_id}";
        }

        // // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "СОСТАВ"
        private void ShowListAdminType()
        {
            AdminTypeWindow.Visibility = Visibility.Visible;
            AdminTypeList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM AdminType", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditAdminType;
                            Button button_delete = new Button
                            {
                                Margin = new Thickness(10, 8, 0, 8),
                                VerticalAlignment = VerticalAlignment.Top,
                                Style = this.TryFindResource("ButtonClotting") as Style,
                                Height = 30,
                                Width = 30
                            };
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            Image image_delete = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Image/trash.png")),
                                Width = 30
                            }; button_delete.Content = image_delete;
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            button_delete.Click += ButtonDeleteAdminType;
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 10),
                                Width = border.Width - 200
                            };
                            panel_button.Children.Add(button_edit);
                            panel_button.Children.Add(button_delete);
                            grid.Children.Add(text);
                            Grid.SetColumn(text, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            AdminTypeList.Children.Add(border);

                        }
                    }
                    else
                    {
                        AdminTypeList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        AdminTypeList.Children.Add(not_list);
                    }
                }
                connection.Close();
            }
        }
        private void ButtonAddAdminType(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            AddEditAdminType addEditAdminType = new AddEditAdminType(0, 0);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditAdminType.Owner = this;
                    addEditAdminType.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditAdminType.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonEditAdminType(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            AddEditAdminType addEditAdminType = new AddEditAdminType(1, button_id);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditAdminType.Owner = this;
                    addEditAdminType.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditAdminType.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonDeleteAdminType(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            DialogBox.Visibility = Visibility.Visible;
            CanvasMain.Content = "Удаление";
            CanvasDescription.Text = "Вы действительно хотите удалить состав?";
            IsDelete = true;
            YesButton.Tag = $"{button_id}";
        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "СПЕЦИАЛЬНОСТИ"

        private void ShowListSpec()
        {
            SpecWindow.Visibility = Visibility.Visible;
            SpecList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Specialties", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditSpec;
                            Button button_delete = new Button
                            {
                                Margin = new Thickness(10, 8, 0, 8),
                                VerticalAlignment = VerticalAlignment.Top,
                                Style = this.TryFindResource("ButtonClotting") as Style,
                                Height = 30,
                                Width = 30
                            };
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            Image image_delete = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Image/trash.png")),
                                Width = 30
                            }; button_delete.Content = image_delete;
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            button_delete.Click += ButtonDeleteSpec;
                            StackPanel textVert = new StackPanel
                            {

                            };
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 0),
                                Width = border.Width - 200
                            };
                            StackPanel textHoriz = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                            };
                            Label nine_class = new Label
                            {
                                Padding = new Thickness(0, 0, 0, 10),
                                Content = "9 класс"
                            };
                            if (reader.GetInt32(2) == 1) nine_class.Foreground = new SolidColorBrush(Colors.DarkGreen);
                            else nine_class.Foreground = new SolidColorBrush(Colors.DarkRed);
                            Label eleven_class = new Label
                            {
                                Padding = new Thickness(5, 0, 0, 10),
                                Content = "11 класс"
                            };
                            if (reader.GetInt32(3) == 1) eleven_class.Foreground = new SolidColorBrush(Colors.DarkGreen);
                            else eleven_class.Foreground = new SolidColorBrush(Colors.DarkRed);
                            textHoriz.Children.Add(nine_class);
                            textHoriz.Children.Add(eleven_class);
                            textVert.Children.Add(text);
                            textVert.Children.Add(textHoriz);
                            panel_button.Children.Add(button_edit);
                            panel_button.Children.Add(button_delete);
                            grid.Children.Add(textVert);
                            Grid.SetColumn(textVert, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            SpecList.Children.Add(border);
                        }
                    }
                    else
                    {
                        SpecList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        SpecList.Children.Add(not_list);
                    }
                }
                connection.Close();
            }
        }

        private void ButtonAddSpec(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            AddEditSpec addEditSpec = new AddEditSpec(0, 0);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditSpec.Owner = this;
                    addEditSpec.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditSpec.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonEditSpec(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            AddEditSpec addEditSpec = new AddEditSpec(1, button_id);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditSpec.Owner = this;
                    addEditSpec.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditSpec.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonDeleteSpec(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            DialogBox.Visibility = Visibility.Visible;
            CanvasMain.Content = "Удаление";
            CanvasDescription.Text = "Вы действительно хотите удалить специальность?";
            IsDelete = true;
            YesButton.Tag = $"{button_id}";
        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "ПРОХОДНЫЕ БАЛЛЫ"

        private void ShowListScore()
        {
            ScoreWindow.Visibility = Visibility.Visible;
            ScoreList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT Years FROM PassingScores WHERE Number = 1", connection);
                using (var reader = command.ExecuteReader()) if (reader.Read()) ScoreEdit.Text = $"{reader.GetInt32(0)}";
                command = new SQLiteCommand("SELECT * FROM Specialties", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditScore;
                            StackPanel textVert = new StackPanel
                            {

                            };
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 0),
                                Width = border.Width - 200
                            };
                            StackPanel textHoriz = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                            };
                            Label nine_class = new Label
                            {
                                Padding = new Thickness(0, 0, 0, 10),
                                Content = "9 класс"
                            };
                            if (reader.GetInt32(2) == 1) nine_class.Foreground = new SolidColorBrush(Colors.DarkGreen);
                            else nine_class.Foreground = new SolidColorBrush(Colors.DarkRed);
                            Label eleven_class = new Label
                            {
                                Padding = new Thickness(5, 0, 0, 10),
                                Content = "11 класс"
                            };
                            if (reader.GetInt32(3) == 1) eleven_class.Foreground = new SolidColorBrush(Colors.DarkGreen);
                            else eleven_class.Foreground = new SolidColorBrush(Colors.DarkRed);
                            textHoriz.Children.Add(nine_class);
                            textHoriz.Children.Add(eleven_class);
                            textVert.Children.Add(text);
                            textVert.Children.Add(textHoriz);
                            panel_button.Children.Add(button_edit);
                            grid.Children.Add(textVert);
                            Grid.SetColumn(textVert, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            ScoreList.Children.Add(border);
                        }
                    }
                    else
                    {
                        ScoreList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        ScoreList.Children.Add(not_list);
                    }
                }
                connection.Close();
            }
        }
        private void ButtonSaveScore(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            DialogBox.Visibility = Visibility.Visible;
            if (ScoreEdit.Text != "")
            {
                IsChangeData = true;
                CanvasMain.Content = "Изменить";
                CanvasDescription.Text = "Вы действительно хотите применить изменения?";
            }
            else
            {
                YesButton.Visibility = Visibility.Collapsed;
                CanvasMain.Content = "Ошибка";
                CanvasDescription.Text = "Поле ввода не должно быть пустым!";
                NoButton.Content = "Окей";
                IsError = true;
            }
        }
        private void ButtonEditScore(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;

        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "ПРЕДМЕТЫ"

        private void ShowListSubject()
        {
            SubjectWindow.Visibility = Visibility.Visible;
            SubjectList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Subject", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditSubject;
                            Button button_delete = new Button
                            {
                                Margin = new Thickness(10, 8, 0, 8),
                                VerticalAlignment = VerticalAlignment.Top,
                                Style = this.TryFindResource("ButtonClotting") as Style,
                                Height = 30,
                                Width = 30
                            };
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            Image image_delete = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Image/trash.png")),
                                Width = 30
                            }; button_delete.Content = image_delete;
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            button_delete.Click += ButtonDeleteSubject;
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 10),
                                Width = border.Width - 200
                            };
                            panel_button.Children.Add(button_edit);
                            panel_button.Children.Add(button_delete);
                            grid.Children.Add(text);
                            Grid.SetColumn(text, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            SubjectList.Children.Add(border);

                        }
                    }
                    else
                    {
                        SubjectList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        SubjectList.Children.Add(not_list);
                    }
                }
                connection.Close();
            }
        }
        private void ButtonAddSubject(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            AddEditSubject addEditSubject = new AddEditSubject(0, 0);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditSubject.Owner = this;
                    addEditSubject.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditSubject.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonEditSubject(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            AddEditSubject addEditSubject = new AddEditSubject(1, button_id);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditSubject.Owner = this;
                    addEditSubject.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditSubject.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonDeleteSubject(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            DialogBox.Visibility = Visibility.Visible;
            CanvasMain.Content = "Удаление";
            CanvasDescription.Text = "Вы действительно хотите удалить предмет?";
            IsDelete = true;
            YesButton.Tag = $"{button_id}";
        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "ГРУППЫ"

        private void ShowListGroup()
        {
            GroupWindow.Visibility = Visibility.Visible;
            GroupList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM GroupList", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditGroup;
                            Button button_delete = new Button
                            {
                                Margin = new Thickness(10, 8, 0, 8),
                                VerticalAlignment = VerticalAlignment.Top,
                                Style = this.TryFindResource("ButtonClotting") as Style,
                                Height = 30,
                                Width = 30
                            };
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            Image image_delete = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Image/trash.png")),
                                Width = 30
                            }; button_delete.Content = image_delete;
                            button_delete.Tag = $"{reader.GetInt32(0)}";
                            button_delete.Click += ButtonDeleteGroup;
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 10),
                                Width = border.Width - 200
                            };
                            panel_button.Children.Add(button_edit);
                            panel_button.Children.Add(button_delete);
                            grid.Children.Add(text);
                            Grid.SetColumn(text, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            GroupList.Children.Add(border);

                        }
                    }
                    else
                    {
                        GroupList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        GroupList.Children.Add(not_list);
                    }
                }
                connection.Close();
            }
        }
        private void ButtonAddGroup(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            AddEditGroup addEditGroup = new AddEditGroup(0, 0);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditGroup.Owner = this;
                    addEditGroup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditGroup.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonEditGroup(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            AddEditGroup addEditGroup = new AddEditGroup(1, button_id);
            bool? result = null;
            if (result == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addEditGroup.Owner = this;
                    addEditGroup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    result = addEditGroup.ShowDialog();
                    MyCanvas.Visibility = Visibility.Collapsed;
                    if (result == true)
                    {
                        UpDateList(sender, e);
                    }
                }));
            }
        }
        private void ButtonDeleteGroup(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            DialogBox.Visibility = Visibility.Visible;
            CanvasMain.Content = "Удаление";
            CanvasDescription.Text = "Вы действительно хотите удалить группу?";
            IsDelete = true;
            YesButton.Tag = $"{button_id}";
        }

        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "Расписания временного"

        private void ShowListSchedule()
        {
            ScheduleWindow.Visibility = Visibility.Visible;
            ScheduleList.Children.Clear();
            tempSchedule = new TempSchedule();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM GroupList", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            ComboBoxSelectDay.ItemsSource = DayWeekList;
                            ComboBoxSelectDay.SelectedIndex = 0;
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditSchedule;
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 10),
                                Width = border.Width - 200
                            };
                            panel_button.Children.Add(button_edit);
                            grid.Children.Add(text);
                            Grid.SetColumn(text, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            ScheduleList.Children.Add(border);

                        }
                    }
                    else
                    {
                        ScheduleList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список пуст",
                            FontSize = 20
                        };
                        ScheduleList.Children.Add(not_list);
                    }
                }
                //получаем временное расписание
                string json = "";
                command = new SQLiteCommand("SELECT List FROM ScheduleTemporary WHERE Number = 1", connection);
                using (var reader = command.ExecuteReader()) while (reader.Read()) json = reader.GetString(0);
                //если его не существует
                if (json == "[]")
                {
                    //создаем его
                    List<ScheduleTemp> groups = new List<ScheduleTemp>();
                    command = new SQLiteCommand("SELECT Number FROM GroupList", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ScheduleTemp group = new ScheduleTemp();
                            List<DayWeek> days = new List<DayWeek>();
                            for (int i = 0; i < 6; i++)
                            {
                                DayWeek day = new DayWeek();
                                List<Lesson> lessons = new List<Lesson>();
                                for (int j = 0; j < 12; j++)
                                {
                                    Lesson lesson = new Lesson()
                                    {
                                        SubjectID = 0,
                                        TeacherOneID = 0,
                                        TeacherTwoID = 0,
                                        OfficeOne = "Нет",
                                        OfficeTwo = "Нет"
                                    };
                                    lessons.Add(lesson);
                                }
                                day.Lessons = lessons;
                                days.Add(day);
                            }
                            group.GroupID = reader.GetInt32(0);
                            group.Days = days;
                            groups.Add(group);
                        }
                    }
                    tempSchedule.Groups = groups;
                    tempSchedule.DateMonday = "2025-07-07";
                    tempSchedule.DateTuesday = "2025-07-08";
                    tempSchedule.DateWednesday = "2025-07-09";
                    tempSchedule.DateThursday = "2025-07-10";
                    tempSchedule.DateFriday = "2025-07-11";
                    tempSchedule.DateSaturday = "2025-07-12";
                }
                else // если существует
                {
                    tempSchedule = JsonConvert.DeserializeObject<TempSchedule>(json);
                }
                ScheduleButtonSave.IsEnabled = false;
                connection.Close();
            }
        }
        private void ButtonEditSchedule(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            bool IsGroup = false;
            for (int i = 0; i < tempSchedule.Groups.Count; i++)
            {
                if (tempSchedule.Groups[i].GroupID == button_id) // если в полученном расписании есть нужная нам группы 
                {
                    IsGroup = true;
                }
            }
            if(!IsGroup) // если нету создаем ее
            {
                ScheduleTemp groupSchedule = new ScheduleTemp();
                groupSchedule.GroupID = button_id;
                List<DayWeek> days = new List<DayWeek>();
                for (int j = 0; j < 6; j++)
                {
                    DayWeek day = new DayWeek();
                    List<Lesson> lessons = new List<Lesson>();
                    for (int z = 0; z < 12; z++)
                    {
                        Lesson lesson = new Lesson()
                        {
                            SubjectID = 0,
                            TeacherOneID = 0,
                            TeacherTwoID = 0,
                            OfficeOne = "Нет",
                            OfficeTwo = "Нет"
                        };
                        lessons.Add(lesson);
                    }
                    day.Lessons = lessons;
                    days.Add(day);
                }
                groupSchedule.Days = days;
                tempSchedule.Groups.Add(groupSchedule);
            }
            for (int i = 0; i < tempSchedule.Groups.Count; i++)
            {
                Debug.Print($"Group {i}: {tempSchedule.Groups[i].GroupID} =?= {button_id}");
                if (tempSchedule.Groups[i].GroupID == button_id)
                {
                    AddEditSchedule addEditGroup = new AddEditSchedule(ComboBoxSelectDay.SelectedIndex, button_id, tempSchedule.Groups[i]);
                    int index = i; // предотвращение изменения i, т.к цикл и действие присваивания расписания происходит в ассинхроне
                    bool? result = null;
                    if (result == null)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            addEditGroup.Owner = this;
                            addEditGroup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            result = addEditGroup.ShowDialog();
                            MyCanvas.Visibility = Visibility.Collapsed;
                            if (result == true)
                            {
                                tempSchedule.Groups[index].Days[ComboBoxSelectDay.SelectedIndex].Lessons = addEditGroup.schedule.Lessons;
                                ScheduleButtonSave.IsEnabled = true;
                                IsChangeSchedule = true;
                            }
                        }));
                    }
                }
            }
        }
        private void ButtonSaveSchedule(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            DialogBox.Visibility = Visibility.Visible;
            IsSaveSchedule = true;
            CanvasMain.Content = "Сохранение";
            CanvasDescription.Text = "Вы действительно хотите сохранить временное расписание?";
        }

        private void ButtonDateChange(object sender, EventArgs e)
        {

        }
        // ФУНКЦИОНАЛ ОТОБРАЖЕНИЯ "Расписания постоянного"

        private void ShowListPermSchedule()
        {
            PermScheduleWindow.Visibility = Visibility.Visible;
            PermScheduleList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM GroupList", connection);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Border border = new Border
                            {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(10, 5, 10, 5),
                                Width = EditBorder.Width - 40,
                                Margin = new Thickness(10, 10, 0, 0)
                            };
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(0),


                            }; border.Child = grid;
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            StackPanel panel_button = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                            };
                            Button button_edit = new Button
                            {
                                Style = this.TryFindResource("RoundedButtonStyle") as Style,
                                FontSize = 18,
                                Padding = new Thickness(20, 5, 20, 5),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 8, 0, 8),
                                Content = "Изменить",
                                Height = 35
                            };
                            button_edit.Tag = $"{reader.GetInt32(0)}";
                            button_edit.Click += ButtonEditPermSchedule;
                            TextBlock text = new TextBlock
                            {
                                Text = $"{reader.GetString(1)}",
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                Padding = new Thickness(0, 10, 0, 10),
                                Width = border.Width - 200
                            };
                            panel_button.Children.Add(button_edit);
                            grid.Children.Add(text);
                            Grid.SetColumn(text, 0);
                            Grid gridPanelButtonWrapper = new Grid();
                            gridPanelButtonWrapper.Children.Add(panel_button);
                            grid.Children.Add(gridPanelButtonWrapper);
                            Grid.SetColumn(gridPanelButtonWrapper, 1);
                            PermScheduleList.Children.Add(border);
                        }
                    }
                    else
                    {
                        Debug.Print("Сработало");
                        PermScheduleList.HorizontalAlignment = HorizontalAlignment.Center;
                        Label not_list = new Label
                        {
                            Content = "Список групп пуст",
                            Padding = new Thickness(0,5,0,0),
                            HorizontalAlignment= HorizontalAlignment.Center,
                            FontSize = 20
                        };
                        Label not_list2 = new Label
                        {
                            Content = "Добавьте хотябы одну запись в список групп",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Padding = new Thickness(0),
                            FontSize = 15
                        };
                        PermScheduleList.Children.Add(not_list);
                        PermScheduleList.Children.Add(not_list2);
                    }
                }
                connection.Close();
            }
        }
        private void ButtonEditPermSchedule(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int button_id = Convert.ToInt32(button.Tag);
            MyCanvas.Visibility = Visibility.Visible;
            bool IsTeacher = false, IsSubject = false;
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Teacher", connection);
                using(var reader = command.ExecuteReader())
                {
                    if (reader.HasRows) IsTeacher = true;
                }
                command = new SQLiteCommand("SELECT * FROM Subject", connection);
                using(var reader = command.ExecuteReader())
                {
                    if (reader.HasRows) IsSubject = true;
                }
                if (IsTeacher && IsSubject)
                {
                    AddEditPermSchedule addEditPermSchedule = new AddEditPermSchedule(button_id);
                    bool? result = null;
                    if (result == null)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            addEditPermSchedule.Owner = this;
                            addEditPermSchedule.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            result = addEditPermSchedule.ShowDialog();
                            MyCanvas.Visibility = Visibility.Collapsed;
                            if (result == true)
                            {
                                
                            }
                        }));
                    }
                } else
                {
                    DialogBox.Visibility = Visibility.Visible;
                    IsError = true;
                    if(!IsTeacher)
                    {
                        CanvasMain.Content = "Ошибка";
                        CanvasDescription.Text = "Добавьте хотябы одну запись в таблицу преподавателей!";
                        YesButton.Visibility = Visibility.Collapsed;
                    } else
                    {
                        CanvasMain.Content = "Ошибка";
                        CanvasDescription.Text = "Добавьте хотябы одну запись в таблицу предметов!";
                        YesButton.Visibility = Visibility.Collapsed;
                    }
                }
                connection.Close();
            }
        }

        // Перевод из byte в bitmap
        private BitmapImage ConvertByteArrayToImage(byte[] imageData)
        {
            using (MemoryStream stream = new MemoryStream(imageData))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        // КЛАССЫ

        // Проходные баллы
        public class Special
        {
            public int id { get; set; }
            public string name { get; set; }
            public int IsNine { get; set; }
            public int IsEleven { get; set; }
            public List<Form> forms { get; set; }
        }
        public class Form
        {
            public int id { get; set; }
            public string DayBudgetPassing { get; set; }
            public string DayBudgetContest { get; set; }
            public string DayPaidPassing { get; set; }
            public string DayPaidContest { get; set; }
            public string ZaoBudgetPassing { get; set; }
            public string ZaoBudgetContest { get; set; }
            public string ZaoPaidPassing { get; set; }
            public string ZaoPaidContest { get; set; }
        }

        // Группа
        public class TempSchedule
        {
            public string DateMonday { get; set; }
            public string DateTuesday { get; set; }
            public string DateWednesday { get; set; }
            public string DateThursday { get; set; }
            public string DateFriday { get; set; }
            public string DateSaturday { get; set; }
            public List<ScheduleTemp> Groups { get; set; }
        }
        public class ScheduleTemp
        {
            public int GroupID {  get; set; }
            public List<DayWeek> Days { get; set; } 
        }
    }
}

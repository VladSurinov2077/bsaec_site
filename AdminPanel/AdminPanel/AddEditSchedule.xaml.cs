using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace AdminPanel
{
    public partial class AddEditSchedule : Window
    {
        int selectDay = 0, id = 0;
        bool IsClosed = false, IsChange;
        private string connectingString = App.ConnectionString;

        // списки учителей, предметов и кабинетов
        List<string> listTeacher = new List<string>() { "Нет" };
        List<string> listSubject = new List<string>() { "Нет" };
        List<string> listOffice = new List<string>
        {
            "Нет",
            "с/з",
            "16-Б",
            "18-Б",
            "20-Б",
            "22-Б",
            "25-Б",
            "26-Б",
            "28-Б",
            "30-Б",
            "31-Б",
            "32-Б",
            "33-Б",
            "34-Б",
            "36-Б",
            "37-Б",
            "1-С",
            "2-C",
            "5-С",
            "6-C",
            "8-С",
            "9-C",
            "16-C",
            "17-C",
            "18-C",
            "19-C",
            "24-C",
            "25-C",
            "26-C",
            "28-C",
            "31-C",
            "1-Л",
            "Маст"
        };

        //структура дня (БД) для переноса на родительское окно
        public DayWeek schedule = new DayWeek();

        //структура дня (локальная) для комбобоксов
        List<LessonCombo> scheduleCombo = new List<LessonCombo>();

        //структура временного расписания группы (чтобы достать из бд и присвоить структуре дня данные из нее)
        MainWindow.ScheduleTemp scheduleTemp = new MainWindow.ScheduleTemp();

        //структура постоянного расписания  и вставить вместо временного (в случае если его нет) или сбросить к постоянному временное
        List<DayWeek> schedulePerm = new List<DayWeek>();
        
        //список ComboBox`ов 
        List<ComboBox> comboboxSubjects = new List<ComboBox>();
        List<ComboBox> comboboxTeachers1 = new List<ComboBox>();
        List<ComboBox> comboboxTeachers2 = new List<ComboBox>();
        List<ComboBox> comboboxOffices1 = new List<ComboBox>();
        List<ComboBox> comboboxOffices2 = new List<ComboBox>();

        public AddEditSchedule(int selectDay, int id, MainWindow.ScheduleTemp schedule) // выбранный день и id группы 
        {
            this.selectDay = selectDay;
            this.id = id;
            this.scheduleTemp = schedule;
            InitializeComponent();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                string json = "";

                //заполнение списка предметов и преподов
                SQLiteCommand command = new SQLiteCommand("SELECT Surname, Name FROM Teacher", connection);
                using (var reader = command.ExecuteReader()) while (reader.Read()) listTeacher.Add(reader.GetString(0) + " " + reader.GetString(1));
                command = new SQLiteCommand("SELECT Name FROM Subject", connection);
                using (var reader = command.ExecuteReader()) while (reader.Read()) listSubject.Add(reader.GetString(0));

                // достаем постоянное расписание нашей грруппы
                command = new SQLiteCommand("SELECT List FROM SchedulePermanent WHERE GroupID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    //если есть постоянное расписание
                    while (reader.Read()) json = reader.GetString(0);
                    
                    //если нет, то создаем пустое
                    if(json=="[]")
                    {
                        for(int i=0; i<6; i++)
                        {
                            DayWeek day = new DayWeek();
                            List<Lesson> lessons = new List<Lesson>(); 
                            for(int j = 0; j < 12; j++)
                            {
                                Lesson lesson = new Lesson
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
                            schedulePerm.Add(day);
                        }
                    } else
                    {
                        schedulePerm = JsonConvert.DeserializeObject<List<DayWeek>>(json);
                    }

                    // создание расписания под combobox`ы
                    for(int i=0; i<12; i++)
                    {
                        LessonCombo lesson = new LessonCombo{
                            SelectSubject = 0, SelectTeacherOne =0, SelectTeacherTwo = 0, SelectOfficeOne = 0, SelectOfficeTwo = 0
                        };
                        scheduleCombo.Add(lesson);
                    }
                }
                //творим и смешиваем временное и постоянное расписание воедино и превращаем его в расписание для combobox`ов
                ConvertToCombo();
                connection.Close();
            }
        }
        private void ConvertToCombo()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command;
                // присваивание постоянного расписания структуры расписания для combobox`ов
                for (int i = 0; i < 6; i++)
                {
                    if (i == selectDay)
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            string SubjectName = "", TeacherOneName = "", TeacherTwoName = "";
                            if (schedulePerm[i].Lessons[j].SubjectID != 0)
                            {
                                command = new SQLiteCommand("SELECT Name FROM Subject WHERE Number = @id",connection);
                                command.Parameters.AddWithValue("@id", schedulePerm[i].Lessons[j].SubjectID);
                                using(var reader = command.ExecuteReader()) while (reader.Read()) SubjectName = reader.GetString(0);
                            }
                            else SubjectName = "Нет";
                            if (schedulePerm[i].Lessons[j].TeacherOneID != 0)
                            {
                                command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                                command.Parameters.AddWithValue("@id", schedulePerm[i].Lessons[j].TeacherOneID);
                                using (var reader = command.ExecuteReader()) while (reader.Read()) TeacherOneName = reader.GetString(0) + " " + reader.GetString(1);
                            }
                            else TeacherOneName = "Нет";
                            if (schedulePerm[i].Lessons[j].TeacherTwoID != 0)
                            {
                                command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                                command.Parameters.AddWithValue("@id", schedulePerm[i].Lessons[j].TeacherTwoID);
                                using (var reader = command.ExecuteReader()) while (reader.Read()) TeacherTwoName = reader.GetString(0) + " " + reader.GetString(1);
                            }
                            else TeacherTwoName = "Нет";
                            scheduleCombo[j].SelectSubject = listSubject.IndexOf(SubjectName);
                            scheduleCombo[j].SelectTeacherOne = listTeacher.IndexOf(TeacherOneName);
                            scheduleCombo[j].SelectTeacherTwo = listTeacher.IndexOf(TeacherTwoName);
                            scheduleCombo[j].SelectOfficeOne = listOffice.IndexOf(schedulePerm[i].Lessons[j].OfficeOne);
                            scheduleCombo[j].SelectOfficeTwo = listOffice.IndexOf(schedulePerm[i].Lessons[j].OfficeTwo);
                        }
                    }
                }

                // замещение уже временным расписанием (предусматриваем чтобы небыло замен пустых данных на имеющиеся
                for (int j = 0; j < 6; j++)
                {
                    if (j == selectDay)
                    {
                        Debug.Print($"День {j}:");
                        int count = 0;
                        for (int z = 0; z < 12; z++)
                        {
                            // проверка на пустой слот
                            if (scheduleTemp.Days[j].Lessons[z].SubjectID == 0 && scheduleTemp.Days[j].Lessons[z].TeacherOneID == 0 && scheduleTemp.Days[j].Lessons[z].TeacherTwoID == 0 &&
                                scheduleTemp.Days[j].Lessons[z].OfficeOne == "Нет" && scheduleTemp.Days[j].Lessons[z].OfficeTwo == "Нет") count++;
                        } // если не все слоты во всех днях пустые
                        if (count != 12)
                        {
                            for (int z = 0; z < 12; z++)
                            {
                                Debug.Print($"\tУрок {z}:\n"+
                                    "\t\t" + $"SubjectID = {scheduleTemp.Days[j].Lessons[z].SubjectID}"+
                                    "\t\t" + $"TeacherOneID = {scheduleTemp.Days[j].Lessons[z].TeacherOneID}" +
                                    "\t\t" + $"TeacherTwoID = {scheduleTemp.Days[j].Lessons[z].TeacherTwoID}" +
                                    "\t\t" + $"OfficeOne = {scheduleTemp.Days[j].Lessons[z].OfficeOne}" +
                                    "\t\t" + $"OfficeTwo = {scheduleTemp.Days[j].Lessons[z].OfficeTwo}");
                                string SubjectName = "", TeacherOneName = "", TeacherTwoName = "";
                                if (scheduleTemp.Days[j].Lessons[z].SubjectID != 0)
                                {
                                    command = new SQLiteCommand("SELECT Name FROM Subject WHERE Number = @id",connection);
                                    command.Parameters.AddWithValue("@id", scheduleTemp.Days[j].Lessons[z].SubjectID);
                                    using (var reader = command.ExecuteReader()) while (reader.Read()) SubjectName = reader.GetString(0);
                                }
                                else SubjectName = "Нет";
                                if (scheduleTemp.Days[j].Lessons[z].TeacherOneID != 0)
                                {
                                    command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                                    command.Parameters.AddWithValue("@id", scheduleTemp.Days[j].Lessons[z].TeacherOneID);
                                    using (var reader = command.ExecuteReader()) while (reader.Read()) TeacherOneName = reader.GetString(0) + " " + reader.GetString(1);
                                }
                                else TeacherOneName = "Нет";
                                if (scheduleTemp.Days[j].Lessons[z].TeacherTwoID != 0)
                                {
                                    command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                                    command.Parameters.AddWithValue("@id", scheduleTemp.Days[j].Lessons[z].TeacherTwoID);
                                    using (var reader = command.ExecuteReader()) while (reader.Read()) TeacherTwoName = reader.GetString(0) + " " + reader.GetString(1);
                                }
                                else TeacherTwoName = "Нет";
                                scheduleCombo[z].SelectSubject = listSubject.IndexOf(SubjectName);
                                scheduleCombo[z].SelectTeacherOne = listTeacher.IndexOf(TeacherOneName);
                                scheduleCombo[z].SelectTeacherTwo = listTeacher.IndexOf(TeacherTwoName);
                                scheduleCombo[z].SelectOfficeOne = listOffice.IndexOf(scheduleTemp.Days[j].Lessons[z].OfficeOne);
                                scheduleCombo[z].SelectOfficeTwo = listOffice.IndexOf(scheduleTemp.Days[j].Lessons[z].OfficeTwo);
                            }
                        }
                    }
                }
                // отображаем список уроков
                ShowListSchedule();
                connection.Close();
            }
        }
        private void SaveSchedule(object sender, EventArgs e)
        {
            for(int i = 0; i<12; i++)
            {
                scheduleCombo[i].SelectSubject = comboboxSubjects[i].SelectedIndex;
                scheduleCombo[i].SelectTeacherOne = comboboxTeachers1[i].SelectedIndex;
                scheduleCombo[i].SelectTeacherTwo = comboboxTeachers2[i].SelectedIndex;
                scheduleCombo[i].SelectOfficeOne = comboboxOffices1[i].SelectedIndex;
                scheduleCombo[i].SelectOfficeTwo = comboboxOffices2[i].SelectedIndex;
            }
            MyCanvas.Visibility = Visibility.Visible;
            CanvasMain.Content = "Сохранение";
            CanvasDescription.Text = "Вы действительно хотите сохранить изменения?";
        }
        private void ResetSchedule(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command;
                for (int i = 0; i < 6; i++)
                {
                    if (i == selectDay)
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            string SubjectName = "", TeacherOneName = "", TeacherTwoName = "";
                            if (schedulePerm[i].Lessons[j].SubjectID != 0)
                            {
                                command = new SQLiteCommand("SELECT Name FROM Subject WHERE Number = @id", connection);
                                command.Parameters.AddWithValue("@id", schedulePerm[i].Lessons[j].SubjectID);
                                using (var reader = command.ExecuteReader()) while (reader.Read()) SubjectName = reader.GetString(0);
                            }
                            else SubjectName = "Нет";
                            if (schedulePerm[i].Lessons[j].TeacherOneID != 0)
                            {
                                command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                                command.Parameters.AddWithValue("@id", schedulePerm[i].Lessons[j].TeacherOneID);
                                using (var reader = command.ExecuteReader()) while (reader.Read()) TeacherOneName = reader.GetString(0) + " " + reader.GetString(1);
                            }
                            else TeacherOneName = "Нет";
                            if (schedulePerm[i].Lessons[j].TeacherTwoID != 0)
                            {
                                command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                                command.Parameters.AddWithValue("@id", schedulePerm[i].Lessons[j].TeacherTwoID);
                                using (var reader = command.ExecuteReader()) while (reader.Read()) TeacherTwoName = reader.GetString(0) + " " + reader.GetString(1);
                            }
                            else TeacherTwoName = "Нет";
                            scheduleCombo[j].SelectSubject = listSubject.IndexOf(SubjectName);
                            scheduleCombo[j].SelectTeacherOne = listTeacher.IndexOf(TeacherOneName);
                            scheduleCombo[j].SelectTeacherTwo = listTeacher.IndexOf(TeacherTwoName);
                            scheduleCombo[j].SelectOfficeOne = listOffice.IndexOf(schedulePerm[i].Lessons[j].OfficeOne);
                            scheduleCombo[j].SelectOfficeTwo = listOffice.IndexOf(schedulePerm[i].Lessons[j].OfficeTwo);
                            /*Debug.Print($"\tУрок {j}:\n" +
                                                "\t\t" + $"SubjectID = {scheduleCombo[j].SelectSubject}" +
                                                "\t\t" + $"TeacherOneID = {scheduleCombo[j].SelectTeacherOne}" +
                                                "\t\t" + $"TeacherTwoID = {scheduleCombo[j].SelectTeacherTwo}" +
                                                "\t\t" + $"OfficeOne = {scheduleCombo[j].SelectOfficeOne}" +
                                                "\t\t" + $"OfficeTwo = {scheduleCombo[j].SelectOfficeTwo}");*/
                        }
                    }
                }
                for(int i=0; i<12; i++)
                {
                    comboboxSubjects[i].SelectedIndex = scheduleCombo[i].SelectSubject;
                    comboboxTeachers1[i].SelectedIndex = scheduleCombo[i].SelectTeacherOne;
                    comboboxTeachers2[i].SelectedIndex = scheduleCombo[i].SelectTeacherTwo;
                    comboboxOffices1[i].SelectedIndex = scheduleCombo[i].SelectOfficeOne;
                    comboboxOffices2[i].SelectedIndex = scheduleCombo[i].SelectOfficeTwo;
                }
                connection.Close();
            }
        }
        private void ClosingWindow(object sender, EventArgs e)
        {
            if (IsChange)
            {
                IsClosed = true;
                MyCanvas.Visibility = Visibility.Visible;
                CanvasMain.Content = "Выход";
                CanvasDescription.Text = "Вы действительно хотите отменить изменения?";
            }
            else this.Close();
        }
        private void ButtonYes(object sender, EventArgs e)
        {
            if (IsClosed) this.Close();
            else
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    List<Lesson> lessons = new List<Lesson>();
                    for (int i = 0; i < 12; i++)
                    {
                        Lesson lesson = new Lesson();
                        if (scheduleCombo[i].SelectSubject != 0)
                        {
                            SQLiteCommand command = new SQLiteCommand("SELECT Number FROM Subject WHERE Name = @name", connection);
                            command.Parameters.AddWithValue("@name", listSubject[scheduleCombo[i].SelectSubject]);
                            using(var reader = command.ExecuteReader()) while(reader.Read()) lesson.SubjectID = reader.GetInt32(0);
                        }
                        else lesson.SubjectID = 0;
                        if (scheduleCombo[i].SelectTeacherOne != 0)
                        {
                            string teacher = listTeacher[scheduleCombo[i].SelectTeacherOne];
                            SQLiteCommand command = new SQLiteCommand("SELECT Number FROM Teacher WHERE Surname = @surname AND Name = @name", connection);
                            command.Parameters.AddWithValue("@surname", teacher.Split(' ')[0]);
                            command.Parameters.AddWithValue("@name", teacher.Split(' ')[1]);
                            using (var reader = command.ExecuteReader()) while (reader.Read()) lesson.TeacherOneID = reader.GetInt32(0);
                        }
                        else lesson.TeacherOneID = 0;
                        if (scheduleCombo[i].SelectTeacherTwo != 0)
                        {
                            string teacher = listTeacher[scheduleCombo[i].SelectTeacherTwo];
                            SQLiteCommand command = new SQLiteCommand("SELECT Number FROM Teacher WHERE Surname = @surname AND Name = @name", connection);
                            command.Parameters.AddWithValue("@surname", teacher.Split(' ')[0]);
                            command.Parameters.AddWithValue("@name", teacher.Split(' ')[1]);
                            using (var reader = command.ExecuteReader()) while (reader.Read()) lesson.TeacherTwoID = reader.GetInt32(0);
                        }
                        else lesson.TeacherTwoID = 0;
                        lesson.OfficeOne = listOffice[scheduleCombo[i].SelectOfficeOne];
                        lesson.OfficeTwo = listOffice[scheduleCombo[i].SelectOfficeTwo];
                        lessons.Add(lesson);
                    }
                    schedule.Lessons = lessons;
                    connection.Close();
                }
                DialogResult = true;
                this.Close();
            }
        }
        private void ButtonNo(object sender, EventArgs e)
        {
            if (IsClosed) IsClosed = false;
            MyCanvas.Visibility = Visibility.Collapsed;
        }
        private void ComboChange(object sender, EventArgs e)
        {
            IsChange = true;
            MainButton.IsEnabled = true;
        }
        private void ShowListSchedule()
        {
            ScheduleTermList.Children.Clear();
            Debug.Print("Процесс отображения");
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                for (int i = 1; i != 13; i++)
                {
                    Debug.Print($"\tУрок {i-1}:\n" +
                                    "\t\t" + $"SubjectID = {scheduleCombo[i - 1].SelectSubject}" +
                                    "\t\t" + $"TeacherOneID = {scheduleCombo[i - 1].SelectTeacherOne}" +
                                    "\t\t" + $"TeacherTwoID = {scheduleCombo[i - 1].SelectTeacherTwo}" +
                                    "\t\t" + $"OfficeOne = {scheduleCombo[i - 1].SelectOfficeOne}" +
                                    "\t\t" + $"OfficeTwo = {scheduleCombo[i - 1 ].SelectOfficeTwo}");
                    Grid grid = new Grid
                    {
                        RowDefinitions =
                        {
                            new RowDefinition
                            {
                                Height = GridLength.Auto
                            }
                        },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition
                            {
                                Width = GridLength.Auto
                            },
                            new ColumnDefinition
                            {
                                Width = new GridLength(1.0, GridUnitType.Star)
                            },
                            new ColumnDefinition
                            {
                                Width = new GridLength(1.0, GridUnitType.Star)
                            },
                            new ColumnDefinition
                            {
                                Width = new GridLength(120.0)
                            }
                        },
                        Margin = new Thickness(0.0, 0.0, 0.0, 30.0)
                    };
                    Label number = new Label
                    {
                        Content = string.Format("{0}", i),
                        FontSize = 20.0,
                        Width = 30.0,
                        HorizontalContentAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(number, 0);
                    Grid.SetRow(number, 0);
                    grid.Children.Add(number);
                    ComboBox comboboxSubject = new ComboBox
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        Height = 30.0,
                        Margin = new Thickness(10.0, 0.0, 10.0, 0.0),
                        ItemsSource = listSubject,
                        SelectedIndex = scheduleCombo[i-1].SelectSubject
                    };
                    comboboxSubjects.Add(comboboxSubject);
                    comboboxSubject.SelectionChanged += ComboChange;
                    Grid.SetColumn(comboboxSubject, 1);
                    Grid.SetRow(comboboxSubject, 0);
                    grid.Children.Add(comboboxSubject);
                    StackPanel panelTeacher = new StackPanel();
                    ComboBox comboboxTeacher = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 0.0, 10.0, 5.0),
                        ItemsSource = listTeacher,
                        SelectedIndex = scheduleCombo[i - 1].SelectTeacherOne
                    };
                    comboboxTeacher.SelectionChanged += ComboChange;
                    comboboxTeachers1.Add(comboboxTeacher);
                    ComboBox comboboxTeacher2 = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 5.0, 10.0, 0.0),
                        ItemsSource = listTeacher,
                        SelectedIndex = scheduleCombo[i - 1].SelectTeacherTwo
                    };
                    comboboxTeacher2.SelectionChanged += ComboChange;
                    comboboxTeachers2.Add(comboboxTeacher2);
                    panelTeacher.Children.Add(comboboxTeacher);
                    panelTeacher.Children.Add(comboboxTeacher2);
                    Grid.SetColumn(panelTeacher, 2);
                    Grid.SetRow(panelTeacher, 0);
                    grid.Children.Add(panelTeacher);
                    StackPanel panelOffice = new StackPanel();
                    ComboBox comboboxOffice = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 0.0, 10.0, 5.0),
                        ItemsSource = listOffice,
                        SelectedIndex = scheduleCombo[i - 1].SelectOfficeOne
                    };
                    comboboxOffice.SelectionChanged += ComboChange;
                    comboboxOffices1.Add(comboboxOffice);
                    ComboBox comboboxOffice2 = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 5.0, 10.0, 0.0),
                        ItemsSource = listOffice,
                        SelectedIndex = scheduleCombo[i - 1].SelectOfficeTwo
                    };
                    comboboxOffice2.SelectionChanged += ComboChange;
                    comboboxOffices2.Add(comboboxOffice2);
                    panelOffice.Children.Add(comboboxOffice);
                    panelOffice.Children.Add(comboboxOffice2);
                    Grid.SetColumn(panelOffice, 3);
                    Grid.SetRow(panelOffice, 0);
                    grid.Children.Add(panelOffice);
                    ScheduleTermList.Children.Add(grid);
                }
                connection.Close();
            }

        }
    }
    // класс хранения расписания (БД)
    public class DayWeek
    {
        public List<Lesson> Lessons { get; set; } 
    }
    public class Lesson
    {
        public int SubjectID { get; set; }
        public int TeacherOneID { get; set; }
        public int TeacherTwoID { get; set; }
        public string OfficeOne { get; set; }
        public string OfficeTwo { get; set; }
    }

    // класс хранения расписания (локальное для комбобоксов)
    public class LessonCombo
    {
        public int SelectSubject { get; set; }
        public int SelectTeacherOne { get; set; }
        public int SelectTeacherTwo { get; set; }
        public int SelectOfficeOne { get; set; }
        public int SelectOfficeTwo { get; set; }
    }
}

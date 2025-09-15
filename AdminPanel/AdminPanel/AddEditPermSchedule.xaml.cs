using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
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
    public partial class AddEditPermSchedule : Window
    {
        bool IsClosed = false;
        int id = 0, selectDay = 0;
        List<string> listTeacher, listSubject;
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
        List<string> DayWeekList = new List<string>
        {
            "Понедельник",
            "Вторник",
            "Среда",
            "Четверг",
            "Пятница",
            "Суббота"
        };
        List<ComboBox> comboboxSubjects = new List<ComboBox>();
        List<ComboBox> comboboxTeachers1 = new List<ComboBox>();
        List<ComboBox> comboboxTeachers2 = new List<ComboBox>();
        List<ComboBox> comboboxOffices1 = new List<ComboBox>();
        List<ComboBox> comboboxOffices2 = new List<ComboBox>();
        private string connectingString = App.ConnectionString;
        private List<DayWeekTemporary> scheduleTemporary = new List<DayWeekTemporary>();
        private List<DayWeek> schedule = new List<DayWeek>();
        public AddEditPermSchedule(int id)
        {
            this.id = id;
            InitializeComponent();
            listTeacher = new List<string>
            {
                "Нет"
            };
            listSubject = new List<string>
            {
                "Нет"
            };
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT Surname, Name FROM Teacher", connection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string text = reader.GetString(0) + " " + reader.GetString(1);
                        listTeacher.Add(text);
                    }
                }
                command = new SQLiteCommand("SELECT Name FROM Subject", connection);
                using (SQLiteDataReader reader2 = command.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        listSubject.Add(reader2.GetString(0));
                    }
                }
                command = new SQLiteCommand("SELECT Name FROM GroupList WHERE Number = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (SQLiteDataReader reader3 = command.ExecuteReader())
                {
                    while (reader3.Read())
                    {
                        MainLabel.Content = "Изменение расписания группы " + reader3.GetString(0);
                    }
                }
                command = new SQLiteCommand("SELECT List FROM SchedulePermanent WHERE GroupID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (SQLiteDataReader reader4 = command.ExecuteReader())
                {
                    while (reader4.Read())
                    {
                        bool flag = reader4.GetString(0) != "[]";
                        if (flag)
                        {
                            schedule = JsonConvert.DeserializeObject<List<AddEditPermSchedule.DayWeek>>(reader4.GetString(0));
                        }
                        else
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                AddEditPermSchedule.DayWeek dayWeek = new AddEditPermSchedule.DayWeek();
                                schedule.Add(dayWeek);
                                List<AddEditPermSchedule.Lesson> lessons = new List<AddEditPermSchedule.Lesson>();
                                for (int j = 0; j < 12; j++)
                                {
                                    AddEditPermSchedule.Lesson lesson = new AddEditPermSchedule.Lesson
                                    {
                                        SubjectID = 0,
                                        TeacherOneID = 0,
                                        TeacherTwoID = 0,
                                        OfficeOne = "Нет",
                                        OfficeTwo = "Нет"
                                    };
                                    lessons.Add(lesson);
                                }
                                schedule[i].Lessons = lessons;
                            }
                        }
                    }
                }
                for (int k = 0; k < 6; k++)
                {
                    AddEditPermSchedule.DayWeekTemporary dayWeekTemporary = new AddEditPermSchedule.DayWeekTemporary();
                    List<AddEditPermSchedule.LessonTemporary> lessons2 = new List<AddEditPermSchedule.LessonTemporary>();
                    for (int l = 0; l < 12; l++)
                    {
                        lessons2.Add(new AddEditPermSchedule.LessonTemporary());
                    }
                    dayWeekTemporary.Lessons = lessons2;
                    scheduleTemporary.Add(dayWeekTemporary);
                }
                ConvertToTemporaryClass();
                connection.Close();
            }
            ComboDay.ItemsSource = DayWeekList;
            ComboDay.SelectedIndex = 0;
            ComboDay.SelectionChanged += new SelectionChangedEventHandler(ChangeDay);
            MainButton.IsEnabled = false;
            ShowListSchedule(0);
        }
        private void ConvertToTemporaryClass()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        string subjectName = "";
                        string teacherOneName = "";
                        string teacherTwoName = "";
                        bool flag = schedule[i].Lessons[j].SubjectID != 0;
                        if (flag)
                        {
                            SQLiteCommand command = new SQLiteCommand("SELECT Name FROM Subject WHERE Number = @id", connection);
                            command.Parameters.AddWithValue("@id", schedule[i].Lessons[j].SubjectID);
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    subjectName = reader.GetString(0);
                                }
                            }
                        }
                        else
                        {
                            subjectName = "Нет";
                        }
                        scheduleTemporary[i].Lessons[j].SelectSubject = listSubject.IndexOf(subjectName);
                        bool flag2 = schedule[i].Lessons[j].TeacherOneID != 0;
                        if (flag2)
                        {
                            SQLiteCommand command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                            command.Parameters.AddWithValue("@id", schedule[i].Lessons[j].TeacherOneID);
                            using (SQLiteDataReader reader2 = command.ExecuteReader())
                            {
                                while (reader2.Read())
                                {
                                    teacherOneName = reader2.GetString(0) + " " + reader2.GetString(1);
                                }
                            }
                        }
                        else
                        {
                            teacherOneName = "Нет";
                        }
                        scheduleTemporary[i].Lessons[j].SelectTeacherOne = listTeacher.IndexOf(teacherOneName);
                        bool flag3 = schedule[i].Lessons[j].TeacherTwoID != 0;
                        if (flag3)
                        {
                            SQLiteCommand command = new SQLiteCommand("SELECT Surname, Name FROM Teacher WHERE Number = @id", connection);
                            command.Parameters.AddWithValue("@id", schedule[i].Lessons[j].TeacherTwoID);
                            using (SQLiteDataReader reader3 = command.ExecuteReader())
                            {
                                while (reader3.Read())
                                {
                                    teacherTwoName = reader3.GetString(0) + " " + reader3.GetString(1);
                                }
                            }
                        }
                        else
                        {
                            teacherTwoName = "Нет";
                        }
                        scheduleTemporary[i].Lessons[j].SelectTeacherTwo = listTeacher.IndexOf(teacherTwoName);
                        scheduleTemporary[i].Lessons[j].SelectOfficeOne = listOffice.IndexOf(schedule[i].Lessons[j].OfficeOne);
                        scheduleTemporary[i].Lessons[j].SelectOfficeTwo = listOffice.IndexOf(schedule[i].Lessons[j].OfficeTwo);
                    }
                }
                connection.Close();
            }
        }
        private void ChangeDay(object sender, EventArgs e)
        {
            this.comboboxSubjects.Clear();
            comboboxTeachers1.Clear();
            comboboxTeachers2.Clear();
            comboboxOffices1.Clear();
            comboboxOffices2.Clear();
            ShowListSchedule(ComboDay.SelectedIndex);
            this.selectDay = ComboDay.SelectedIndex;
        }
        private void ShowListSchedule(int DayID)
        {
            ScheduleList.Children.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(connectingString))
            {
                connection.Open();
                for (int i = 1; i != 13; i++)
                {
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
                        SelectedIndex = scheduleTemporary[DayID].Lessons[i - 1].SelectSubject
                    };
                    comboboxSubject.SelectionChanged += new SelectionChangedEventHandler(ChangedCombo);
                    comboboxSubjects.Add(comboboxSubject);
                    Grid.SetColumn(comboboxSubject, 1);
                    Grid.SetRow(comboboxSubject, 0);
                    grid.Children.Add(comboboxSubject);
                    StackPanel panelTeacher = new StackPanel();
                    ComboBox comboboxTeacher = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 0.0, 10.0, 5.0),
                        ItemsSource = listTeacher,
                        SelectedIndex = scheduleTemporary[DayID].Lessons[i - 1].SelectTeacherOne
                    };
                    comboboxTeacher.SelectionChanged += new SelectionChangedEventHandler(ChangedCombo);
                    comboboxTeachers1.Add(comboboxTeacher);
                    ComboBox comboboxTeacher2 = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 5.0, 10.0, 0.0),
                        ItemsSource = listTeacher,
                        SelectedIndex = scheduleTemporary[DayID].Lessons[i - 1].SelectTeacherTwo
                    };
                    comboboxTeacher2.SelectionChanged += new SelectionChangedEventHandler(ChangedCombo);
                    panelTeacher.Children.Add(comboboxTeacher);
                    panelTeacher.Children.Add(comboboxTeacher2);
                    comboboxTeachers2.Add(comboboxTeacher2);
                    Grid.SetColumn(panelTeacher, 2);
                    Grid.SetRow(panelTeacher, 0);
                    grid.Children.Add(panelTeacher);
                    StackPanel panelOffice = new StackPanel();
                    ComboBox comboboxOffice = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 0.0, 10.0, 5.0),
                        ItemsSource = listOffice,
                        SelectedIndex = scheduleTemporary[DayID].Lessons[i - 1].SelectOfficeOne
                    };
                    comboboxOffice.SelectionChanged += new SelectionChangedEventHandler(ChangedCombo);
                    comboboxOffices1.Add(comboboxOffice);
                    ComboBox comboboxOffice2 = new ComboBox
                    {
                        Height = 30.0,
                        Margin = new Thickness(10.0, 5.0, 10.0, 0.0),
                        ItemsSource = listOffice,
                        SelectedIndex = scheduleTemporary[DayID].Lessons[i - 1].SelectOfficeTwo
                    };
                    comboboxOffice2.SelectionChanged += new SelectionChangedEventHandler(ChangedCombo);
                    panelOffice.Children.Add(comboboxOffice);
                    panelOffice.Children.Add(comboboxOffice2);
                    comboboxOffices2.Add(comboboxOffice2);
                    Grid.SetColumn(panelOffice, 3);
                    Grid.SetRow(panelOffice, 0);
                    grid.Children.Add(panelOffice);
                    ScheduleList.Children.Add(grid);
                }
                connection.Close();
            }
        }
        private void ChangedCombo(object sender, EventArgs e)
        {
            MainButton.IsEnabled = true;
            int selectDay = ComboDay.SelectedIndex;
            for (int i = 0; i < 12; i++)
            {
                scheduleTemporary[selectDay].Lessons[i].SelectSubject = comboboxSubjects[i].SelectedIndex;
                scheduleTemporary[selectDay].Lessons[i].SelectTeacherOne = comboboxTeachers1[i].SelectedIndex;
                scheduleTemporary[selectDay].Lessons[i].SelectTeacherTwo = comboboxTeachers2[i].SelectedIndex;
                scheduleTemporary[selectDay].Lessons[i].SelectOfficeOne = comboboxOffices1[i].SelectedIndex;
                scheduleTemporary[selectDay].Lessons[i].SelectOfficeTwo = comboboxOffices2[i].SelectedIndex;
            }
        }
        private void ClosingWindow(object sender, EventArgs e)
        {
            bool isEnabled = MainButton.IsEnabled;
            if (isEnabled)
            {
                IsClosed = true;
                MyCanvas.Visibility = Visibility.Visible;
                CanvasMain.Content = "Выход";
                CanvasDescription.Text = "Вы действительно хотите отменить изменения?";
            }
            else
            {
                base.Close();
            }
        }
        private void SaveSchedule(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;
            CanvasMain.Content = "Сохранение";
            CanvasDescription.Text = "Вы действительно хотите сохранить расписание?";
        }
        private void ButtonYes(object sender, EventArgs e)
        {
            bool isClosed = IsClosed;
            if (isClosed)
            {
                base.Close();
            }
            else
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    for (int i = 0; i < 6; i++)
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            int SubjectID = 0;
                            int TeacherOneID = 0;
                            int TeacherTwoID = 0;
                            string SubjectName = listSubject[scheduleTemporary[i].Lessons[j].SelectSubject];
                            bool flag = SubjectName != "Нет";
                            SQLiteCommand command;
                            if (flag)
                            {
                                command = new SQLiteCommand("SELECT Number FROM Subject WHERE Name = @SubjectText", connection);
                                command.Parameters.AddWithValue("@SubjectText", SubjectName);
                                using (SQLiteDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        SubjectID = reader.GetInt32(0);
                                    }
                                }
                            }
                            else
                            {
                                SubjectID = 0;
                            }
                            string TeacherNameOne = listTeacher[scheduleTemporary[i].Lessons[j].SelectTeacherOne];
                            bool flag2 = TeacherNameOne != "Нет";
                            if (flag2)
                            {
                                command = new SQLiteCommand("SELECT Number FROM Teacher WHERE Surname = @surname AND Name = @name", connection);
                                command.Parameters.AddWithValue("@surname", TeacherNameOne.Split(new char[]
                                {
                                    ' '
                                })[0]);
                                command.Parameters.AddWithValue("@name", TeacherNameOne.Split(new char[]
                                {
                                    ' '
                                })[1]);
                                using (SQLiteDataReader reader2 = command.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        TeacherOneID = reader2.GetInt32(0);
                                    }
                                }
                            }
                            else
                            {
                                TeacherOneID = 0;
                            }
                            string TeacherNameTwo = listTeacher[scheduleTemporary[i].Lessons[j].SelectTeacherTwo];
                            bool flag3 = TeacherNameTwo != "Нет";
                            if (flag3)
                            {
                                command = new SQLiteCommand("SELECT Number FROM Teacher WHERE Surname = @surname AND Name = @name", connection);
                                command.Parameters.AddWithValue("@surname", TeacherNameTwo.Split(new char[]
                                {
                                    ' '
                                })[0]);
                                command.Parameters.AddWithValue("@name", TeacherNameTwo.Split(new char[]
                                {
                                    ' '
                                })[1]);
                                using (SQLiteDataReader reader3 = command.ExecuteReader())
                                {
                                    while (reader3.Read())
                                    {
                                        TeacherTwoID = reader3.GetInt32(0);
                                    }
                                }
                            }
                            else
                            {
                                TeacherTwoID = 0;
                            }
                            schedule[i].Lessons[j].SubjectID = SubjectID;
                            schedule[i].Lessons[j].TeacherOneID = TeacherOneID;
                            schedule[i].Lessons[j].TeacherTwoID = TeacherTwoID;
                            schedule[i].Lessons[j].OfficeOne = listOffice[scheduleTemporary[i].Lessons[j].SelectOfficeOne];
                            schedule[i].Lessons[j].OfficeTwo = listOffice[scheduleTemporary[i].Lessons[j].SelectOfficeTwo];
                            command = new SQLiteCommand("UPDATE SchedulePermanent SET List = @ScheduleList WHERE GroupID = @id", connection);
                            string json = JsonConvert.SerializeObject(schedule);
                            command.Parameters.AddWithValue("@ScheduleList", json);
                            command.Parameters.AddWithValue("@id", id);
                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
                base.DialogResult = new bool?(true);
                base.Close();
            }

        }
        private void ButtonNo(object sender, EventArgs e)
        {
            bool isClosed = IsClosed;
            if (isClosed)
            {
                IsClosed = false;
            }
            MyCanvas.Visibility = Visibility.Collapsed;
        }
        private class DayWeekTemporary
        {
            public List<LessonTemporary> Lessons = new List<LessonTemporary>();
        }
        private class DayWeek
        { 
            public List<Lesson> Lessons { get; set; }
        }

        private class Lesson
        {
            public int SubjectID { get; set; }
            public int TeacherOneID { get; set; }
            public int TeacherTwoID { get; set; }
            public string OfficeOne { get; set; }
            public string OfficeTwo { get; set; }
        }
        private class LessonTemporary
        {
            public int SelectSubject { get; set; }
            public int SelectTeacherOne { get; set; }
            public int SelectTeacherTwo { get; set; }
            public int SelectOfficeOne { get; set; }
            public int SelectOfficeTwo { get; set; }
        }
    }
}

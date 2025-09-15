using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace AdminPanel
{
    public partial class AddEditGroup : Window
    {
        bool IsClosed = false;
        int type = 0, id = 0;
        private string connectingString = App.ConnectionString;
        public AddEditGroup(int type, int id)
        {
            this.type = type;
            this.id = id;
            InitializeComponent();
            if (type == 0)
            {
                Title = "Добавление";
                MainLabal.Content = "Добавление группы";
                MainButton.Content = "Добавить";
            }
            else if (type == 1)
            {
                Title = "Изменение";
                MainLabal.Content = "Изменение группы";
                MainButton.Content = "Изменить";
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"SELECT Name FROM GroupList WHERE Number = @id", connection);
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) GroupEdit.Text = reader.GetString(0);
                    }
                    connection.Close();
                }
                MainButton.IsEnabled = false;
            }
        }
        private void ClosingWindow(object sender, EventArgs e)
        {
            if (MainButton.IsEnabled && type == 1)
            {
                IsClosed = true;
                MyCanvas.Visibility = Visibility.Visible;
                CanvasMain.Content = "Выход";
                CanvasDescription.Text = "Вы действительно хотите отменить изменения?";
            }
            else Close();
        }
        private void SaveGroup(object sender, EventArgs e)
        {
            GroupError.Visibility = Visibility.Hidden;
            if (GroupEdit.Text != "")
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"SELECT COUNT(*) FROM GroupList WHERE Name = @GroupText AND Number != @id", connection);
                    command.Parameters.AddWithValue("@GroupText", GroupEdit.Text);
                    command.Parameters.AddWithValue("@id", id);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        GroupError.Content = "Данная группа уже есть!";
                        GroupError.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MyCanvas.Visibility = Visibility.Visible;
                        if (type == 0)
                        {
                            CanvasMain.Content = "Добавление";
                            CanvasDescription.Text = "Вы действительно хотите добавить группу?";
                        }
                        else if (type == 1)
                        {
                            CanvasMain.Content = "Изменение";
                            CanvasDescription.Text = "Вы действительно хотите изменить группу?";
                        }
                    }
                    connection.Close();
                }
            }
            else
            {
                GroupError.Content = "Пустое поле!";
                GroupError.Visibility = Visibility.Visible;
            }
        }
        private void ButtonYes(object sender, EventArgs e)
        {
            if (type == 0)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"INSERT INTO GroupList (Name) VALUES (@GroupText)", connection);
                    command.Parameters.AddWithValue("@GroupText", GroupEdit.Text);
                    command.ExecuteNonQuery();
                    command = new SQLiteCommand("SELECT Number FROM GroupList WHERE Name = @GroupText", connection);
                    command.Parameters.AddWithValue("@GroupText", GroupEdit.Text);
                    int GroupID=0;
                    using(var reader = command.ExecuteReader()) while(reader.Read()) GroupID = reader.GetInt32(0);
                    command = new SQLiteCommand("INSERT INTO SchedulePermanent (GroupID, List) VALUES (@id,'[]')", connection);
                    command.Parameters.AddWithValue("@id", GroupID);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                this.DialogResult = true;
                this.Close();
            }
            else if (type == 1)
            {
                if (IsClosed) this.Close();
                else
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"UPDATE GroupList SET Name = @GroupText WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@GroupText", GroupEdit.Text);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }
        private void ButtonNo(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Collapsed;
            if (IsClosed) IsClosed = false;
        }
        private void TextedChanged(object sender, EventArgs e)
        {
            if (type == 1) MainButton.IsEnabled = true;
        }
    }
}

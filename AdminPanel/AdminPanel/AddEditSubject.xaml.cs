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
    public partial class AddEditSubject : Window
    {
        bool IsClosed = false;
        int type = 0, id = 0;
        private string connectingString = App.ConnectionString;
        public AddEditSubject(int type, int id)
        {
            this.type = type;
            this.id = id;
            InitializeComponent();
            if (type == 0)
            {
                Title = "Добавление";
                MainLabal.Content = "Добавление предмета";
                MainButton.Content = "Добавить";
            }
            else if (type == 1)
            {
                Title = "Изменение";
                MainLabal.Content = "Изменение предмета";
                MainButton.Content = "Изменить";
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"SELECT Name FROM Subject WHERE Number = @id", connection);
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) SubjectEdit.Text = reader.GetString(0);
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
        private void SaveSubject(object sender, EventArgs e)
        {
            SubjectError.Visibility = Visibility.Hidden;
            if (SubjectEdit.Text != "")
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"SELECT COUNT(*) FROM Subject WHERE Name = @SubjectText AND Number != @id", connection);
                    command.Parameters.AddWithValue("@SubjectText", SubjectEdit.Text);
                    command.Parameters.AddWithValue("@id", id);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        SubjectError.Content = "Данный предмет уже есть!";
                        SubjectError.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MyCanvas.Visibility = Visibility.Visible;
                        if (type == 0)
                        {
                            CanvasMain.Content = "Добавление";
                            CanvasDescription.Text = "Вы действительно хотите добавить предмет?";
                        }
                        else if (type == 1)
                        {
                            CanvasMain.Content = "Изменение";
                            CanvasDescription.Text = "Вы действительно хотите изменить предмет?";
                        }
                    }
                    connection.Close();
                }
            }
            else
            {
                SubjectError.Content = "Пустое поле!";
                SubjectError.Visibility = Visibility.Visible;
            }
        }
        private void ButtonYes(object sender, EventArgs e)
        {
            if (type == 0)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"INSERT INTO Subject (Name) VALUES (@SubjectText)", connection);
                    command.Parameters.AddWithValue("@SubjectText", SubjectEdit.Text);
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
                        SQLiteCommand command = new SQLiteCommand($"UPDATE Subject SET Name = @SubjectText WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@SubjectText", SubjectEdit.Text);
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

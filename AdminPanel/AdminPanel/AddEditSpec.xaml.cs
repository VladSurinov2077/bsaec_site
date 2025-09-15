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
using System.Xml.Linq;

namespace AdminPanel
{
    public partial class AddEditSpec : Window
    {
        private string connectingString = App.ConnectionString;
        int type = 0, id = 0;
        bool IsClosed = false;
        public AddEditSpec(int type, int id)
        {
            this.type = type;
            this.id = id;
            InitializeComponent();
            if (type == 0)
            {
                Title = "Добавление";
                MainLabal.Content = "Добавление специальности";
                MainButton.Content = "Добавить";
            }
            else if (type == 1)
            {
                Title = "Изменение";
                MainLabal.Content = "Изменение специальности";
                MainButton.Content = "Изменить";
                using(SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"SELECT * FROM Specialties WHERE Number = @id", connection);
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            SpecEdit.Text = reader.GetString(1);
                            if(reader.GetInt32(2)==0) CheckBox1.IsChecked = false;
                            else CheckBox1.IsChecked = true;
                            if (reader.GetInt32(3) == 0) CheckBox2.IsChecked = false;
                            else CheckBox2.IsChecked = true;
                        }
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
        private void SaveSpec(object sender, EventArgs e)
        {
            SpecError.Visibility = Visibility.Hidden;
            CheckError.Visibility = Visibility.Hidden;
            if(SpecEdit.Text!="" && (CheckBox1.IsChecked==true || CheckBox2.IsChecked==true))
            {
                using(SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"SELECT COUNT(*) FROM Specialties WHERE Name = @SpecText AND Number != @id", connection);
                    command.Parameters.AddWithValue("@SpecText", SpecEdit.Text);
                    command.Parameters.AddWithValue("@id", id);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if(count>0)
                    {
                        SpecError.Visibility= Visibility.Visible;
                        SpecError.Content = "Данная специальность уже есть!";
                    } else
                    {
                        MyCanvas.Visibility = Visibility.Visible;
                        if (type == 0)
                        {
                            CanvasMain.Content = "Добавление";
                            CanvasDescription.Text = "Вы действительно хотите добавить специальность?";
                        }
                        else if (type == 1)
                        {
                            CanvasMain.Content = "Изменение";
                            CanvasDescription.Text = "Вы действительно хотите изменить специальность?";
                        }
                    }
                    connection.Close();
                }
            }
            else
            {
                if (SpecEdit.Text == "")
                {
                    SpecError.Visibility = Visibility.Visible;
                    SpecError.Content = "Пустое поле!";
                }
                if (CheckBox1.IsChecked == false && CheckBox2.IsChecked == false)
                {
                    CheckError.Visibility = Visibility.Visible;
                    CheckError.Content = "Выберите хотя бы один из двух вариантов!";
                }
            }
        }
        private void ButtonYes(object sender, EventArgs e)
        {
            if(type==0)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    int nine = CheckBox1.IsChecked.GetValueOrDefault() ? 1 : 0;
                    int eleven = CheckBox2.IsChecked.GetValueOrDefault() ? 1 : 0;
                    SQLiteCommand command = new SQLiteCommand($"INSERT INTO Specialties (Name, IsNineClass, IsElevenClass) VALUES (@Name, @NineClass, @ElevenClass)", connection);
                    command.Parameters.AddWithValue("@Name", SpecEdit.Text);
                    command.Parameters.AddWithValue("@NineClass", nine);
                    command.Parameters.AddWithValue("@ElevenClass", eleven);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                this.DialogResult = true;
                this.Close();
            } else if(type==1)
            {
                if (IsClosed) this.Close();
                else
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        int nine = CheckBox1.IsChecked.GetValueOrDefault() ? 1 : 0;
                        int eleven = CheckBox2.IsChecked.GetValueOrDefault() ? 1 : 0;
                        SQLiteCommand command = new SQLiteCommand($"UPDATE Specialties SET Name = @SpecText, IsNineClass = @NineClass, IsElevenClass = @ElevenClass WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@SpecText", SpecEdit.Text);
                        command.Parameters.AddWithValue("@NineClass", nine);
                        command.Parameters.AddWithValue("@ElevenClass", eleven);
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
            if(IsClosed) IsClosed = true;
        }
        private void CheckChange(object sender, EventArgs e)
        {
            if(type==1) MainButton.IsEnabled = true;
        }
        private void TextedChanged(object sender, EventArgs e)
        {
            if (type == 1) MainButton.IsEnabled = true;
        }
    }
}

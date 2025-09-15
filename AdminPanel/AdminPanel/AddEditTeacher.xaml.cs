using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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
using Microsoft.Win32;

namespace AdminPanel
{
    public partial class AddEditTeacher : Window
    {
        int type = 0; int id = 0;
        bool IsDelete = false;
        byte[] _imageData;
        bool IsClosed = false;
        private string connectingString = App.ConnectionString;
        public AddEditTeacher(int type, int id)
        {
            InitializeComponent();
            this.type = type;
            this.id = id;
            List<string> combo1 = new List<string>
            {
                "мастер произв. обуч.",
                "преподаватель"
            };
            List<string> combo2 = new List<string>
            {
                "без",
                "первой",
                "второй",
                "высшей"
            };
            ComboBox1.ItemsSource = combo1;
            ComboBox2.ItemsSource = combo2;
            if (type == 0)
            {
                Title = "Добавление";
                MainLabal.Content = "Добавление пользователя";
                TeacherDelete.Visibility = Visibility.Collapsed;
                MainButton.Content = "Добавить";
                ButtonDel.IsEnabled = false;
                ComboBox1.SelectedIndex = 0;
                ComboBox2.SelectedIndex = 0;
            }
            else
            {
                Title = "Изменение";
                MainLabal.Content = "Изменение пользователя";
                MainButton.Content = "Изменить";
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand($"SELECT * FROM Teacher WHERE Number = @id", connection);
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _imageData = (byte[])reader[6];
                            ImageBox.Source = ConvertByteArrayToImage(_imageData);
                            SurnameEdit.Text = reader.GetString(1);
                            NameEdit.Text = reader.GetString(2);
                            PatronymicEdit.Text = reader.GetString(3);
                            ComboBox1.SelectedIndex = reader.GetInt32(4);
                            ComboBox2.SelectedIndex = reader.GetInt32(5);
                            MainButton.IsEnabled = false;
                        }
                    }
                    connection.Close();
                }
            }
        }
        private void ClosingWindow(object sender, EventArgs e)
        {
            if(MainButton.IsEnabled && type==1)
            {
                IsClosed = true;
                MyCanvas.Visibility = Visibility.Visible;
                CanvasMain.Content = "Выход";
                CanvasDescription.Text = "Вы действительно хотите отменить изменения?";
            } else Close();
        }
        private void AddImage(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                ImageBox.Source = bitmap;
                _imageData = File.ReadAllBytes(openFileDialog.FileName);
                ButtonDel.IsEnabled = true;
                if(type==1) MainButton.IsEnabled = true;
            }
        }
        private void DeleteImage(object sender, EventArgs e)
        {
            ImageBox.Source = new BitmapImage(new Uri("pack://application:,,,/Image/not_picture.png"));
            ButtonDel.IsEnabled = false;
            if (type == 1) MainButton.IsEnabled = true;
        }
        private void SaveTeacher(object sender, EventArgs e)
        {
            if (SurnameEdit.Text != "" && NameEdit.Text != "" && PatronymicEdit.Text != "")
            {
                MyCanvas.Visibility = Visibility.Visible;
                if(type == 0)
                {
                    CanvasMain.Content = "Добавление";
                    CanvasDescription.Text = "Вы действительно хотите добавить преподователя?";
                } else
                {
                    CanvasMain.Content = "Изменение";
                    CanvasDescription.Text = "Вы действительно хотите изменить преподователя?";
                }
            }
            else
            {
                if (SurnameEdit.Text == "") SurnameError.Visibility = Visibility.Visible;
                else SurnameError.Visibility = Visibility.Hidden;
                if (NameEdit.Text == "") NameError.Visibility = Visibility.Visible;
                else NameError.Visibility = Visibility.Hidden;
                if (PatronymicEdit.Text == "") PatronymicError.Visibility = Visibility.Visible;
                else PatronymicError.Visibility = Visibility.Hidden;
            }
        }
        private void ButtonNo(object sender, EventArgs e)
        {
            MyCanvas.Visibility = Visibility.Collapsed;
            if(IsDelete) IsDelete = false;
            if(IsClosed) IsClosed = false;
        }
        private void ButtonYes(object sender, EventArgs e)
        {
            if(type==0)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                {
                    connection.Open();
                    _imageData = ImageSourceToByteArray(ImageBox.Source);
                    SQLiteCommand command = new SQLiteCommand($"INSERT INTO Teacher (Surname, Name, Patronymic, Category1, Category2, ImgData) VALUES (@Surname, @Name, @Patronymic, @Category1, @Category2,@ImageData)", connection);
                    command.Parameters.AddWithValue("@Surname", SurnameEdit.Text);
                    command.Parameters.AddWithValue("@Name", NameEdit.Text);
                    command.Parameters.AddWithValue("@Patronymic", PatronymicEdit.Text);
                    command.Parameters.AddWithValue("@Category1", ComboBox1.SelectedIndex);
                    command.Parameters.AddWithValue("@Category2", ComboBox2.SelectedIndex);
                    command.Parameters.AddWithValue("@ImageData", _imageData);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                this.DialogResult = true;
                this.Close();
            } else if(type==1)
            {
                if (IsDelete)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand($"DELETE FROM Teacher WHERE Number = @id", connection);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    if (IsClosed)
                    {
                        this.Close();
                    }
                    else
                    {
                        using (SQLiteConnection connection = new SQLiteConnection(connectingString))
                        {
                            connection.Open();
                            _imageData = ImageSourceToByteArray(ImageBox.Source);
                            SQLiteCommand command = new SQLiteCommand($"UPDATE Teacher SET Surname = @Surname, Name = @Name, Patronymic = @Patronymic, Category1 = @Category1, Category2 = @Category2, ImgData = @ImageData WHERE Number = @id", connection);
                            command.Parameters.AddWithValue("@Surname", SurnameEdit.Text);
                            command.Parameters.AddWithValue("@Name", NameEdit.Text);
                            command.Parameters.AddWithValue("@Patronymic", PatronymicEdit.Text);
                            command.Parameters.AddWithValue("@Category1", ComboBox1.SelectedIndex);
                            command.Parameters.AddWithValue("@Category2", ComboBox1.SelectedIndex);
                            command.Parameters.AddWithValue("@ImageData", _imageData);
                            command.Parameters.AddWithValue("@id", id);
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        this.DialogResult = true;
                        this.Close();
                    }
                }
            }
        }
        private void TextedChanged(object sender, EventArgs e)
        {
            if(type==1) MainButton.IsEnabled = true;
        }
        private void ComboBoxChanged(object sender, EventArgs e)
        {
            if (type == 1) MainButton.IsEnabled = true;
        }
        private void DeleteElement(object sender, EventArgs e)
        {
            IsDelete = true;
            MyCanvas.Visibility = Visibility.Visible;
            CanvasMain.Content = "Удаление";
            CanvasDescription.Text = "Вы действительно хотите удалить преподователя?";
        }
        // Перевод из ImageSource в byte
        private byte[] ImageSourceToByteArray(ImageSource imageSource)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            throw new InvalidOperationException("ImageSource не является BitmapSource.");
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
    }
}

using Production.Services;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Production
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _db;
        public MainWindow(string email)
        {
            _db = new DatabaseService();
            InitializeComponent();
           
            GetUser(email);
        }

        public void GetUser(string email)
        {
            var user = _db.GetUserByEmail(email);
            NameBlock.Text = user["Фамилия"] + " " + user["Имя"] + " " + user["Отчество"];
            LoginBox.Text = user["Логин"].ToString();
        }

        public void RefreshGrid(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.Trim();
            RefreshData(filter);
        }

        public void RefreshData(string filter = "")
        {
            DataView view = _db.GetData().DefaultView;
            if (!string.IsNullOrEmpty(filter))
            {
                view.RowFilter = $"Наименование LIKE '%{filter.Replace("'", "''")}%' OR Описание LIKE '%{filter.Replace("'", "''")}%' OR [Единица Измерения] LIKE '%{filter.Replace("'", "''")}%' OR [Поставщик] LIKE '%{filter.Replace("'", "''")}%' OR [Производитель] LIKE '%{filter.Replace("'", "''")}%' OR [Категория] LIKE '%{filter.Replace("'", "''")}%' OR Цена_Итог LIKE '%{filter.Replace("'", "''")}%' OR Статус LIKE '%{filter.Replace("'", "''")}%'";
            }
            ProdGrid.ItemsSource = view;
        }

        private void ProdGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProdGrid.SelectedItem is DataRowView row)
            {
                var fileName = row["Путь_Фото"]?.ToString();
                
                string imagePath = !string.IsNullOrEmpty(fileName) ? $"Images/{fileName}" : "Images/picture.png";
                try
                {
                    var uri = new Uri($"pack://application:,,,/{imagePath}", UriKind.Absolute);

                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Загружаем изображение сразу
                    bitmap.EndInit();

                    ProdPicture.Source = bitmap;
                }
                catch
                {
                    ProdPicture.Source = null; // Если изображение не найдено, очищаем
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var auth = new AuthWindow();
            auth.Show();
            this.Close();
        }
    }
}
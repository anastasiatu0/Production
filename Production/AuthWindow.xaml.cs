using Production.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Production
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private readonly DatabaseService _db; // здесь ебутся клоуны(я и андрей)
        public AuthWindow()
        {
            InitializeComponent();
            _db = new DatabaseService();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailInput.Text.Trim().ToLower();
            string password = PasswordInput.Password;

            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) )
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool success = _db.authUser(email, password);
                if (success)
                {
                    MessageBox.Show("Авторизация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    var main = new MainWindow(email);
                    main.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль. Пожалуйста, попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при попытке авторизации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

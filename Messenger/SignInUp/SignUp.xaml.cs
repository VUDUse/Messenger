using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Messenger.DB;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для SignUp.xaml
    /// </summary>
    public partial class SignUp : Page
    {
        private AuthService authService = new AuthService();

        public static string host = Dns.GetHostName();
        public static IPAddress[] address = Dns.GetHostAddresses(host);

        bool success = true;

        public SignUp()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SignIn());
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            success = true;

            if (PasswordSignUp.Password != PasswordProofSignUp.Password)
            {
                Passwordconfirm.Content = "PASSWORDCONFIRM - Passwords do not match";
                Passwordconfirm.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");

                Password.Content = "PASSWORD - Passwords do not match";
                Password.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");

                PasswordSignUp.Password = null;
                PasswordProofSignUp.Password = null;
                success = false;
            }

            if (NicknameSignUp.Text.Length < 1)
            {
                NicknameSignUp.Text = null;
                Nickname.Content = "NICKNAME - field cannot be empty";
                Nickname.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                success = false;
            }

            if (LoginSignUp.Text.Length < 6 || LoginSignUp.Text.Length > 24)
            {
                LoginSignUp.Text = null;
                Login.Content = "LOGIN - less than 6 or more than 24";
                Login.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                success = false;
            }

            if (NicknameSignUp.Text.Length > 16)
            {
                Nickname.Content = "NICKNAME - no more than 16";
                Nickname.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                success = false;
            }

            if (PasswordSignUp.Password.Length < 8 || PasswordSignUp.Password.Length > 24)
            {
                PasswordSignUp.Password = null;
                Password.Content = "PASSWORD - less than 8 or more than 24";
                Password.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                success = false;
            }

            if (PasswordProofSignUp.Password.Length < 8 || PasswordSignUp.Password.Length > 24)
            {
                PasswordProofSignUp.Password = null;
                Passwordconfirm.Content = "PASSWORD CONFIRM - less than 8 or more than 24";
                Passwordconfirm.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                success = false;
            }

            using (var db = new ApplicationContext())
            {
                // Проверяем уникальность логина
                if (db.Users.Any(u => u.Login == LoginSignUp.Text))
                {
                    LoginSignUp.Text = null;
                    Login.Content = "LOGIN - this login already exists";
                    Login.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                    success = false;
                }

                if (success)
                {
                    Users user = new Users
                    {
                        // Id назначается базой автоматически (identity PK)
                        Nickname = NicknameSignUp.Text,
                        Login = LoginSignUp.Text,
                        Password = BCrypt.Net.BCrypt.HashPassword(PasswordSignUp.Password), // хешируем пароль
                        IP = address[4].ToString(),
                        AuthToken = Guid.NewGuid().ToString() // отдельный случайный токен, не совпадает с Id
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    PasswordSignUp.Password = null;
                    PasswordProofSignUp.Password = null;
                    LoginSignUp.Text = null;
                    NicknameSignUp.Text = null;

                    // Сохраняем токен в реестре
                    authService.SaveToken(user.AuthToken);

                    DataBank.UserLog = user.Nickname; // Сохраняем никнейм

                    MainMenu mainMenu = new MainMenu();
                    mainMenu.Show();
                    mainMenu.RefreshChatList(); // Обновляем список чатов
                    Application.Current.MainWindow.Close();
                }
            }
        }
    }
}

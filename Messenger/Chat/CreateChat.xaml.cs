using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Messenger.DB;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для CreateChat.xaml
    /// </summary>
    public partial class CreateChat : Page
    {
        public CreateChat()
        {
            InitializeComponent();
        }

        private void OpenCreateChat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OpenChat());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close(); // Закрыть текущее окно
        }

        private void CreateChatButton_Click(object sender, RoutedEventArgs e)
        {
            bool succsess = true; // Сбрасываем флаг успешности перед каждой попыткой

            if (string.IsNullOrWhiteSpace(CreateChatName.Text))
            {
                Chatname.Content = "CHAT NAME - field cannot be empty";
                Chatname.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                succsess = false;
            }
            else if (IsChatNameTaken(CreateChatName.Text))
            {
                CreateChatName.Text = null;
                Chatname.Content = "CHAT NAME - this chat name already exists";
                Chatname.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
                succsess = false;
            }

            if (succsess)
            {
                CreateNewChatRoom();
                ResetForm();
                Window.GetWindow(this).Close();
            }
        }

        private bool IsChatNameTaken(string chatName)
        {
            using (var db = new ApplicationContext())
            {
                return db.ChatRooms.Any(c => c.Login == chatName);
            }
        }

        private void CreateNewChatRoom()
        {
            using (var db = new ApplicationContext())
            {
                ChatRooms chatRoom = new ChatRooms
                {
                    Login = CreateChatName.Text
                    // Id назначается базой автоматически (identity PK).
                    // Пароля больше нет — вступление в чат/группу временно происходит по названию,
                    // до внедрения полноценных приглашений (см. следующую фазу).
                };

                db.ChatRooms.Add(chatRoom);
                db.SaveChanges(); // После SaveChanges chatRoom.Id уже заполнен базой

                // Обновляем список чатов в главном меню
                var mainMenu = Application.Current.Windows.OfType<MainMenu>().FirstOrDefault();
                if (mainMenu != null)
                {
                    mainMenu.UpdateChatList(chatRoom.Id);
                }
            }
        }

        private void ResetForm()
        {
            CreateChatName.Text = null;
        }
    }
}

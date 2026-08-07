using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Messenger.DB;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для OpenChat.xaml
    /// </summary>
    public partial class OpenChat : Page
    {
        public OpenChat()
        {
            InitializeComponent();
        }

        private void OpenCreateChat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateChat());
        }

        private void ChatOpen_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ApplicationContext())
            {
                // Пароля у чатов больше нет. Пока вступление происходит по названию чата —
                // это временное решение до внедрения приглашений (следующая фаза).
                var chatRoom = db.ChatRooms.FirstOrDefault(cr => cr.Login == OpenChatLogin.Text);

                if (chatRoom != null)
                {
                    DataBank.RoomID = chatRoom.Id;

                    OpenChatLogin.Text = null;

                    MainMenu mainMenu = Application.Current.Windows.OfType<MainMenu>().FirstOrDefault();
                    if (mainMenu == null)
                    {
                        mainMenu = new MainMenu();
                        mainMenu.Show();
                        Application.Current.MainWindow.Close();
                        Application.Current.MainWindow = mainMenu;
                    }
                    mainMenu.UpdateChatList(chatRoom.Id);
                    mainMenu.RefreshChatList(); // Обновляем список чатов
                    Window.GetWindow(this).Close();

                    return;
                }

                Chat.Content = "CHAT NAME - chat not found";
                Chat.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C22F1F");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close(); // Закрыть текущее окно
        }
    }
}

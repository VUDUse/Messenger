using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Messenger.DB;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        private int roomId;
        private DispatcherTimer chatUpdateTimer;

        public ChatPage(int roomId, MainMenu mainMenu)
        {
            InitializeComponent();
            this.roomId = roomId;
            ChatUpdate();
            ScrollToBottom(); // Прокрутка к низу при открытии чата

            // Подписка на событие удаления чата
            mainMenu.ChatDeleted += MainMenu_ChatDeleted;

            // Инициализация таймера для обновления чата
            chatUpdateTimer = new DispatcherTimer();
            chatUpdateTimer.Interval = TimeSpan.FromSeconds(1); // Интервал обновления чата
            chatUpdateTimer.Tick += ChatUpdateTimer_Tick;
            chatUpdateTimer.Start();
        }

        private void MainMenu_ChatDeleted(int deletedRoomId)
        {
            if (deletedRoomId == roomId)
            {
                Dispatcher.Invoke(() =>
                {
                    // Заменяем содержимое фрейма на пустую страницу
                    NavigationService?.Navigate(new Page());
                });
            }
        }

        private void ChatUpdateTimer_Tick(object sender, EventArgs e)
        {
            ChatUpdate();
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            string messageText = Message.Text;

            if (string.IsNullOrWhiteSpace(messageText))
            {
                return; // Не отправлять пустые сообщения
            }

            using (var db = new ApplicationContext())
            {
                var chatRoom = db.ChatRooms.FirstOrDefault(cr => cr.Id == roomId);
                if (chatRoom != null)
                {
                    if (chatRoom.ChatHistory == null)
                    {
                        chatRoom.ChatHistory = "";
                    }

                    chatRoom.ChatHistory += DataBank.UserLog + ": " + messageText + "\r\n"; // Добавляет сообщение к истории чата в БД
                    db.SaveChanges(); // Сохраняет изменения в БД
                    AddMessageToChat(DataBank.UserLog, messageText); // Добавляет сообщение в UI
                    Message.Text = null; // Очищает текстовое поле для ввода сообщений
                    ScrollToBottom(); // Прокрутка к низу после добавления нового сообщения
                }
            }
        }

        private void AddMessageToChat(string user, string message)
        {
            if (ChatStackPanel == null)
            {
                return;
            }

            TextBlock messageBlock = new TextBlock
            {
                Text = $"{user}: {message}",
                TextWrapping = System.Windows.TextWrapping.Wrap,
                FontSize = 16,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new System.Windows.Thickness(0, 5, 0, 5)
            };
            ChatStackPanel.Children.Add(messageBlock); // Добавление сообщения в StackPanel
        }

        public void ChatUpdate()
        {
            if (!string.IsNullOrEmpty(DataBank.UserLog))
            {
                using (var db = new ApplicationContext())
                {
                    var chatRoom = db.ChatRooms.FirstOrDefault(cr => cr.Id == roomId);
                    if (chatRoom != null)
                    {
                        ChatStackPanel?.Children.Clear(); // Очищает текущие сообщения в StackPanel

                        if (!string.IsNullOrEmpty(chatRoom.ChatHistory))
                        {
                            foreach (var message in chatRoom.ChatHistory.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var parts = message.Split(new[] { ": " }, 2, StringSplitOptions.None);
                                if (parts.Length == 2)
                                {
                                    AddMessageToChat(parts[0], parts[1]); // Добавляет каждое сообщение в StackPanel
                                }
                            }
                        }
                        ScrollToBottom(); // Прокрутка к низу после обновления чата
                    }
                }
            }
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer?.ScrollToEnd(); // Прокрутка ScrollViewer к концу
        }

        private void Message_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Enter_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SendMessage();
        }
    }
}

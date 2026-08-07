using Messenger.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        private Dictionary<string, int> chatLoginToRoomIdMap = new Dictionary<string, int>();
        private List<string> allChats = new List<string>(); // Список всех чатов
        private AuthService authService = new AuthService();

        public event Action<int> ChatDeleted; // Событие для удаления чата

        public MainMenu()
        {
            InitializeComponent();
            LoadChatList();
            ChatListBox.SelectionChanged += ChatListBox_SelectionChanged;

            // Добавление обработчика события для контекстного меню
            ChatListBox.ContextMenu = CreateChatListContextMenu();
        }

        private ContextMenu CreateChatListContextMenu()
        {
            var contextMenu = new ContextMenu();
            var deleteMenuItem = new MenuItem { Header = "Delete Chat" };
            deleteMenuItem.Click += DeleteChat_Click;
            contextMenu.Items.Add(deleteMenuItem);
            return contextMenu;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainChat mainChat = new MainChat();
            mainChat.Show();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            Name.Content = DataBank.UserLog; // Убедитесь, что никнейм отображается
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SettingsMenu settingsMenu = new SettingsMenu();
            settingsMenu.Show();
        }

        public void UpdateChatList(int roomID)
        {
            using (var db = new ApplicationContext())
            {
                var chatRoom = db.ChatRooms.FirstOrDefault(cr => cr.Id == roomID);

                if (chatRoom != null)
                {
                    string chatItem = chatRoom.Login;
                    if (!allChats.Contains(chatItem))
                    {
                        allChats.Add(chatItem);
                        chatLoginToRoomIdMap[chatItem] = roomID; // Сохранение соответствия между логином и RoomID
                        FilterChatList(Search.Text); // Обновление отображаемого списка с учетом поиска
                    }
                }
            }
        }

        public void RefreshChatList()
        {
            if (ChatListBox.Dispatcher.CheckAccess())
            {
                LoadChatList();
            }
            else
            {
                ChatListBox.Dispatcher.Invoke(LoadChatList);
            }
        }

        private void LoadChatList()
        {
            ChatListBox.Items.Clear();
            chatLoginToRoomIdMap.Clear();
            allChats.Clear();

            using (var db = new ApplicationContext())
            {
                var chatRooms = db.ChatRooms.ToList();
                foreach (var chatRoom in chatRooms)
                {
                    string chatItem = chatRoom.Login;
                    if (!allChats.Contains(chatItem))
                    {
                        allChats.Add(chatItem);
                        chatLoginToRoomIdMap[chatItem] = chatRoom.Id; // Сохранение соответствия между логином и RoomID
                    }
                }
            }
            FilterChatList(Search.Text); // Обновление отображаемого списка с учетом поиска
        }

        private void FilterChatList(string searchText)
        {
            ChatListBox.Items.Clear();
            foreach (var chat in allChats)
            {
                if (chat.ToLower().Contains(searchText.ToLower()))
                {
                    ChatListBox.Items.Add(chat);
                }
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterChatList(Search.Text);
        }

        private void ChatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListBox.SelectedItem != null)
            {
                string selectedChat = ChatListBox.SelectedItem.ToString();
                if (chatLoginToRoomIdMap.TryGetValue(selectedChat, out int roomId))
                {
                    UserFrame.Navigate(new ChatPage(roomId, this));
                }
                else
                {
                    MessageBox.Show("Unable to find the corresponding Room ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DeleteChat_Click(object sender, RoutedEventArgs e)
        {
            if (ChatListBox.SelectedItem != null)
            {
                string selectedChat = ChatListBox.SelectedItem.ToString();
                if (chatLoginToRoomIdMap.TryGetValue(selectedChat, out int roomId))
                {
                    // Удаляем чат только из списка и словаря
                    chatLoginToRoomIdMap.Remove(selectedChat);
                    allChats.Remove(selectedChat);
                    ChatListBox.Items.Remove(selectedChat);

                    // Вызываем событие удаления чата
                    ChatDeleted?.Invoke(roomId);

                    MessageBox.Show($"Chat with {selectedChat} removed from the list.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a chat to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

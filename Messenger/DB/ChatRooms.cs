using System.ComponentModel.DataAnnotations;

namespace Messenger.DB
{
    public class ChatRooms
    {
        [Key]
        public int Id { get; set; } // Раньше было RoomID (случайная строка) — теперь нормальный identity PK

        public string Login { get; set; } // Название чата/группы
        public string? ChatHistory { get; set; }
        public string? LastMessage { get; set; }

        // Password удалён: вступление в чат больше не защищается общим паролем.
        // В следующей фазе для добавления в группу будет использоваться приглашение.
    }
}

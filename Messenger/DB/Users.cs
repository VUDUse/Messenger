using System.ComponentModel.DataAnnotations;

namespace Messenger.DB
{
    public class Users
    {
        [Key]
        public int Id { get; set; } // Раньше было ID (случайная строка вида 0000-0000-0000-0000) — теперь identity PK

        public string Login { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; } // Хранится хеш пароля (BCrypt), а не сам пароль
        public string? IP { get; set; }
        public string AuthToken { get; set; } // Отдельный случайный токен, больше не совпадает с Id
    }
}

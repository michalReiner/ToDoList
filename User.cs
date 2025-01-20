public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; } // נוודא לשמור את הסיסמה לאחר חישוב hash
}

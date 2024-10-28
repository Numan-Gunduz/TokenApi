namespace TokenApi.Models
{
    public class User
    {

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Normalde hashlenmiş şekilde saklanmalıdır
        //public string PasswordConfirm { get; set; }
    }
}

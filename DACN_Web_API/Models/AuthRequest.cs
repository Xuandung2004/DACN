namespace DACN_Web_API.Models
{
    public class RegisterRequest
    {
        public string TenDn { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string Sdt { get; set; }
        public string HoTen { get; set; }
    }

    public class LoginRequest
    {
        public string TenDn { get; set; }
        public string MatKhau { get; set; }
    }
}


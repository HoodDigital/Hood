namespace Hood.ViewModels
{
    public class CreateUserModel
    {
        public string cuFirstName { get; set; }
        public string cuLastName { get; set; }
        public string cuUserName { get; set; }
        public string cuPassword { get; set; }
        public bool cuNotifyUser { get; set; }
        public bool cuGeneratePassword { get; set; }
    }
}
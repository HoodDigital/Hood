namespace Hood.Models
{
    public interface IHoodIdentity
    {
        string Id { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
    }
}

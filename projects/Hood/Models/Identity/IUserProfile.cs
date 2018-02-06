namespace Hood.Models
{
    public interface IUserProfile
    {
        string Bio { get; set; }
        string ClientCode { get; set; }
        string CompanyName { get; set; }
        string DisplayName { get; set; }
        bool EmailOptin { get; set; }
        string Facebook { get; set; }
        string FirstName { get; set; }
        string GooglePlus { get; set; }
        string JobTitle { get; set; }
        string LastName { get; set; }
        string LinkedIn { get; set; }
        string Twitter { get; set; }
        string TwitterHandle { get; set; }
        string VATNumber { get; set; }
        string WebsiteUrl { get; set; }
        string Notes { get; set; }

        void SetProfile(IUserProfile profile);
    }
}
namespace Hood.Interfaces
{
    public interface IAddress
    {
        string ContactName { get; set; }

        string Number { get; set; }
        string Address1 { get; set; }
        string Address2 { get; set; }
        string City { get; set; }
        string County { get; set; }
        string Country { get; set; }
        string Postcode { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }
    }
}


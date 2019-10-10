namespace Hood.Interfaces
{
    public interface IPerson : IName, IAddress
    {
        string JobTitle { get; set; }
        string Phone { get; set; }
    }
}
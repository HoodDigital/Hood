namespace Hood.Interfaces
{
    public interface IMetadata
    {
        int Id { get; set; }
        string Name { get; set; }
        string Type { get; set; }
        string BaseValue { get; set; }
    }
}


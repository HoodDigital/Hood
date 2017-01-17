namespace Hood.Models
{
    public class ContentSettings
    {
        public ContentType[] Types { get; set; }

        public ContentSettings()
        {
            // SET DEFAULTS
            Types = ContentTypes.All.ToArray();
        }
    }
}
using Hood.BaseTypes;
namespace Hood.Models
{
    public class ContentSettings : SaveableModel
    {
        public ContentType[] Types { get; set; }

        public ContentSettings()
        {
            // SET DEFAULTS
            Types = ContentTypes.All.ToArray();
        }
    }
}
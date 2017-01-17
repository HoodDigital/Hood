using Hood.Models.Api;
namespace Hood.Models
{
    public class NewsStoryModel
    {
        public ContentApi Post { get; set; }
        public ContentApi Next { get; set; }
        public ContentApi Previous { get; set; }
        public ApplicationUserApi Author { get; set; }
    }
}

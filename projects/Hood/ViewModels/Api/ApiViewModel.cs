using Hood.BaseTypes;

namespace Hood.ViewModels
{
    public class ApiViewModel : SaveableModel
    {
        public string Title { get; set; }
        public string Details { get; set; }
    }
}

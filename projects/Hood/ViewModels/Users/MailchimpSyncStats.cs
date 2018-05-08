namespace Hood.ViewModels
{
    public class MailchimpSyncStats
    {
        public int Added { get; set; }
        public int Deleted { get; set; }
        public int SiteTotal { get; set; }
        public int MailchimpTotal { get; set; }

        public MailchimpSyncStats()
        {
            Added = 0;
            Deleted = 0;
            SiteTotal = 0;
            MailchimpTotal = 0;
        }
    }
}
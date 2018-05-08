using Hood.Models;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class MailchimpSyncStats
    {
        public int Added { get; set; }
        public int Deleted { get; set; }
        public int SiteTotal { get; set; }
        public int MailchimpTotal { get; set; }
        public List<string> UnsubscribedUsers { get; set; }

        public string UnsubscribedUserList
        {
            get
            {
                var output = "";
                foreach (var user in UnsubscribedUsers)
                    output += string.Format("{0};", user);
                return output;
            }
        }

        public MailchimpSyncStats()
        {
            Added = 0;
            Deleted = 0;
            SiteTotal = 0;
            MailchimpTotal = 0;
        }
    }
}
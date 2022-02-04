using System.Collections.Generic;

namespace Hood.Models
{
    public class UserStatistics
    {
        public UserStatistics(int totalUsers, int totalAdmins, List<KeyValuePair<string, int>> days, List<KeyValuePair<string, int>> months)
        {
            TotalUsers = totalUsers;
            TotalAdmins = totalAdmins;
            Days = days;
            Months = months;
        }

        public int TotalUsers { get; }
        public int TotalAdmins { get; }
        public List<KeyValuePair<string, int>> Days { get; }
        public List<KeyValuePair<string, int>> Months { get; }
    }
}

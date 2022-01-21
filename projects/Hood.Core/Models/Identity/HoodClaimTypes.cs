namespace Hood.Identity
{
    public class HoodClaimTypes
    {
        public const string AccountNotConnected = "https://schemas.hooddigital.com/hoodcms/account-not-connected";
        public const string Active = "https://schemas.hooddigital.com/hoodcms/account-active";
        public const string Anonymous = "https://schemas.hooddigital.com/hoodcms/anonymous";
        public const string LocalUserId = "https://schemas.hooddigital.com/hoodcms/auth-nameidentifier";
    }
    public class Policies
    {
        public const string AccountNotConnected = "hoodcms-account-not-connected";
        public const string Active = "hoodcms-account-active";
    }
    public class Constants
    {
        public const string CookieDefaultName = "hoodcms";

        public const string ReturnUrlParameter = "returnurl";
    }
}
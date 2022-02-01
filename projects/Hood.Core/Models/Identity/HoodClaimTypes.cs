namespace Hood.Identity
{
    public class HoodClaimTypes
    {
        public const string AccountNotConnected = "https://schemas.hooddigital.com/hoodcms/account-not-connected";
        public const string AccountLinkRequired = "https://schemas.hooddigital.com/hoodcms/account-link-required";
        public const string Active = "https://schemas.hooddigital.com/hoodcms/account-active";
        public const string Anonymous = "https://schemas.hooddigital.com/hoodcms/anonymous";
        public const string LocalUserId = "https://schemas.hooddigital.com/hoodcms/local-user-id";
        public const string IsImpersonating = "https://schemas.hooddigital.com/hoodcms/impersonation-active";
        public const string OriginalUserId = "https://schemas.hooddigital.com/hoodcms/impersonation-original-user-id";
        public const string RemotePicture = "https://schemas.hooddigital.com/hoodcms/remote-picture";
        public const string UserName = "name";
        public const string Picture = "picture";
        public const string Nickname = "nickname";
        public const string EmailConfirmed = "email_verified";
    }
    public class Policies
    {
        public const string AccountLinkRequired = "hoodcms-account-link-required";
        public const string AccountNotConnected = "hoodcms-account-not-connected";
        public const string Active = "hoodcms-account-active";
    }
    public class Constants
    {
        public const string CookieDefaultName = "hoodcms";
        public const string ReturnUrlParameter = "returnurl";
        public const string MagicLinkState = "hoodcms-magic-link-login";
        public const string UsernamePasswordConnectionName = "Username-Password-Authentication";
        public const string AuthProviderName = "auth0";
        public const string AddRoleClaimsRuleName = "hoodcms-add-aspnet-roles-as-claims";
        public const string Auth0RoleCacheName = "hoodcms-role-cache";
    }
}
namespace Hood.Identity
{
    public class Authentication
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
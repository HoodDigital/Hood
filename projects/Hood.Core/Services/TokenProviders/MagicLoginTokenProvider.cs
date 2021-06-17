using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class MagicLoginTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
        where TUser : IdentityUser
    {
        public MagicLoginTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<DataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<TUser>> logger) : base(dataProtectionProvider, options, logger)
        {}
    }

    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddMagicLoginTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var magicLoginTokenProviderType = typeof(MagicLoginTokenProvider<>).MakeGenericType(userType);
            return builder.AddTokenProvider("MagicLoginTokenProvider", magicLoginTokenProviderType);
        }
    }
}

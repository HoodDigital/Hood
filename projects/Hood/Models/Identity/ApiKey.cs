using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hood.Models
{
    public class ApiKey : BaseEntity<string>
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }

        public AccessLevel AccessLevel { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public List<ApiEvent> Events { get; set; }

        public string Token
        {
            get
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

                var _contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
                var claims = new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, Id),
                    new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(7)).ToUnixTimeSeconds()}")
                };

                var token = new JwtSecurityToken(new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256)), new JwtPayload(claims));

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }

        public string PublicToken
        {
            get
            {
                var privateKey = Key;
                privateKey += Engine.Settings.Get("Hood.Api.SystemPrivateKey");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));

                var _contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
                var claims = new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, Id),
                    new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(7)).ToUnixTimeSeconds()}")
                };

                var token = new JwtSecurityToken(new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256)), new JwtPayload(claims));

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }

        public bool ValidateToken(string token, AccessLevel access)
        {
            var privateKey = Key;

            if (access == AccessLevel.Public)
            {
                privateKey += Engine.Settings.Get("Hood.Api.SystemPrivateKey");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));

            SecurityToken validatedToken;
            var credentials = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters()
            {
                IssuerSigningKey = key,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = access == AccessLevel.Public,
                ValidateAudience = false,
                ValidateIssuer = false
            }, out validatedToken);

            return credentials.Identity.IsAuthenticated;
        }

    }

    public class ApiKeyPair
    {
        public ApiKeyPair()
        {
        }

        public ApiKeyPair(string encodedBase64String)
        {
            try
            {
                byte[] byteArray = Convert.FromBase64String(encodedBase64String);
                string json = Encoding.UTF8.GetString(byteArray);
                ApiKeyPair decoded = JsonConvert.DeserializeObject<ApiKeyPair>(json);

                Id = decoded.Id;
                Key = decoded.Key;
            }
            catch
            {
                Id = null;
                Key = null;
            }
        }

        public string ToBase64EncodedString()
        {
            string json = JsonConvert.SerializeObject(this);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public string Id { get; set; }
        public string Key { get; set; }
    }
}

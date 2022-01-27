using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Http;
using Hood.Extensions;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Hood.Models;
using System.Linq;
using Hood.Interfaces;
using Hood.Core;
using Newtonsoft.Json;
using System;

namespace Hood.Services
{
    public class Auth0EmailSender : EmailSender
    {
        public Auth0EmailSender() : base()
        { }

        public override async Task<int> NotifyRoleAsync(MailObject message, string roleName, EmailAddress from = null, EmailAddress replyTo = null)
        {
            var auth0Service = new Auth0Service();
            var client = await auth0Service.GetClientAsync();
            var role = (await auth0Service.GetRoles(roleName)).List.FirstOrDefault();
            if (role != null)
            {
                int sent = 0;
                var users = await client.Roles.GetUsersAsync(roleName);
                foreach (var user in users)
                {
                    var messageToSend = message;
                    messageToSend.To = new EmailAddress(user.Email);
                    sent += await SendEmailAsync(messageToSend, from, replyTo);
                }
                return sent;
            }
            return 0;
        }
        public override async Task<int> NotifyRoleAsync(string roleName, string subject, string htmlContent, string textContent = null, EmailAddress from = null, EmailAddress replyTo = null)
        {
            var auth0Service = new Auth0Service();
            var client = await auth0Service.GetClientAsync();
            var role = (await auth0Service.GetRoles(roleName)).List.FirstOrDefault();
            if (role != null)
            {
                var users = await client.Roles.GetUsersAsync(roleName);
                var emails = users.Select(u => new EmailAddress(u.Email, u.FullName)).ToArray();
                return await SendEmailAsync(emails, subject, htmlContent, textContent, from, replyTo);
            }
            return 0;
        }
    }
}
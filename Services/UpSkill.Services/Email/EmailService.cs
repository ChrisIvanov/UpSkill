﻿namespace UpSkill.Services.Email
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.WebUtilities; 

    using System.Text;
    using System.Threading.Tasks; 

    using UpSkill.Common;
    using UpSkill.Data.Models;
    using UpSkill.Services.Contracts.Email;
    using UpSkill.Services.Messaging;

    using static Common.GlobalConstants.EmailSenderConstants;
    using static Common.GlobalConstants.ControllerRoutesConstants;
    using static Common.GlobalConstants.MessagesConstants;

    public class EmailService : IEmailService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public EmailService(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        public async Task SendEmailConfirmation(string origin, ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var verifyUrl = string.Format(
                VerifyUrl,
                origin,
                EmailControllerName,
                VerifyEmailRoute,
                token,
                user.Email);

            var message = string.Format(HtmlContent, verifyUrl);

            await emailSender.SendEmailAsync(FromEmail, EmailSubject, user.Email, message, verifyUrl);
        }

        public async Task<Result> VerifyEmailAsync(string userId, string token) 
        {
            var user = await this.userManager.FindByIdAsync(userId);

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await this.userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                return IncorretcEmail;
            } 

            return true; 
        }

        public async Task<Result> ResendEmailConfirmationLink(string email, string origin)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return EmailsDoNotMatch;
            }

            await SendEmailConfirmation(origin, user);

            return true;
        }
    }
}

﻿namespace UpSkill.Web.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using UpSkill.Data.Models;
    using UpSkill.Services.Contracts.Email;
    using UpSkill.Services.Contracts.Identity;
    using UpSkill.Web.ViewModels.Identity;

    using static Common.GlobalConstants.ControllerRoutesConstants;
    using static Common.GlobalConstants.IdentityConstants;
    using static Common.GlobalConstants.MessagesConstants;

    public class IdentityController : ApiController
    {
        private readonly IIdentityService identity;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailService emailService;

        public IdentityController(
            IIdentityService identity,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            this.identity = identity;
            this.userManager = userManager;
            this.emailService = emailService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(RegisterRoute)]
        public async Task<IActionResult> Register(RegisterRequestModel model)
        {
            await this.ValidateRegisterModel(model);

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var isUserRegistered = await this.identity.RegisterAsync(model);

            if (isUserRegistered.Failure)
            {
                return this.BadRequest(isUserRegistered.Error);
            }

            await this.EmailConfirmation(model.Email);

            return this.StatusCode(201);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(LoginRoute)]
        public async Task<ActionResult<LoginResponseModel>> Login(LoginRequestModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var ipAddress = this.IpAddress();

            var embededToken = await this.identity.LoginAsync(model, ipAddress);

            this.Response.Cookies.Append(JWT, embededToken.Token, new CookieOptions()
            {
                HttpOnly = true,

                // Maybe add an expiration date for the cookie?
            });

            return this.Ok(embededToken);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = this.Request.Cookies[JWT];
            var ipAddress = this.IpAddress();

            var response = await this.identity.RefreshToken(refreshToken, ipAddress);

            if (response == null)
            {
                return this.Unauthorized(new { message = "Invalid token" });
            }

            this.SetTokenCookie(response.RefreshToken);

            return this.Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? this.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return this.BadRequest(new { message = "Token is required" });
            }

            var ipAddress = this.IpAddress();

            var response = await this.identity.RevokeToken(token, ipAddress);

            if (!response)
            {
                return this.NotFound(new { message = "Token not found" });
            }

            return this.Ok(new { message = "Token revoked" });
        }

        [HttpPost]
        [Route(LogoutRoute)]
        public IActionResult Logout()
        {
            this.Response.Cookies.Delete(JWT);

            return this.Ok(new { message = SuccessMessage });
        }

        [HttpGet]
        [Route(UserRoute)]
        public async Task<LoginResponseModel> GetCurrentUser()
        {
            var user = await this.userManager.FindByEmailAsync(this.User.FindFirstValue(ClaimTypes.Email));

            var roles = await this.userManager.GetRolesAsync(user);

            return new LoginResponseModel
            {
                Id = user.Id,
                Email = user.Email,
                Role = roles[0] ?? string.Empty,
            };
        }

        private async Task EmailConfirmation(string email)
        {
            var user = await this.userManager.FindByEmailAsync(email);

            var origin = this.Request.Headers[HeaderOrigin];
            var host = this.Request.Host.Value;

            await this.emailService.SendEmailConfirmationAsync(origin, host, user);
        }

        private async Task ValidateRegisterModel(RegisterRequestModel model)
        {
            if (await this.userManager.Users.AnyAsync(x => x.Email == model.Email))
            {
                this.ModelState.AddModelError(nameof(model.Email), EmailExist);
            }

            if (model.Password != model.ConfirmPassword)
            {
                this.ModelState.AddModelError(nameof(model.Password), PasswordNotMatch);
            }
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,

                // Maybe add cookies expiration here?
            };

            this.Response.Cookies.Append(JWT, token, cookieOptions);
        }
        private string IpAddress()
        {
            if (this.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return this.Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return this.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}

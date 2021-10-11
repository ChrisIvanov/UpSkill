﻿namespace UpSkill.Services.Identity
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    using UpSkill.Common;
    using UpSkill.Data.Common.Repositories;
    using UpSkill.Data.Models;
    using UpSkill.Services.Contracts.Identity;
    using UpSkill.Web.ViewModels.Identity;

    using static UpSkill.Common.GlobalConstants.IdentityConstants;
    using static UpSkill.Common.GlobalConstants.PositionsNamesConstants; 

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IDeletableEntityRepository<Company> companies;
        private readonly IDeletableEntityRepository<Position> positions;
        private readonly AppSettings appSettings; 

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appSettings,
            IDeletableEntityRepository<Company> companies, 
            IDeletableEntityRepository<Position> positions)
        {
            this.userManager = userManager;
            this.companies = companies;
            this.appSettings = appSettings.Value;
            this.positions = positions;
        }

        public string GenerateJwtToken(string userId, string userName, string secret, string userEmail)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Email, userEmail)
                    //Need to add claims for roles
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;
        }

        public async Task<Result> RegisterAsync(RegisterRequestModel model)
        {
            var company = await this.companies
                .All()
                .FirstOrDefaultAsync(x => x.Name == model.CompanyName);

            var positionObj = await this.positions
                .AllAsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == model.PositionName);  

            if (company == null)
            {
                company = new Company { Name = model.CompanyName, };
            }

            if (positions == null)
            {
                return PositionDoesNotExist; 
            }

            var user = new ApplicationUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Company = company,
                PositionId = positionObj.Id,
                Email = model.Email,
                UserName = model.Email
            };

            var result = await this.userManager.CreateAsync(user, model.Password);

            return result.Succeeded;
        }

        public async Task<LoginResponseModel> LoginAsync(LoginRequestModel model)
        {
            var user = await this.userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                throw new ArgumentException(UserNotFound);
            }

            if (!user.EmailConfirmed)
            {
                throw new ArgumentException(ConfirmEmail);
            }

            var passwordValid = await this.userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
            {
                throw new ArgumentException(IncorrectEmailOrPassword);
            }

            var token = GenerateJwtToken(
                user.Id,
                user.UserName,
                this.appSettings.Secret,
                user.Email);

            return new LoginResponseModel()
            {
                Token = token
            };
        }
    }
}

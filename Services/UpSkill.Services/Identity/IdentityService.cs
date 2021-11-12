﻿namespace UpSkill.Services.Identity
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    using UpSkill.Common;
    using UpSkill.Data.Common.Repositories;
    using UpSkill.Data.Configurations;
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
        private readonly IDeletableEntityRepository<ApplicationUser> users;
        private readonly AppSettings appSettings;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appSettings,
            IDeletableEntityRepository<Company> companies,
            IDeletableEntityRepository<Position> positions, 
            IDeletableEntityRepository<ApplicationUser> users)
        {
            this.userManager = userManager;
            this.companies = companies;
            this.appSettings = appSettings.Value;
            this.positions = positions;
            this.users = users;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.appSettings.Secret);

            List<string> roles = (await this.userManager.GetRolesAsync(user)).ToList();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>()
                {
                    { ClaimTypes.NameIdentifier, user.Id },
                    { ClaimTypes.Name, user.UserName },
                    { ClaimTypes.Email, user.Email },
                },
                Expires = DateTime.UtcNow.AddMinutes(5), // the token exipration time shuold be between 5 and 10 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);

                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    CreatedOn = DateTime.UtcNow,
                    CreatedByIp = ipAddress,
                };
            }
        }

        public async Task<bool> RevokeToken(string token, string ipAddress)
        {
            var user = await this.users
                .All()
                .FirstOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
            {
                return false;
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            // Should this method have an Update clause or SaveChangesAsync() is enough ?
            this.users.Update(user);
            await this.users.SaveChangesAsync();

            return true;
        }

        public async Task<LoginResponseModel> RefreshToken(string token, string ipAddress)
        {
            var user = await this.users
                .All()
                .FirstOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
            {
                return null;
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
                return null;
            }

            var newRefreshToken = this.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);

            this.users.Update(user);
            await this.users.SaveChangesAsync();

            var jwtToken = await this.GenerateJwtToken(user);

            return new LoginResponseModel
            {
                Token = jwtToken,
                RefreshToken = newRefreshToken.Token,
            };
        }

        public async Task<Result> RegisterAsync(RegisterRequestModel model)
        {
            var company = await this.companies
                .All()
                .FirstOrDefaultAsync(x => x.Name == model.CompanyName);

            var positionObj = await this.positions
                .AllAsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == OwnerPositionName);

            if (company == null)
            {
                company = new Company { Name = model.CompanyName };
            }

            var user = new ApplicationUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Company = company,
                PositionId = positionObj.Id,
                Email = model.Email,
                UserName = model.Email,
            };

            var result = await this.userManager.CreateAsync(user, model.Password);

            return result.Succeeded;
        }

        public async Task<LoginResponseModel> LoginAsync(LoginRequestModel model, string ipAddress)
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

            var token = await this.GenerateJwtToken(user);
            var refreshToken = this.GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);

            // Should this method have an Update clause or SaveChangesAsync() is enough ?
            this.users.Update(user);

            await this.users.SaveChangesAsync();

            return new LoginResponseModel()
            {
                Token = token,
                RefreshToken = refreshToken.Token,
            };
        }
    }
}

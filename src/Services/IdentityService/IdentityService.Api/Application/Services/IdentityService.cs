using IdentityService.Api.Application.Services.Requests;
using IdentityService.Api.Application.Services.Responses;
using IdentityService.Api.Core.Domain;
using IdentityService.Api.Infrastructure.Context;
using IdentityService.Application.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IdentityContext _identityContext;

        public IdentityService(IdentityContext identityContext)
        {
            _identityContext = identityContext;
        }

        public Task<LoginResponseModel> Login(LoginRequestModel requestModel)
        {
            User user = _identityContext.Users.FirstOrDefault(m => m.Email == requestModel.Email);

            if (user == null)
            {

            }

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, requestModel.Email),
                new Claim(ClaimTypes.Name, "Emre Erkoca"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MicroserviceSampleSecretKeyShouldBeLong"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(10);

            var token = new JwtSecurityToken(claims: claims, expires: expiry, signingCredentials: creds, notBefore: DateTime.Now);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

            LoginResponseModel response = new()
            {
                UserToken = encodedJwt,
                UserName = requestModel.Email
            };

            return Task.FromResult(response);
        }

        public async Task<UserResponse> PostUser(PostUserRequest postUserRequest)
        {
            bool isUserExist = _identityContext.Users.Any(m => m.Email.Equals(postUserRequest.Email));

            if (isUserExist)
            {
                throw new Exception("User already exists!");
            }

            #region Generate Hashed Password
            var saltByte = GenerateSaltByte();
            string hashedPassword = GenerateHashedPassword(saltByte, postUserRequest.Password);
            string salt = Convert.ToBase64String(saltByte);
            #endregion

            var user = new User
            {
                Email = postUserRequest.Email,
                Password = hashedPassword,
                Salt = salt,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };


            await _identityContext.Users.AddAsync(user);
            await _identityContext.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                IsEmailConfirmed = user.IsEmailConfirmed,
                CreateDate = user.CreateDate,
                UpdateDate = user.UpdateDate
            };
        }

        private byte[] GenerateSaltByte()
        {
            byte[] saltByte = new byte[128 / 8];

            using (var rndNumberGenerator = RandomNumberGenerator.Create())
            {
                rndNumberGenerator.GetBytes(saltByte);
            }

            return saltByte;
        }

        private string GenerateHashedPassword(byte[] saltByte, string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltByte,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}

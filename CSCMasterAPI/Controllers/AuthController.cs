using Azure.Core;
using CSCMasterAPI.Persistence.Service;
using CSCMasterAPI.Models;
using CSCMasterAPI.Persistence.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace CSCMasterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        IUserService _userService;
        IConfiguration _config;
        private static readonly Dictionary<string, (Claim[] Claims, DateTime Expiry)> _refreshTokens = new();
        public AuthController(IUserService user, IConfiguration configuration)
        {
            _userService = user;
            _config = configuration;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var member = new Member() { Phone = model.Mobile, Password = model.Password };
            if (_userService.TryFeatchDetailForLogin(ref member))
            {
                var claims = new[]{
                                        new Claim("MemberId", member.Id.ToString()),
                                        new Claim("StationId", member.StationId.ToString()),
                                        new Claim(ClaimTypes.Name, member.Name),
                                        new Claim(ClaimTypes.Role, Role.Member.ToString()),
                                        new Claim(ClaimTypes.MobilePhone, member.Phone.ToString()),
                                        new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(member))
                                    };
                var accessToken = GenerateJwtToken(claims);
                var refreshToken = GenerateRefreshToken(claims);
                var tokenResponse = new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Settings = new LoginSettings()
                    {
                        CurrencySymbole = "₹",
                        DateTimeFormat = "dd-MM-yyyy"
                    },
                    User = new UserModel()
                    {
                        Id = member.Id,
                        Name = member.Name,
                        Mobile = member.Phone,
                        StationId = member.StationId,
                        Role = Role.Member
                    }
                };
                return new JsonResult(new ActionResponse() { SucessValue = tokenResponse });
            }

            var user = new User() { Phone = model.Mobile, Password = model.Password };
            if (_userService.TryFeatchDetailForLogin(ref user))
            {
                var claims = new[]{
                                        new Claim("UserId", user.Id.ToString()),
                                        new Claim(ClaimTypes.Name, user.Name),
                                        new Claim(ClaimTypes.Role, Role.User.ToString()),
                                        new Claim(ClaimTypes.MobilePhone, user.Phone.ToString()),
                                        new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(user))
                                    };
                var accessToken = GenerateJwtToken(claims);
                var refreshToken = GenerateRefreshToken(claims);
                var tokenResponse = new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Settings = new LoginSettings()
                    {
                        CurrencySymbole = "₹",
                        DateTimeFormat = "dd-MM-yyyy"
                    },
                    User = new UserModel()
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Mobile = user.Phone,
                        Role = Role.User
                    }
                };
                return new JsonResult(new ActionResponse() { SucessValue = tokenResponse });
            }

            return Unauthorized();
        }
        
        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] TokenRequest tokens)
        {
            if (!_refreshTokens.TryGetValue(tokens.RefreshToken, out var tokenData))
                return Unauthorized("Invalid refresh token");

            if (tokenData.Expiry < DateTime.UtcNow)
            {
                _refreshTokens.Remove(tokens.RefreshToken);
                return Unauthorized("Refresh token expired");
            }

            var newAccessToken = GenerateJwtToken(tokenData.Claims);
            var newRefreshToken = GenerateRefreshToken(tokenData.Claims, tokens.RefreshToken);
            var tokenResponse = new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return new JsonResult(new ActionResponse() { SucessValue = tokenResponse });
        }
        private string GenerateJwtToken(Claim[] claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("M0MjGA3c4w6FhXpavzFwOuDchrBo9JSZ"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Retinue",
                audience: "CSCMaster",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string GenerateRefreshToken(Claim[] claims, string oldRefreshToken="")
        {
            var newRefreshToken = Guid.NewGuid().ToString("N");
            var newRefreshTokenExpiry = DateTime.UtcNow.AddMinutes(10);
            //foreach (var key in _refreshTokens.Where(x => x.Value.Expiry > DateTime.UtcNow.AddMinutes(1)).Select(x => x.Key).ToList())
            //    _refreshTokens.Remove(key);

            _refreshTokens.Remove(oldRefreshToken);
            _refreshTokens[newRefreshToken] = (claims, newRefreshTokenExpiry);
            return newRefreshToken;
        }
    }
}

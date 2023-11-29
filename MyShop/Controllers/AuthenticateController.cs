using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyShop.Auth;
using MyShop.Models.Domains;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthenticateController(UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        //LOGIN
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await
           _userManager.FindByNameAsync(model.Username);
            if (user != null && await
           _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await
               _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),
                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role,userRole));
                }
                var token = CreateToken(authClaims);
                var refreshToken = GenerateRefreshToken();
                _ =
               int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out
               int refreshTokenValidityInDays);
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime =
               DateTime.Now.AddDays(refreshTokenValidityInDays);
                await _userManager.UpdateAsync(user);
                return Ok(new
                {
                    Token = new
               JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        //REGISTRATION
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody]
        RegisterModel model)
        {
            var userExists = await
           _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return
               StatusCode(StatusCodes.Status500InternalServerError, new Response
               {
                   Status = "Error",
                   Message = "User already exists!"
               });
            User user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user,
           model.Password);
            if (!result.Succeeded)
                return
               StatusCode(StatusCodes.Status500InternalServerError, new Response
               {
                   Status = "Error",
                   Message = "User creation failed! Please check user details and try again." });
                return Ok(new Response
                     {
                         Status = "Success",
                         Message =
                    "User created successfully!"
                     });
        }

        // REGISTRATION ADMIN
        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await
           _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return
               StatusCode(StatusCodes.Status500InternalServerError, new Response
               {
                   Status = "Error",
                   Message = "User already exists!"
               });
            User user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user,
           model.Password);
            if (!result.Succeeded)
                return
               StatusCode(StatusCodes.Status500InternalServerError, new Response
               {
                   Status = "Error",
                   Message = "User creation failed! Please check user details and try again."
               });
            //CHANGEMENT DE UserRoles en UserRole
            if (!await
            _roleManager.RoleExistsAsync(UserRole.Admin))
                await _roleManager.CreateAsync(new
               IdentityRole(UserRole.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRole.User))
                await _roleManager.CreateAsync(new
               IdentityRole(UserRole.User));
            if (await _roleManager.RoleExistsAsync(UserRole.Admin))
            {
                await _userManager.AddToRoleAsync(user,UserRole.Admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRole.Admin))
            {
                await _userManager.AddToRoleAsync(user,UserRole.User);
            }
            return Ok(new Response
            {
                Status = "Success",
                Message =
           "User created successfully!"
            });
        }

        // REFRESN TOKEN
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }
            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;
            var principal =
           GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }
            #pragma warning disable CS8600 // Converting null literal or possible null value to non - nullable type.
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            string username = principal.Identity.Name;
            #pragma warning restore CS8602 // Dereference of a possibly null reference.
            #pragma warning restore CS8600 // Converting null literal or possible null value to non - nullable type.
             var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.RefreshToken != refreshToken ||
           user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }
            var newAccessToken =
            CreateToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);
            return new ObjectResult(new
            {
                accessToken = new
           JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        // REVOKE ONE USER WHEN INVALIDITY
        [Authorize]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Invalid user name");
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        // REVOKE ALL USERS WHEN INVALIDITY
        [Authorize]
        [HttpPost]
        [Route("revoke-all")]
        public async Task<IActionResult> RevokeAll()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);
            }
            return NoContent();
        }

        // CREATE TOKEN
        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new
           SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
           
            _ =
           int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int
           tokenValidityInMinutes);
            var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires:
           DateTime.Now.AddMinutes(tokenValidityInMinutes),
            claims: authClaims,
            signingCredentials: new
           SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return token;
        }

        // GENERATE TOKEN
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // GENERATE REFRESH TOKEN
        private ClaimsPrincipal?
        GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new
           TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new
           SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
            ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token,
           tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken
           jwtSecurityToken ||
           !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
           StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

    }
    }

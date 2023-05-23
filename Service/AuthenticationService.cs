using AutoMapper;
using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts;
using Shared.DataTransferObjects;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service
{
    internal sealed class AuthenticationService : IAuthenticationService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        private User? _user;

        public AuthenticationService(
            ILoggerManager logger,
            IMapper mapper,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        // Register a new user
        public async Task<IdentityResult> RegisterUser(
            UserForRegistrationDto userForRegistration)
        {
            var user = _mapper.Map<User>(userForRegistration);

            // Create the user with the provided password
            var result = await _userManager.CreateAsync(user, userForRegistration.Password);

            if (result.Succeeded)
            {
                // Filter the roles that exist in the role manager
                var validRoles = userForRegistration.Roles
                    .Where(role => _roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    .ToList();

                // Add the user to the valid roles
                await _userManager.AddToRolesAsync(user, validRoles);
            }

            return result;
        }

        // Validate user credentials
        public async Task<bool> ValidateUser(UserForAuthenticationDto userForAuth)
        {
            // Find the user by their username
            _user = await _userManager.FindByNameAsync(userForAuth.UserName);

            // Check if the user exists and the provided password is correct
            var result = (_user != null && await _userManager.CheckPasswordAsync(_user, userForAuth.Password));

            if (!result)
                _logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong user name or password.");

            return result;
        }

        // Create JWT token
        public async Task<string> CreateToken()
        {
            // Get the signing credentials used to sign the token
            var signingCredentials = GetSigningCredentials();

            // Get the claims associated with the authenticated user
            var claims = await GetClaims();

            // Generate the options for the JWT token
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            // Write the token as a string
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        // Get signing credentials for JWT token
        private static SigningCredentials GetSigningCredentials()
        {
            // Get the secret key from the environment variable
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"));

            // Create a new symmetric security key using the secret key
            var secret = new SymmetricSecurityKey(key);

            // Return the signing credentials using the symmetric security key
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        // Get claims for JWT token
        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>
            {
                // Add the user's name as a claim
                new Claim(ClaimTypes.Name, _user.UserName)
            };

            // Get the roles associated with the user
            var roles = await _userManager.GetRolesAsync(_user);

            // Add each role as a claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        // Generate options for JWT token
        private JwtSecurityToken GenerateTokenOptions(
            SigningCredentials signingCredentials,
            List<Claim> claims)
        {
            // Get the JWT settings from the configuration
            var jwtSettings = _configuration.GetSection("JwtSettings");

            // Create a new JWT security token with the specified options
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings["validIssuer"],
                audience: jwtSettings["validAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
                signingCredentials: signingCredentials
            );

            return tokenOptions;
        }
    }
}

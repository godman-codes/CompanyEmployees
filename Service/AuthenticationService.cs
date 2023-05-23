using AutoMapper;
using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace Service
{
    internal sealed class AuthenticationService : IAuthenticationService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

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
        public async Task<IdentityResult> RegisterUser(
            UserForRegistrationDto userForRegistration)
        { 
            var user = _mapper.Map<User>(userForRegistration); 
            var result = await _userManager.CreateAsync(user, userForRegistration.Password); 
            if (result.Succeeded)
                foreach (var role in userForRegistration.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        userForRegistration.Roles.Remove(role);
                    }
                }
                await _userManager.AddToRolesAsync(user, userForRegistration.Roles);
            return result;
        }

    }
}

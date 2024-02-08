using AutoMapper;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListingAPI.Repository
{
    public class AuthManager : IAuthManager
    {
        private IMapper _mapper;
        private UserManager<ApiUser> _userManager;
        private IConfiguration _configuration;
        private readonly ILogger<AuthManager> _logger;
        private ApiUser _user;

        private const string LOGIN_PROVIDER = "HotelListingApi";
        private const string REFRESH_TOKEN = "RefreshToken";

        public AuthManager(
            IMapper mapper, 
            UserManager<ApiUser> userManager, 
            IConfiguration configuration, 
            ILogger<AuthManager> logger
            )
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
            this._logger = logger;
        }


        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = _mapper.Map<ApiUser>(userDto);
            _user.UserName = userDto.Email;

            var result = await _userManager.CreateAsync(_user, userDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }

            return result.Errors;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            bool isValidUser = false;
            try
            {
                _logger.LogInformation($"Looking for user with email {loginDto.Email}");
                _user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (_user == null)
                {
                    _logger.LogWarning($"User with email {loginDto.Email} was not found.");
                    return default;
                }

                isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password);
                if (!isValidUser)
                {
                    return default;
                }

                var token = await GenerateToken();
                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: ");
                Console.Error.WriteLine(e.Message);
                return new AuthResponseDto
                {
                    Token = "",
                    UserId = "500"
                };
            }
        }

        private async Task<string> GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            var userClaims = await _userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id),
            }
            .Union(userClaims)
            .Union(roleClaims);

            var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                    Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])
                ),
            signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateRefreshToken()
        {
            await _userManager.RemoveAuthenticationTokenAsync(_user, LOGIN_PROVIDER, REFRESH_TOKEN);
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, LOGIN_PROVIDER, REFRESH_TOKEN);
            var result = await _userManager.SetAuthenticationTokenAsync(_user, LOGIN_PROVIDER, REFRESH_TOKEN, newRefreshToken);

            return newRefreshToken;
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            var userName = tokenContent.Claims.ToList().FirstOrDefault(
                q => q.Type == JwtRegisteredClaimNames.Email
            )?.Value;
            _user = await _userManager.FindByNameAsync(userName);

            if (_user == null)
            {
                return null;
            }

            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, LOGIN_PROVIDER, REFRESH_TOKEN, request.RefreshToken);

            if (isValidRefreshToken)
            {
                var token = await GenerateToken();
                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }
            await _userManager.UpdateSecurityStampAsync(_user);
            return null;
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignalrServer.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SignalrServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsersContext _usersContext;
        private readonly IConfiguration _configuration;
        //public static User user = new User();

        public AuthController(UsersContext usersContext, IConfiguration configuration)
        {
            _usersContext = usersContext;
            _configuration = configuration;
        }


        [HttpPost("register")]
        public ActionResult<User> Register(UserDto request)
        {
            if (_usersContext.Users.FirstOrDefault(u => u.UserName == request.UserName) != null)
            {
                return BadRequest("This user name already exists in the system");
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            User user = new User()
            {
                UserName = request.UserName,
                PasswordHash = passwordHash,
            };

            try
            {
                _usersContext.Users.Add(user);
                _usersContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(user.UserName + " registered successfully!");
        }

        [HttpPost("login")]
        public ActionResult<User> Login(UserDto request)
        {
            var user = _usersContext.Users.FirstOrDefault(u => u.UserName == request.UserName);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password,user.PasswordHash))
            {
                return BadRequest("Wrong password");
            }

            string token = CreateToken(user);

            return Ok(token);
        }

        [HttpPost("try"), Authorize(Roles = "Admin,User")]
        public ActionResult<User> Try(UserDto request)
        {
            return Ok("Authorize");
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                //new Claim(ClaimTypes.Role,"Admin"),
                new Claim(ClaimTypes.Role,"User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: cred
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}

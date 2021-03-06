using APIServices.Common;
using APIServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace APIServices.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        List<User> GetAll();
        User GetById(long id);
    }

    public class UserService : DAOHelper, IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        /// <summary>
        /// Hàm xác thực tài khoản, tạo token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = ExecStoreToObject<User>(new List<object> { model.Username, model.Password},"masterdata.user_getbyuserpwd", _appSettings.ConnectionString).FirstOrDefault();

            if (user == null) return null;

            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public List<User> GetAll()
        {
            string query = @"
                select *
                from masterdata.user
            ";

            return DAOHelper.ExecQueryToObject<User>(query, _appSettings.ConnectionString);
        }
        /// <summary>
        /// Lấy thông tin User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetById(long id)
        {
            return ExecStoreToObject<User>(new List<object> { id }, "masterdata.user_getbyid", _appSettings.ConnectionString).FirstOrDefault();
        }

        /// <summary>
        /// Tạo jwt token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.USERID.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public void UpdateConnectionId(string username, string connectionId)
        {
            ExecStoreNoneQuery(new List<object> { username, connectionId}, "masterdata.user_connectionid_upd", _appSettings.ConnectionString);
        }

        public User GetByUsername(string username)
        {
            return ExecStoreToObject<User>(new List<object> { username }, "masterdata.user_getbyusername", _appSettings.ConnectionString).FirstOrDefault();
        }
    }
}

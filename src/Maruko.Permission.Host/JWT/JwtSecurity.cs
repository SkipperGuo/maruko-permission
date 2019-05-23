﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Maruko.Permission.Host.JWT
{
    public class JwtSecurity : IJwtSecurity
    {
        private JwtSettings _setting;

        public JwtSecurity(IOptions<JwtSettings> setting)
        {
            this._setting = setting.Value;
        }
        public string JwtSecurityToken(int userId, string roleName)
        {
            var claims = new List<Claim>{
                new Claim(ClaimTypes.Name, userId.ToString()),
            };

            return JwtSecurityToken(claims, roleName);
        }

        public string JwtSecurityToken(List<Claim> claims, string roleName)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_setting.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _setting.Issuer,
                _setting.Audience,
                claims,
                DateTime.Now,
                DateTime.Now.AddHours(2),
                creds);

            return new JwtSecurityTokenHandler().WriteToken(token).Trim();
        }
    }
}

﻿using Autofac;
using Maruko.Permission.Core.Utils.Cache;
using Maruko.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Maruko.Permission.Host.JWT
{
    public class TokenValidate : ISecurityTokenValidator
    {
        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters,
            out SecurityToken validatedToken)
        {
            ClaimsPrincipal principal;
            try
            {
                validatedToken = null;


                var cache = ContainerManager.Current.Resolve<IMarukoCache>();


                var token = new JwtSecurityToken(securityToken);
                //获取到Token的一切信息
                var payload = token.Payload;
                var role = (from t in payload where t.Key == ClaimTypes.Role select t.Value).FirstOrDefault();
                var name = (from t in payload where t.Key == ClaimTypes.Name select t.Value).FirstOrDefault();
                var identityName = (from t in payload where t.Key == ClaimTypes.NameIdentifier select t.Value).FirstOrDefault();
                var issuer = token.Issuer;
                var key = token.SecurityKey;
                var audience = token.Audiences;

                var tokenCache = cache.Get<string>($"token_{ name?.ToString()}");
                if (tokenCache == null || securityToken != tokenCache)
                    throw new Exception();

                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(securityToken, validationParameters, out validatedToken);

                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, name?.ToString()));
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, role?.ToString()));
                if (identityName != null)
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identityName.ToString()));

                principal = new ClaimsPrincipal(identity);
            }
            catch
            {
                validatedToken = null;
                principal = null;
                CanValidateToken = false;

            }
            return principal ?? new ClaimsPrincipal();
        }

        public bool CanValidateToken { get; set; } = true;
        public int MaximumTokenSizeInBytes { get; set; }
    }
}

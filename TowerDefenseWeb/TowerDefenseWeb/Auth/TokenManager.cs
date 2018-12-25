using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TowerDefenseWeb.Auth
{
    public class TokenManager
    {
        private static string _secret = "VGhWbVlxM3Q2dzl6JEMmRilKQE5jUmZValhuWnI0dTd4IUElRCpHLUthUGRTZ1ZrWXAzczV2OHkvQj9FKEgrTWJRZVRoV21acTR0N3c5eiRDJkYpSkBOY1JmVWpYbjJyNXU4eC9BJUQqRy1LYVBkU2dWa1lwM3M2djl5JEImRSg=";

        public static string GenerateToken(string name)
        {
            byte[] key = Convert.FromBase64String(_secret);
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                      new Claim(ClaimTypes.Name, name)}),
                Expires = DateTime.UtcNow.AddMinutes(5256000),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }

        public static string ValidateToken(string token)
        {
            string name = null;
            ClaimsPrincipal principal = GetPrincipal(token);
            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }
            Claim nameClaim = identity.FindFirst(ClaimTypes.Name);
            name = nameClaim.Value;
            return name;
        }

        private static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                    return null;
                byte[] key = Convert.FromBase64String(_secret);
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token,
                      parameters, out SecurityToken securityToken);
                return principal;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
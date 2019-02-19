using Microsoft.IdentityModel.Tokens;
using OneOne.Core.Logger;
using OneOne.Utility4Core.Helper;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Web.Common
{
    public class JwtHelper
    {
        /// <summary>
        /// token签名密钥
        /// </summary>
        private const string Secret = "EatsJwtSecretKey";

        /// <summary>
        /// 用户id的加密密钥
        /// </summary>
        private const string UidKey = "OneOne_UserIdKey";

        /// <summary>
        /// token的过期时间，单位分钟
        /// </summary>
        private const int TokenTimeOut = 120;

        /// <summary>
        /// 根据用户id生成token
        /// </summary>
        /// <param name="userId">用户的id</param>
        /// <returns>token内容</returns>
        public static string GetJwt(int userId)
        {

            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, CryptogramHelper.Encrypt3DES(userId.ToString(),UidKey)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(),
                          ClaimValueTypes.Integer64),
            };
            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: DateTime.Now.AddMinutes(TokenTimeOut),
                signingCredentials: new SigningCredentials(GetSecretKey(), SecurityAlgorithms.HmacSha256)
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        /// <summary>
        /// jwt token解密获取用户标识
        /// </summary>
        /// <param name="token">待验证的token</param>
        /// <returns>验证后的用户id，当用户id为0或者产生异常则表示Token验证失败</returns>
        public static int JwtValidate(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                SecurityToken secretToken = null;
                var tokenValidationParameters = new TokenValidationParameters
                {
                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKeys = new List<SecurityKey> { GetSecretKey() },
                    TokenDecryptionKey = GetSecretKey(),

                    // Validate the token expiry
                    ValidateLifetime = true,
                };
                var value = tokenHandler.ValidateToken(token, tokenValidationParameters, out secretToken);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(p => p.Type == JwtRegisteredClaimNames.Sub);
                int userId = 0;
                if (!string.IsNullOrEmpty(userIdClaim.Value))
                {
                    Int32.TryParse(CryptogramHelper.Decrypt3DES(userIdClaim.Value, UidKey), out userId);
                }
                return userId;
            }
            catch (SecurityTokenValidationException ex)
            {
                Log.Write($"Token校验不通过,token:{token}", MessageType.Error, typeof(JwtHelper), ex);
                return 0;
            }

        }

        /// <summary>
        /// 获取token的加密签名密钥
        /// </summary>
        /// <returns></returns>
        private static SymmetricSecurityKey GetSecretKey()
        {
            var keyByteArray = Encoding.ASCII.GetBytes(Secret);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            return signingKey;
        }
    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using Citadel.Model.Root;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Dynamic;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;

namespace Citadel
{
    public class Common
    {
        public static ErrorMessage GetErrorResponse(string message, string statusCode)
        {
            ErrorMessage error = new ErrorMessage()
            {
                statusCode = statusCode,
                message = message
            };
            return error;

        }

        public static ObjectResult ReturnErrorResponse(string message, string statusCode)
        {
            ErrorMessage error = Common.GetErrorResponse(message, statusCode);
            ObjectResult returnObj;
            switch (statusCode)
            {
                case "400":
                    returnObj = new ObjectResult(JsonSerializer.Serialize(error)) { StatusCode = StatusCodes.Status400BadRequest };
                    break;
                case "401":
                    returnObj = new ObjectResult(JsonSerializer.Serialize(error)) { StatusCode = StatusCodes.Status401Unauthorized };
                    break;
                case "403":
                    returnObj = new ObjectResult(JsonSerializer.Serialize(error)) { StatusCode = StatusCodes.Status403Forbidden };
                    break;
                case "404":
                    returnObj = new ObjectResult(JsonSerializer.Serialize(error)) { StatusCode = StatusCodes.Status404NotFound };
                    break;
                case "409":
                    returnObj = new ObjectResult(JsonSerializer.Serialize(error)) { StatusCode = StatusCodes.Status409Conflict };
                    break;
                case "429":
                    returnObj = new ObjectResult(JsonSerializer.Serialize(error)) { StatusCode = StatusCodes.Status429TooManyRequests };
                    break;
                default:
                    returnObj = new ObjectResult(JsonSerializer.Serialize(error)) { StatusCode = StatusCodes.Status500InternalServerError };
                    break;
            }
            return returnObj;
        }

        public static dynamic GetNothingToDoResponse()
        {
            ExpandoObject response = new ExpandoObject();
            response.TryAdd("200 OK", "Nothing to do");
            var result = new List<ExpandoObject>();
            result.Add(response);
            return result;
        }

        public static async Task<bool> ValidateBearertokenAsync(string token)
        {
            token = token.Remove(0, 7);

            var issuer = Environment.GetEnvironmentVariable("CERTIFICATE_ISSUER");
            var audience = Environment.GetEnvironmentVariable("CERTIFICATE_AUDIENCE");

            var discoveryEndpoint = String.Format("{0}.well-known/openid-configuration", issuer);
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            OpenIdConnectConfiguration config = await OpenIdConnectConfigurationRetriever.GetAsync(discoveryEndpoint, cancellationToken);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKeys = config.SigningKeys
                }, out SecurityToken validatedToken);
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }
    }
}

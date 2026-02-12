using log4net;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Utils
{
	class JiraAuthenticationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly string baseUrl;
		private readonly string key;
		private readonly string authServer;
		private readonly string appAuthKey;
		private readonly string oAuthClientId;

		public enum MethodType
		{
			GET,
			POST,
			PUT,
			DELETE
		}

		//public string AppAuthKey { get { return appAuthKey; } }
		public JiraAuthenticationHelper(string baseUrl, string key, string authServer, string sharedSecret, string oAuthClientId)
		{
			this.baseUrl = baseUrl;
			this.key = key;
			this.authServer = authServer;
			appAuthKey = sharedSecret;
			this.oAuthClientId = oAuthClientId;
			if (sharedSecret == null)
				throw new Exception("Auth key missing!");
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11; 
		}

		public string GetToken(string methodUrl, MethodType methodType = MethodType.GET, string querystring = "")
		{
			SHA256 sha256 = SHA256.Create();
			var qsh = sha256.ComputeHash(Encoding.ASCII.GetBytes(methodType.ToString() + "&" + methodUrl + "&" + querystring));

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appAuthKey));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256, SecurityAlgorithms.Sha256Digest);
			var header = new JwtHeader(credentials);
			DateTime exp = DateTime.Now.AddMinutes(5);
			DateTime iat = DateTime.Now;
			var payload = new JwtPayload
			{
				{ "exp", getUTCSeconds(exp) },
				{"iat", getUTCSeconds(iat)},
				{"iss", key},
				{"qsh", BitConverter.ToString(qsh).Replace("-", "").ToLower()}
			};
			var secToken = new JwtSecurityToken(header, payload);
			var handler = new JwtSecurityTokenHandler();
			var tokenString = handler.WriteToken(secToken);
			return tokenString;
		}

		public JiraToken GetBearerToken(string userKey, string methodUrl, MethodType methodType = MethodType.GET, string queryString = "")
		{
			log.Info("Getting Bearer token...");
			string iss = "urn:atlassian:connect:clientid:" + oAuthClientId;
			string sub = "urn:atlassian:connect:useraccountid:" + userKey;
			string tnt = baseUrl;
			string aud = authServer;
			long iat = getUTCSeconds(DateTime.Now);
			long exp = getUTCSeconds(DateTime.Now.AddMinutes(1));

			var payload = new JwtPayload();
			payload.Add("iss", iss);
			payload.Add("sub", sub);
			payload.Add("tnt", tnt);
			payload.Add("aud", aud);
			payload.Add("iat", iat);
			payload.Add("exp", exp);
			try
			{
				SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appAuthKey));
				SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256, SecurityAlgorithms.Sha256Digest);
				JwtHeader header = new JwtHeader(credentials);

				JwtSecurityToken secToken = new JwtSecurityToken(header, payload);
				JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
				string token = handler.WriteToken(secToken);

				string grantType = "grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&scope=READ+WRITE&assertion=" + token;

				RestClient client = new RestClient("https://auth.atlassian.io/oauth2/token");
				RestRequest request = new RestRequest("", Method.POST);
				request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
				request.AddParameter("application/x-www-form-urlencoded", grantType, ParameterType.RequestBody);
				IRestResponse response = client.Execute(request);
				if (response.ErrorMessage != null) throw new Exception("Couldn't get correct bearer token. Error: " + response.ErrorMessage);
				AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(response.Content);
				log.Info("Token got!");
				return new JiraToken()
				{
					Token = authResponse.AccessToken,
					IssDate = DateTime.Now,
					ExpirationDate = DateTime.Now.AddSeconds(authResponse.ExpiresIn).AddMinutes(-1),
				};
			}
			catch (Exception e)
			{
				log.Error("Something went wrong when getting bearer token...", e);
				return null;
			}
		}

		private static long getUTCSeconds(DateTime date)
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan diff = date.ToUniversalTime() - origin;
			return (long)diff.TotalSeconds;
		}

		private class AuthResponse
		{
			[JsonProperty(PropertyName = "access_token")]
			public string AccessToken { get; set; }
			[JsonProperty(PropertyName = "expires_in")]
			public long ExpiresIn { get; set; }
			[JsonProperty(PropertyName = "token_type")]
			public string TokenType { get; set; }

		}
	}
}

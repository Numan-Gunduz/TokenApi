//using Microsoft.AspNetCore.Mvc;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using TokenApi.Models;
//using System.Text.Json;
//using Microsoft.Extensions.Configuration;
//using System.Net.Http.Headers;

//namespace YourNamespace.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly IConfiguration _configuration;

//        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//        {
//            _httpClientFactory = httpClientFactory;
//            _configuration = configuration;
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] RegisterModel model)
//        {
//            var client = _httpClientFactory.CreateClient();

//            // Admin token almak için çağrı yapılıyor
//            var adminToken = await GetAdminTokenAsync(client);
//            if (adminToken == null)
//            {
//                return StatusCode(500, "Admin token alınamadı.");
//            }

//            // Kullanıcı oluşturma isteği
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

//            var createUserPayload = new
//            {
//                username = model.Username,
//                enabled = true,
//                email = model.Email,
//                credentials = new[]
//                {
//                    new { type = "password", value = model.Password, temporary = false }
//                }
//            };

//            var response = await client.PostAsJsonAsync($"{_configuration["Keycloak:Admin:BaseUrl"]}/admin/realms/{_configuration["Keycloak:Admin:Realm"]}/users", createUserPayload);
//            if (response.IsSuccessStatusCode)
//            {
//                return Ok("Kullanıcı kaydı başarılı.");
//            }

//            return StatusCode((int)response.StatusCode, "Kullanıcı kaydı başarısız.");
//        }
//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] LoginModel model)
//        {
//            var client = _httpClientFactory.CreateClient();

//            var tokenUrl = $"{_configuration["Keycloak:Client:BaseUrl"]}/realms/{_configuration["Keycloak:Client:Realm"]}/protocol/openid-connect/token";

//            var loginParameters = new Dictionary<string, string>
//    {
//        { "client_id", _configuration["Keycloak:Client:ClientId"] },
//        { "client_secret", _configuration["Keycloak:Client:ClientSecret"] },
//        { "grant_type", "password" },
//        { "username", model.Username },
//        { "password", model.Password }
//    };

//            var content = new FormUrlEncodedContent(loginParameters);
//            var response = await client.PostAsync(tokenUrl, content);


//            if (!response.IsSuccessStatusCode)
//            {
//                var error = await response.Content.ReadAsStringAsync();
//                return StatusCode((int)response.StatusCode, $"Login başarısız: {error}");
//            }

//            var tokenResponse = await response.Content.ReadAsStringAsync();
//            var accessToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();
//            var refreshToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("refresh_token").GetString();

//            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
//        }

//        [HttpPost("refresh-token")]
//        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
//        {
//            var client = _httpClientFactory.CreateClient();
//            var tokenUrl = $"{_configuration["Keycloak:Client:BaseUrl"]}/realms/{_configuration["Keycloak:Client:Realm"]}/protocol/openid-connect/token";

//            var refreshTokenParameters = new Dictionary<string, string>
//            {
//                { "client_id", _configuration["Keycloak:Client:ClientId"] },
//                { "client_secret", _configuration["Keycloak:Client:ClientSecret"] },
//                { "grant_type", "refresh_token" },
//                { "refresh_token", model.RefreshToken }
//            };

//            var content = new FormUrlEncodedContent(refreshTokenParameters);
//            var response = await client.PostAsync(tokenUrl, content);

//            if (!response.IsSuccessStatusCode)
//            {
//                var error = await response.Content.ReadAsStringAsync();
//                return StatusCode((int)response.StatusCode, $"Refresh token başarısız: {error}");
//            }

//            var tokenResponse = await response.Content.ReadAsStringAsync();
//            var newAccessToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();
//            var newRefreshToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("refresh_token").GetString();

//            return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
//        }

//        private async Task<string> GetAdminTokenAsync(HttpClient client)
//        {
//            var tokenUrl = $"{_configuration["Keycloak:Admin:BaseUrl"]}/realms/{_configuration["Keycloak:Admin:Realm"]}/protocol/openid-connect/token";

//            var adminTokenParameters = new Dictionary<string, string>
//    {
//        { "client_id", _configuration["Keycloak:Admin:ClientId"] },
//        { "client_secret", _configuration["Keycloak:Admin:ClientSecret"] },
//        { "grant_type", "client_credentials" }
//    };

//            var content = new FormUrlEncodedContent(adminTokenParameters);
//            var response = await client.PostAsync(tokenUrl, content);

//            if (!response.IsSuccessStatusCode)
//            {
//                var error = await response.Content.ReadAsStringAsync();
//                Console.WriteLine($"Token Alınamadı: {response.StatusCode} - {error}");
//                return null;
//            }

//            var tokenResponse = await response.Content.ReadAsStringAsync();
//            var token = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();
//            return token;
//        }
//        [HttpPost("logout")]
//        public async Task<IActionResult> Logout([FromBody] LogoutModel model)
//        {
//            var client = _httpClientFactory.CreateClient();
//            var revokeUrl = $"{_configuration["Keycloak:Client:BaseUrl"]}/realms/{_configuration["Keycloak:Client:Realm"]}/protocol/openid-connect/revoke";

//            var logoutParameters = new Dictionary<string, string>
//    {
//        { "client_id", _configuration["Keycloak:Client:ClientId"] },
//        { "client_secret", _configuration["Keycloak:Client:ClientSecret"] },
//        { "token", model.RefreshToken },
//        { "token_type_hint", "refresh_token" }
//    };

//            var content = new FormUrlEncodedContent(logoutParameters);
//            var response = await client.PostAsync(revokeUrl, content);

//            if (!response.IsSuccessStatusCode)
//            {
//                var error = await response.Content.ReadAsStringAsync();
//                return StatusCode((int)response.StatusCode, $"Logout başarısız: {error}");
//            }

//            return Ok("Kullanıcı başarıyla çıkış yaptı.");
//        }
//        [HttpGet("users")]
//        public async Task<IActionResult> GetUsers()
//        {
//            var client = _httpClientFactory.CreateClient();

//            // Admin token almak için çağrı yapılıyor
//            var adminToken = await GetAdminTokenAsync(client);
//            if (adminToken == null)
//            {
//                return StatusCode(500, "Admin token alınamadı.");
//            }

//            // Kullanıcıları listeleme isteği
//            var usersUrl = $"{_configuration["Keycloak:Admin:BaseUrl"]}/admin/realms/{_configuration["Keycloak:Admin:Realm"]}/users";
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

//            var response = await client.GetAsync(usersUrl);

//            if (!response.IsSuccessStatusCode)
//            {
//                var error = await response.Content.ReadAsStringAsync();
//                return StatusCode((int)response.StatusCode, $"Kullanıcıları listeleme başarısız: {error}");
//            }

//            var users = await response.Content.ReadAsStringAsync();
//            return Ok(JsonDocument.Parse(users));
//        }



//    }
//}
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using TokenApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Kullanıcı kayıt
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var adminToken = await GetAdminTokenAsync(client);
            if (adminToken == null)
            {
                return StatusCode(500, "Admin token alınamadı.");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var createUserPayload = new
            {
                username = model.Username,
                enabled = true,
                email = model.Email,
                credentials = new[]
                {
                    new { type = "password", value = model.Password, temporary = false }
                }
            };

            var response = await client.PostAsJsonAsync($"{_configuration["Keycloak:Admin:BaseUrl"]}/admin/realms/{_configuration["Keycloak:Admin:Realm"]}/users", createUserPayload);
            return response.IsSuccessStatusCode ? Ok("Kullanıcı kaydı başarılı.") : StatusCode((int)response.StatusCode, "Kullanıcı kaydı başarısız.");
        }

        // Kullanıcı girişi
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var tokenUrl = $"{_configuration["Keycloak:Client:BaseUrl"]}/realms/{_configuration["Keycloak:Client:Realm"]}/protocol/openid-connect/token";

            var loginParameters = new Dictionary<string, string>
            {
                { "client_id", _configuration["Keycloak:Client:ClientId"] },
                { "client_secret", _configuration["Keycloak:Client:ClientSecret"] },
                { "grant_type", "password" },
                { "username", model.Username },
                { "password", model.Password }
            };

            var content = new FormUrlEncodedContent(loginParameters);
            var response = await client.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Login başarısız: {error}");
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            var accessToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();
            var refreshToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("refresh_token").GetString();

            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
        }

        // Refresh token ile yeni access token alma
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var tokenUrl = $"{_configuration["Keycloak:Client:BaseUrl"]}/realms/{_configuration["Keycloak:Client:Realm"]}/protocol/openid-connect/token";

            var refreshTokenParameters = new Dictionary<string, string>
            {
                { "client_id", _configuration["Keycloak:Client:ClientId"] },
                { "client_secret", _configuration["Keycloak:Client:ClientSecret"] },
                { "grant_type", "refresh_token" },
                { "refresh_token", model.RefreshToken }
            };

            var content = new FormUrlEncodedContent(refreshTokenParameters);
            var response = await client.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Refresh token başarısız: {error}");
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            var newAccessToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();
            var newRefreshToken = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("refresh_token").GetString();

            return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        }

        // Kullanıcı oturum kapatma
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var revokeUrl = $"{_configuration["Keycloak:Client:BaseUrl"]}/realms/{_configuration["Keycloak:Client:Realm"]}/protocol/openid-connect/revoke";

            var logoutParameters = new Dictionary<string, string>
    {
        { "client_id", _configuration["Keycloak:Client:ClientId"] },
        { "client_secret", _configuration["Keycloak:Client:ClientSecret"] },
        { "token", model.RefreshToken },
        { "token_type_hint", "refresh_token" }
    };

            var content = new FormUrlEncodedContent(logoutParameters);
            var response = await client.PostAsync(revokeUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Logout başarısız: {error}");
            }

            return Ok("Kullanıcı başarıyla çıkış yaptı.");
        }

        //// Keycloak'taki kullanıcıları listeleme
        //[HttpGet("users")]
        //public async Task<IActionResult> GetUsers()
        //{
        //    var client = _httpClientFactory.CreateClient();

        //    // Admin token almak için çağrı yapılıyor
        //    var adminToken = await GetAdminTokenAsync(client);
        //    if (adminToken == null)
        //    {
        //        // Admin yetkisi yoksa 403 Forbidden veya 401 Unauthorized dönebiliriz
        //        return StatusCode(403, "Bu işlem için admin yetkisine sahip olmalısınız.");
        //    }

        //    // Kullanıcıları listeleme isteği
        //    var usersUrl = $"{_configuration["Keycloak:Admin:BaseUrl"]}/admin/realms/{_configuration["Keycloak:Admin:Realm"]}/users";
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        //    var response = await client.GetAsync(usersUrl);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, $"Kullanıcıları listeleme başarısız: {error}");
        //    }

        //    var users = await response.Content.ReadAsStringAsync();
        //    return Ok(JsonDocument.Parse(users));
        //}


        private async Task<string> GetAdminTokenAsync(HttpClient client)
        {
            var tokenUrl = $"{_configuration["Keycloak:Admin:BaseUrl"]}/realms/{_configuration["Keycloak:Admin:Realm"]}/protocol/openid-connect/token";

            var adminTokenParameters = new Dictionary<string, string>
    {
        { "client_id", _configuration["Keycloak:Admin:ClientId"] },
        { "client_secret", _configuration["Keycloak:Admin:ClientSecret"] },
        { "grant_type", "client_credentials" }
    };

            var content = new FormUrlEncodedContent(adminTokenParameters);
            var response = await client.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Admin Token Alınamadı: {response.StatusCode} - {error}");
                return null; // Admin token alınamazsa null döndürülüyor
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();
        }

    }
}

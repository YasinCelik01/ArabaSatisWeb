using ArabaSatisWeb.DataAccess;
using ArabaSatisWeb.JWT;
using ArabaSatisWeb.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Security.Claims;

namespace ArabaSatisWeb.Controllers
{
    public class ApiResponse
    {
        public string Message { get; set; }
        public object Data { get; set; }
    }
    public class KullaniciController : ApiController
    {
        private UserRepository _repository = new UserRepository();

        [HttpPost]
        [Route("api/kullanici/register")]
        public IHttpActionResult Register(User yeniKullanici)
        {
            if (yeniKullanici == null)
            {
                return BadRequest("Geçersiz kullanıcı verisi.");
            }
            if (string.IsNullOrEmpty(yeniKullanici.Username))
            {
                return BadRequest("Kullanıcı adı boş bırakılamaz.");
            }
            if (string.IsNullOrEmpty(yeniKullanici.Email))
            {
                return BadRequest("E-posta boş bırakılamaz.");
            }
            if (string.IsNullOrEmpty(yeniKullanici.PasswordHash))
            {
                return BadRequest("Şifre boş bırakılamaz.");
            }

            var success = _repository.Register(yeniKullanici);
            if (!success)
            {
                return BadRequest("Bu e-psota adresi zaten kayıtlı.");
            }
            return Ok(new ApiResponse { Message = "Kayıt başarılı." });
        }

        [HttpPost]
        [Route("api/kullanici/login")]
        public IHttpActionResult Login([FromBody] UserLoginRequest request)
        {
            if (request == null)
            {
                return BadRequest("Geçersiz kullanıcı verisi.");
            }
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.PasswordHash))
            {
                return BadRequest("Kullanıcı adı ve şifre boş bırakılamaz.");
            }

            var user = _repository.Login(request.Email, request.PasswordHash);
            if (user != null)
            {
                var token = JwtHelper.GenerateToken(user.Email, user.Role);

                return Ok(new ApiResponse { Message = "Giriş başarılı", Data = new { Token = token, User = user } });
            }
            return BadRequest("Geçersiz Email adresi veya gecersiz şifre");
        }

        [HttpGet]
        [Route("api/kullanici/getprofile")]
        public IHttpActionResult GetProfile()
        {
            var authHeader = Request.Headers.Authorization;

            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            var token = authHeader.Parameter;
            var claimsPrincipal = JwtHelper.ValidateToken(token);
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var user = _repository.GetUserByEmail(emailClaim.Value);
            if (user != null)
            {
                return Ok(new ApiResponse { Data = user });
            }

            return NotFound();
        }


        [HttpGet]
        [Route("api/admin/panel")]
        public IHttpActionResult AdminPanel()
        {
            // Cookie'den JWT token'ı alma
            var authHeader = Request.Headers.Authorization;
            var token = authHeader.Parameter;

            if (string.IsNullOrEmpty(token))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            // JWT token doğrulama
            var claimsPrincipal = JwtHelper.ValidateToken(token);

            // Kullanıcının admin olup olmadığını kontrol etme
            var roleClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null || roleClaim.Value != "admin")
            {
                return Content(System.Net.HttpStatusCode.Forbidden, new
                {
                    Message = "Bu sayfayı görüntüleme izniniz yok. Admin yetkileri gereklidir."
                });
            }

            // Admin'e özel içerik veya panel verisini döndürme
            return Ok(new ApiResponse
            {
                Message = "Admin paneline başarılı şekilde erişildi.",
                Data = new { PanelContent = "Burada admin paneline ait veriler gösterilecek." }
            });
        }


        [HttpDelete]
        [Route("api/kullanici/delete")]
        public IHttpActionResult DeleteAccount()
        {
            var authHeader = Request.Headers.Authorization;

            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            var token = authHeader.Parameter;

            var claimsPrincipal = JwtHelper.ValidateToken(token);
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var user = _repository.GetUserByEmail(emailClaim.Value);
            if (user == null)
            {
                return NotFound();
            }

            var success = _repository.DeleteUser(user);
            if (!success)
            {
                return InternalServerError(new Exception("Hesap silme işlemi başarısız oldu."));
            }

            return Ok(new ApiResponse { Message = "Hesap başarıyla silindi." });
        }


        [HttpPut]
        [Route("api/kullanici/updateprofile")]
        public IHttpActionResult UpdateProfile(User updatedUser)
        {
            var authHeader = Request.Headers.Authorization;

            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            var token = authHeader.Parameter;
           
            var claimsPrincipal = JwtHelper.ValidateToken(token);
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var user = _repository.GetUserByEmail(emailClaim.Value);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = updatedUser.Username ?? user.Username;
            user.OldEmail = user.Email;
            user.Email = updatedUser.Email ?? user.Email;

            var success = _repository.UpdateUser(user);
            if (!success)
            {
                return BadRequest("Profil güncellenirken bir hata oluştu.");
            }

            var newToken = JwtHelper.GenerateToken(user.Email, user.Role);
            return Ok(new
            {
                Message = "Profil başarıyla güncellendi.",
                NewToken = newToken
            });
        }



        private string GetTokenFromCookie(string cookieHeader)
        {
            if (string.IsNullOrEmpty(cookieHeader))
            {
                return null;
            }

            var cookies = cookieHeader.Split(';');
            foreach (var cookie in cookies)
            {
                var cookieParts = cookie.Split('=');
                if (cookieParts.Length == 2 && cookieParts[0].Trim() == "authToken")
                {
                    return cookieParts[1].Trim();
                }
            }
            return null;
        }

    }

    public class UserLoginRequest
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}


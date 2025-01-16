using ArabaSatisWeb.DataAccess;
using ArabaSatisWeb.Models;
using ArabaSatisWeb.Classes;
using System;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using ArabaSatisWeb.JWT;
using System.Security.Claims;

namespace ArabaSatisWeb.Controllers
{
    public class ApiRSPNS
    {
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class ArabaController : ApiController
    {
        private CarRepository _carRepository = new CarRepository();
        private UserRepository _repository = new UserRepository();

        [HttpPost]
        [Route("api/araba/ekle")]
        public IHttpActionResult AddCar([FromBody] Car newCar)
        {
            var authHeader = Request.Headers.Authorization;

            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }
            Debug.WriteLine("Car Directory Path3:");
            // JWT token'ı doğrulama
            var token = authHeader.Parameter;
            var claimsPrincipal = JwtHelper.ValidateToken(token);
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized();
            }

            // E-posta adresinden kullanıcıyı al
            var user = _repository.GetUserByEmail(emailClaim.Value);
            if (user == null)
            {
                return Unauthorized();
            }

            // Araba verisini al ve UserId'yi ekle
            if (newCar == null)
            {
                return BadRequest("Geçersiz araba verisi.");
            }

            newCar.UserId = user.UserID;  // JWT'den alınan userId'yi yeni araca ekle
            Debug.WriteLine("Car Directory Path4:");
            // Araba verilerini veritabanına ekle
            bool success = _carRepository.AddCar(newCar);
            if (!success)
            {
                return BadRequest("Araba eklenirken bir hata oluştu.");
            }
            
            // Fotoğrafları kaydet
            if (newCar.FotografListesi != null && newCar.FotografListesi.Any())
            {
                string carDirectory = Path.Combine(HttpContext.Current.Server.MapPath("~/Images/ArabaFotos"), newCar.ArabaId.ToString());
                if (!Directory.Exists(carDirectory))
                {
                    Directory.CreateDirectory(carDirectory);
                }
                foreach (var photo in newCar.FotografListesi)
                {
                    Debug.WriteLine(photo);
                    // Fotoğrafı fiziksel olarak kaydet
                    string fileExtension = ".jpg";
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string filePath = Path.Combine(carDirectory, fileName);

                    // Fotoğrafı kaydet
                    var byteArray = Convert.FromBase64String(photo.Fotograf); // Base64 formatında gönderilen fotoğraf verisi
                    File.WriteAllBytes(filePath, byteArray);

                    // Fotoğrafın veritabanına kaydını yap
                    photo.FotografUrl = "https://localhost:44381/Images/ArabaFotos/" + newCar.ArabaId + "/" + fileName;
                    photo.KayitTarihi = DateTime.Now;

                    if (!_carRepository.AddCarPhoto(photo))
                    {
                        return BadRequest("Fotoğraf kaydedilemedi.");
                    }
                }
            }

            return Ok(new ApiRSPNS { Message = "Araba başarıyla eklendi." });
        }

        [HttpGet]
        [Route("api/araclarim")]
        public IHttpActionResult GetCarsByUser()
        {
            // JWT token'ı almak
            var authHeader = Request.Headers.Authorization;

            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            // JWT token'ı doğrulama
            var token = authHeader.Parameter;
            var claimsPrincipal = JwtHelper.ValidateToken(token);

            // JWT'den e-posta claim'ini al
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized();
            }

            // E-posta adresiyle kullanıcıyı bul
            var user = _repository.GetUserByEmail(emailClaim.Value);
            if (user == null)
            {
                return Unauthorized();
            }

            // Kullanıcının arabalarını al
            var cars = _carRepository.GetCarsByUserId(user.UserID);
            if (cars == null || !cars.Any())
            {
                return NotFound();
            }

            return Ok(new ApiRSPNS
            {
                Message = "Kullanıcıya ait arabalar başarıyla getirildi.",
                Data = cars
            });
        }


        [HttpGet]
        [Route("api/araba/listele")]
        public IHttpActionResult GetAllCars()
        {
            var cars = _carRepository.GetAllCars();
            if (cars == null || !cars.Any())
            {
                return NotFound();
            }

            return Ok(new ApiRSPNS { Message = "Arabalar başarıyla listelendi.", Data = cars });
        }

        [HttpGet]
        [Route("api/admin/dashboard")]
        public IHttpActionResult GetPendingApprovalCars()
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

            // Onay durumu 0 olan arabaları alma
            var carsWithPendingApproval = _carRepository.GetCarsWithPendingApproval(); // Veritabanı üzerinden arabaları al

            if (carsWithPendingApproval == null || !carsWithPendingApproval.Any())
            {
                return Content(System.Net.HttpStatusCode.NotFound, new
                {
                    Message = "Onay bekleyen araba bulunamadı."
                });
            }

            // Admin'e onay bekleyen arabaları döndürme
            return Ok(new ApiRSPNS
            {
                Message = "Onay bekleyen arabalar başarıyla getirildi.",
                Data = carsWithPendingApproval
            });
        }

        [HttpPost]
        [Route("api/admin/approveCar")]
        public IHttpActionResult ApproveCar([FromBody] CarApprovalRequest request)
        {
            // Cookie'den JWT token'ı alma
            var authHeader = Request.Headers.Authorization;
            var token = authHeader?.Parameter;

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

            // İstek geçerli mi kontrol et
            if (request == null || request.CarId <= 0 || (request.Status != 0 && request.Status != 1))
            {
                return Content(System.Net.HttpStatusCode.BadRequest, new
                {
                    Message = "Geçersiz istek. Lütfen geçerli bir araç ID'si ve durum değeri gönderin."
                });
            }

            // Aracı veritabanından bul
     
            var car = _carRepository.GetCarById(request.CarId);            
            if (car == null)
            {
                return Content(System.Net.HttpStatusCode.NotFound, new
                {
                    Message = "Araç bulunamadı."
                });
            }

            // Araç onay durumunu güncelle
            bool updateSuccess = _carRepository.UpdateCarApprovalStatus(request.CarId, request.Status);
            if (!updateSuccess)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, new
                {
                    Message = "Araç durumu güncellenirken bir hata oluştu."
                });
            }

            return Ok(new ApiRSPNS
            {
                Message = "Araç durumu başarıyla güncellendi.",
                Data = car
            });
        }

        [HttpDelete]
        [Route("api/deleteCar")]
        public IHttpActionResult DeleteCar([FromBody] CarDeleteRequest request)
        {
            // Cookie'den JWT token'ı alma
            var authHeader = Request.Headers.Authorization;
            var token = authHeader?.Parameter;

            if (string.IsNullOrEmpty(token))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            // JWT token doğrulama
            var claimsPrincipal = JwtHelper.ValidateToken(token);

            
            var roleClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            

            // İstek geçerli mi kontrol et
            if (request == null || request.CarId <= 0)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, new
                {
                    Message = "Geçersiz istek. Lütfen geçerli bir araç ID'si gönderin."
                });
            }

            // Aracı veritabanından sil
            bool deleteSuccess = _carRepository.DeleteCarById(request.CarId);
            if (!deleteSuccess)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, new
                {
                    Message = "Araç silinirken bir hata oluştu."
                });
            }

            return Ok(new ApiRSPNS
            {
                Message = "Araç başarıyla silindi."
            });
        }

        [HttpPost]
        [Route("api/araba/favorilereEkle")]
        public IHttpActionResult AddToFavorites([FromBody] CarDeleteRequest request)
        {
            // JWT token'ı almak
            var authHeader = Request.Headers.Authorization;

            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            // JWT token'ı doğrulama
            var token = authHeader.Parameter;
            var claimsPrincipal = JwtHelper.ValidateToken(token);

            // JWT'den e-posta claim'ini al
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized();
            }

            // E-posta adresiyle kullanıcıyı bul
            var user = _repository.GetUserByEmail(emailClaim.Value);
            if (user == null)
            {
                return Unauthorized();
            }

            // İstekten carId'yi al
            if (request == null || request.CarId <= 0)
            {
                return BadRequest("Geçersiz istek. 'carId' eksik.");
            }

            
            // Favorilere ekle
            bool success = _carRepository.AddToFavorites(request.CarId, user.UserID);
            if (!success)
            {
                return BadRequest("Favorilere eklenirken bir hata oluştu.");
            }

            return Ok(new ApiRSPNS
            {
                Message = "Araba favorilere başarıyla eklendi."
            });
        }

        [HttpGet]
        [Route("api/favorilerim")]
        public IHttpActionResult GetFavoriteCarsByUser()
        {
            // JWT token'ı almak
            var authHeader = Request.Headers.Authorization;

            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, new
                {
                    Message = "JWT token bulunamadı. Lütfen giriş yapın."
                });
            }

            // JWT token'ı doğrulama
            var token = authHeader.Parameter;
            var claimsPrincipal = JwtHelper.ValidateToken(token);

            // JWT'den e-posta claim'ini al
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized();
            }

            // E-posta adresiyle kullanıcıyı bul
            var user = _repository.GetUserByEmail(emailClaim.Value);
            if (user == null)
            {
                return Unauthorized();
            }
            Debug.WriteLine(user.UserID);
            // Kullanıcının arabalarını al
            var cars = _carRepository.GetFavoriteCarsByUserId(user.UserID);
            if (cars == null || !cars.Any())
            {
                Debug.WriteLine(user.UserID);
                return NotFound();
            }
            
            return Ok(new ApiRSPNS
            {
                Message = "Kullanıcıya ait favori arabalar başarıyla getirildi.",
                Data = cars
            });
        }

        public class CarApprovalRequest
        {
            public int CarId { get; set; } // Araç ID'si
            public byte Status { get; set; } // Onay durumu: 1 = Onaylı, 0 = Reddedilmiş
        }

        public class CarDeleteRequest
        {
            public int CarId { get; set; }
        }
    }
}

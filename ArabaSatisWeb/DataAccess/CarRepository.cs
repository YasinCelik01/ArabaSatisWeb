using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ArabaSatisWeb.Models;
using ArabaSatisWeb.Classes;
using System.Diagnostics;

namespace ArabaSatisWeb.DataAccess
{
    public class CarRepository
    {
        public bool AddCar(Car newCar)
        {
            SqlConnectionClass.CheckConnection();

            using (SqlCommand cmdAddCar = new SqlCommand(
                "INSERT INTO Arabalar (Marka, Model, Yil, Fiyat, Kilometre, YakitTuru, VitesTuru, KasaTuru, MotorHacmi, Renk,Telefon, Aciklama, KayitTarihi,UserId) " +
                "OUTPUT INSERTED.ArabaId " +
                "VALUES (@Marka, @Model, @Yil, @Fiyat, @Kilometre, @YakitTuru, @VitesTuru, @KasaTuru, @MotorHacmi, @Renk,@Telefon, @Aciklama, @KayitTarihi,@UserId)",
                SqlConnectionClass.connection))
            {
                cmdAddCar.Parameters.AddWithValue("@Marka", newCar.Marka);
                cmdAddCar.Parameters.AddWithValue("@Model", newCar.Model);
                cmdAddCar.Parameters.AddWithValue("@Yil", newCar.Yil);
                cmdAddCar.Parameters.AddWithValue("@Fiyat", newCar.Fiyat);
                cmdAddCar.Parameters.AddWithValue("@Kilometre", newCar.Kilometre);
                cmdAddCar.Parameters.AddWithValue("@YakitTuru", newCar.YakitTuru);
                cmdAddCar.Parameters.AddWithValue("@VitesTuru", newCar.VitesTuru);
                cmdAddCar.Parameters.AddWithValue("@KasaTuru", newCar.KasaTuru);
                cmdAddCar.Parameters.AddWithValue("@MotorHacmi", newCar.MotorHacmi);
                cmdAddCar.Parameters.AddWithValue("@Renk", newCar.Renk);
                cmdAddCar.Parameters.AddWithValue("@Telefon", newCar.Telefon);
                cmdAddCar.Parameters.AddWithValue("@Aciklama", newCar.Aciklama ?? (object)DBNull.Value);
                cmdAddCar.Parameters.AddWithValue("@KayitTarihi", newCar.KayitTarihi);
                cmdAddCar.Parameters.AddWithValue("@UserId", newCar.UserId);
                try
                {
                    newCar.ArabaId = (int)cmdAddCar.ExecuteScalar();
                    Debug.WriteLine($"Yeni Araba ID'si: {newCar.ArabaId}");
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Veritabanı hatası: {ex.Message}");
                }

            }

            foreach (var photo in newCar.FotografListesi)
            {
                photo.ArabaId = newCar.ArabaId;                
            }

            return true;
        }

        public bool AddCarPhoto(CarPhoto photo)
        {
            SqlConnectionClass.CheckConnection();

            using (SqlCommand cmdAddPhoto = new SqlCommand(
                "INSERT INTO arabafotograflari (ArabaId, FotografUrl, VarsayilanFotograf, KayitTarihi) " +
                "VALUES (@ArabaId, @FotografUrl, @VarsayilanFotograf, @KayitTarihi)",
                SqlConnectionClass.connection))
            {
                cmdAddPhoto.Parameters.AddWithValue("@ArabaId", photo.ArabaId);
                cmdAddPhoto.Parameters.AddWithValue("@FotografUrl", photo.FotografUrl);
                cmdAddPhoto.Parameters.AddWithValue("@VarsayilanFotograf", photo.VarsayilanFotograf);
                cmdAddPhoto.Parameters.AddWithValue("@KayitTarihi", photo.KayitTarihi);

                try
                {
                    cmdAddPhoto.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        public bool UpdateCarApprovalStatus(int carId, byte newStatus)
        {
            SqlConnectionClass.CheckConnection();

            using (SqlCommand cmdUpdateStatus = new SqlCommand(
                "UPDATE Arabalar SET Onay = @Onay WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
            {
                cmdUpdateStatus.Parameters.AddWithValue("@ArabaId", carId);
                cmdUpdateStatus.Parameters.AddWithValue("@Onay", newStatus);

                try
                {
                    // Onay durumu güncelleniyor
                    int rowsAffected = cmdUpdateStatus.ExecuteNonQuery();

                    // Eğer onay durumu 0 ise, aracı sil
                    if (newStatus == 0 && rowsAffected > 0)
                    {
                        // ArabaFotograflari tablosundaki fotoğrafları sil
                        using (SqlCommand cmdDeletePhotos = new SqlCommand(
                            "DELETE FROM ArabaFotograflari WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
                        {
                            cmdDeletePhotos.Parameters.AddWithValue("@ArabaId", carId);
                            cmdDeletePhotos.ExecuteNonQuery();
                        }

                        // Araba kaydını sil
                        using (SqlCommand cmdDeleteCar = new SqlCommand(
                            "DELETE FROM Arabalar WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
                        {
                            cmdDeleteCar.Parameters.AddWithValue("@ArabaId", carId);
                            int deleteRowsAffected = cmdDeleteCar.ExecuteNonQuery();
                            return deleteRowsAffected > 0; // Eğer araç başarıyla silindiyse true döndür
                        }
                    }

                    return rowsAffected > 0; // Eğer 0'dan fazla satır güncellenmişse başarılı
                }
                catch (SqlException ex)
                {
                    // Hata mesajını yazdır
                    Debug.WriteLine($"SQL Error: {ex.Message}, Code: {ex.Number}, State: {ex.State}, Details: {string.Join(", ", ex.Errors.Cast<SqlError>().Select(e => e.Message))}");
                    return false; // Hata durumunda false döndür
                }
            }
        }



        public Car GetCarById(int carId)
        {
            SqlConnectionClass.CheckConnection();

            Car car = null;

            using (SqlCommand cmdGetCar = new SqlCommand(
                "SELECT * FROM Arabalar WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
            {
                cmdGetCar.Parameters.AddWithValue("@ArabaId", carId);

                try
                {
                    using (SqlDataReader reader = cmdGetCar.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            car = new Car
                            {
                                ArabaId = reader.GetInt32(reader.GetOrdinal("ArabaId")),
                                Marka = reader.GetString(reader.GetOrdinal("Marka")),
                                Model = reader.GetString(reader.GetOrdinal("Model")),
                                Yil = reader.GetInt32(reader.GetOrdinal("Yil")),
                                Fiyat = reader.GetDecimal(reader.GetOrdinal("Fiyat")),
                                Kilometre = reader.GetInt32(reader.GetOrdinal("Kilometre")),
                                YakitTuru = reader.GetString(reader.GetOrdinal("YakitTuru")),
                                VitesTuru = reader.GetString(reader.GetOrdinal("VitesTuru")),
                                KasaTuru = reader.GetString(reader.GetOrdinal("KasaTuru")),
                                MotorHacmi = reader.GetDecimal(reader.GetOrdinal("MotorHacmi")),
                                Renk = reader.GetString(reader.GetOrdinal("Renk")),
                                Telefon = reader.GetString(reader.GetOrdinal("Telefon")),
                                Aciklama = reader.IsDBNull(reader.GetOrdinal("Aciklama")) ? null : reader.GetString(reader.GetOrdinal("Aciklama")),
                                KayitTarihi = reader.GetDateTime(reader.GetOrdinal("KayitTarihi")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                            };
                        }
                    }
                }
                catch (SqlException)
                {
                    return null;
                }
            }

            // Fotoğraf bilgilerini daha sonra yükleyin
            if (car != null)
            {
                car.FotografListesi = GetCarPhotos(car.ArabaId);
            }

            return car;
        }




        public List<Car> GetAllCars()
        {
            SqlConnectionClass.CheckConnection();

            List<Car> cars = new List<Car>();

            using (SqlCommand cmdGetCars = new SqlCommand(
                "SELECT * FROM Arabalar where Onay=1", SqlConnectionClass.connection))
            {
                try
                {
                    using (SqlDataReader reader = cmdGetCars.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var car = new Car
                            {
                                ArabaId = reader.GetInt32(reader.GetOrdinal("ArabaId")),
                                Marka = reader.GetString(reader.GetOrdinal("Marka")),
                                Model = reader.GetString(reader.GetOrdinal("Model")),
                                Yil = reader.GetInt32(reader.GetOrdinal("Yil")),
                                Fiyat = reader.GetDecimal(reader.GetOrdinal("Fiyat")),
                                Kilometre = reader.GetInt32(reader.GetOrdinal("Kilometre")),
                                YakitTuru = reader.GetString(reader.GetOrdinal("YakitTuru")),
                                VitesTuru = reader.GetString(reader.GetOrdinal("VitesTuru")),
                                KasaTuru = reader.GetString(reader.GetOrdinal("KasaTuru")),
                                MotorHacmi = reader.GetDecimal(reader.GetOrdinal("MotorHacmi")),
                                Renk = reader.GetString(reader.GetOrdinal("Renk")),
                                Telefon = reader.GetString(reader.GetOrdinal("Telefon")),
                                Aciklama = reader.IsDBNull(reader.GetOrdinal("Aciklama")) ? null : reader.GetString(reader.GetOrdinal("Aciklama")),
                                KayitTarihi = reader.GetDateTime(reader.GetOrdinal("KayitTarihi")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                Onay = reader.GetInt32(reader.GetOrdinal("Onay"))
                            };
                            cars.Add(car);
                        }
                    }
                }
                catch (SqlException)
                {
                    return null;
                }
            }
            
            foreach (var car in cars)
            {
                car.FotografListesi = GetCarPhotos(car.ArabaId);
            }           
            return cars;
        }


        public List<CarPhoto> GetCarPhotos(int arabaId)
        {
            SqlConnectionClass.CheckConnection();

            List<CarPhoto> photos = new List<CarPhoto>();

            using (SqlCommand cmdGetPhotos = new SqlCommand(
                "SELECT * FROM ArabaFotograflari WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
            {
                cmdGetPhotos.Parameters.AddWithValue("@ArabaId", arabaId);

                try
                {
                    using (SqlDataReader reader = cmdGetPhotos.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Debug.WriteLine("asdf2");
                            var photo = new CarPhoto
                            {
                                FotografId = reader.GetInt32(reader.GetOrdinal("FotografId")),
                                ArabaId = reader.GetInt32(reader.GetOrdinal("ArabaId")),
                                FotografUrl = reader.GetString(reader.GetOrdinal("FotografUrl")),
                                VarsayilanFotograf = reader.GetBoolean(reader.GetOrdinal("VarsayilanFotograf")),
                                KayitTarihi = reader.GetDateTime(reader.GetOrdinal("KayitTarihi"))
                            };
                            photos.Add(photo);
                        }
                    }
                }
                catch (SqlException)
                {
                    return null;
                }
            }
            Debug.WriteLine(photos);
            Debug.WriteLine("asdf");
            return photos;
        }

        public List<Car> GetCarsWithPendingApproval()
        {
            SqlConnectionClass.CheckConnection();

            List<Car> cars = new List<Car>();

            using (SqlCommand cmdGetCars = new SqlCommand(
                "SELECT * FROM Arabalar WHERE Onay = 0", SqlConnectionClass.connection))
            {
                try
                {
                    using (SqlDataReader reader = cmdGetCars.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var car = new Car
                            {
                                ArabaId = reader.GetInt32(reader.GetOrdinal("ArabaId")),
                                Marka = reader.GetString(reader.GetOrdinal("Marka")),
                                Model = reader.GetString(reader.GetOrdinal("Model")),
                                Yil = reader.GetInt32(reader.GetOrdinal("Yil")),
                                Fiyat = reader.GetDecimal(reader.GetOrdinal("Fiyat")),
                                Kilometre = reader.GetInt32(reader.GetOrdinal("Kilometre")),
                                YakitTuru = reader.GetString(reader.GetOrdinal("YakitTuru")),
                                VitesTuru = reader.GetString(reader.GetOrdinal("VitesTuru")),
                                KasaTuru = reader.GetString(reader.GetOrdinal("KasaTuru")),
                                MotorHacmi = reader.GetDecimal(reader.GetOrdinal("MotorHacmi")),
                                Renk = reader.GetString(reader.GetOrdinal("Renk")),
                                Telefon = reader.GetString(reader.GetOrdinal("Telefon")),
                                Aciklama = reader.IsDBNull(reader.GetOrdinal("Aciklama")) ? null : reader.GetString(reader.GetOrdinal("Aciklama")),
                                KayitTarihi = reader.GetDateTime(reader.GetOrdinal("KayitTarihi")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                            };
                            
                            cars.Add(car);
                        }
                    }
                }
                catch (SqlException)
                {
                    return null;
                }
            }
            foreach (var car in cars)
            {
                car.FotografListesi = GetCarPhotos(car.ArabaId);
            }
            return cars;
        }

        public List<Car> GetCarsByUserId(int userId)
        {
            SqlConnectionClass.CheckConnection();

            List<Car> cars = new List<Car>();

            using (SqlCommand cmdGetCars = new SqlCommand(
                "SELECT * FROM Arabalar WHERE UserId = @UserId", SqlConnectionClass.connection))
            {
                cmdGetCars.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    using (SqlDataReader reader = cmdGetCars.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var car = new Car
                            {
                                ArabaId = reader.GetInt32(reader.GetOrdinal("ArabaId")),
                                Marka = reader.GetString(reader.GetOrdinal("Marka")),
                                Model = reader.GetString(reader.GetOrdinal("Model")),
                                Yil = reader.GetInt32(reader.GetOrdinal("Yil")),
                                Fiyat = reader.GetDecimal(reader.GetOrdinal("Fiyat")),
                                Kilometre = reader.GetInt32(reader.GetOrdinal("Kilometre")),
                                YakitTuru = reader.GetString(reader.GetOrdinal("YakitTuru")),
                                VitesTuru = reader.GetString(reader.GetOrdinal("VitesTuru")),
                                KasaTuru = reader.GetString(reader.GetOrdinal("KasaTuru")),
                                MotorHacmi = reader.GetDecimal(reader.GetOrdinal("MotorHacmi")),
                                Renk = reader.GetString(reader.GetOrdinal("Renk")),
                                Telefon = reader.GetString(reader.GetOrdinal("Telefon")),
                                Aciklama = reader.IsDBNull(reader.GetOrdinal("Aciklama")) ? null : reader.GetString(reader.GetOrdinal("Aciklama")),
                                KayitTarihi = reader.GetDateTime(reader.GetOrdinal("KayitTarihi")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                Onay = reader.GetInt32(reader.GetOrdinal("Onay"))
                            };

                            cars.Add(car);
                        }
                    }
                }
                catch (SqlException)
                {
                    return null;
                }
            }

            foreach (var car in cars)
            {
                car.FotografListesi = GetCarPhotos(car.ArabaId);
            }

            return cars;
        }

        public bool DeleteCarById(int carId)
        {
            SqlConnectionClass.CheckConnection();

            try
            {
                // ArabaFotograflari tablosundaki fotoğrafları sil
                using (SqlCommand cmdDeletePhotos = new SqlCommand(
                    "DELETE FROM arabafotograflari WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
                {
                    cmdDeletePhotos.Parameters.AddWithValue("@ArabaId", carId);
                    cmdDeletePhotos.ExecuteNonQuery();
                }
                using (SqlCommand cmdDeletefavori = new SqlCommand(
                    "DELETE FROM Favoriler WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
                {
                    cmdDeletefavori.Parameters.AddWithValue("@ArabaId", carId);
                    cmdDeletefavori.ExecuteNonQuery();
                }
                // Arabayı Arabalar tablosundan sil
                using (SqlCommand cmdDeleteCar = new SqlCommand(
                    "DELETE FROM Arabalar WHERE ArabaId = @ArabaId", SqlConnectionClass.connection))
                {
                    cmdDeleteCar.Parameters.AddWithValue("@ArabaId", carId);
                    int rowsAffected = cmdDeleteCar.ExecuteNonQuery();

                    return rowsAffected > 0; // Eğer araba başarıyla silindiyse true döndür
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"Veritabanı hatası: {ex.Message}");
                return false;
            }
        }

        public bool AddToFavorites(int carId, int userId)
        {
            SqlConnectionClass.CheckConnection();

            using (SqlCommand cmdAddToFavorites = new SqlCommand(
                "INSERT INTO Favoriler (ArabaId, UserId) VALUES (@ArabaId, @UserId)",
                SqlConnectionClass.connection))
            {
                cmdAddToFavorites.Parameters.AddWithValue("@ArabaId", carId);
                cmdAddToFavorites.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    cmdAddToFavorites.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException ex)
                {
                    Debug.WriteLine($"SQL Error: {ex.Message}, Code: {ex.Number}");
                    return false;
                }
            }
        }

        public List<Car> GetFavoriteCarsByUserId(int userId)
        {
            SqlConnectionClass.CheckConnection();
            List<Car> favoriteCars = new List<Car>();

            try
            {
                // Favori Araba ID'lerini al
                using (SqlCommand cmdGetFavorites = new SqlCommand(
                    "SELECT ArabaId FROM Favoriler WHERE UserId = @UserId", SqlConnectionClass.connection))
                {
                    cmdGetFavorites.Parameters.AddWithValue("@UserId", userId);

                    List<int> favoriteCarIds = new List<int>();
                    using (SqlDataReader reader = cmdGetFavorites.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            favoriteCarIds.Add(reader.GetInt32(reader.GetOrdinal("ArabaId")));
                        }
                    } // Burada reader kapanıyor

                    // Eğer favori araba varsa
                    if (favoriteCarIds.Any())
                    {
                        // Arabalar bilgilerini al
                        string query = "SELECT * FROM Arabalar WHERE ArabaId IN (" + string.Join(",", favoriteCarIds) + ")";
                        using (SqlCommand cmdGetCars = new SqlCommand(query, SqlConnectionClass.connection))
                        {
                            using (SqlDataReader carReader = cmdGetCars.ExecuteReader())
                            {
                                while (carReader.Read())
                                {
                                    var car = new Car
                                    {
                                        ArabaId = carReader.GetInt32(carReader.GetOrdinal("ArabaId")),
                                        Marka = carReader.GetString(carReader.GetOrdinal("Marka")),
                                        Model = carReader.GetString(carReader.GetOrdinal("Model")),
                                        Yil = carReader.GetInt32(carReader.GetOrdinal("Yil")),
                                        Fiyat = carReader.GetDecimal(carReader.GetOrdinal("Fiyat")),
                                        Kilometre = carReader.GetInt32(carReader.GetOrdinal("Kilometre")),
                                        YakitTuru = carReader.GetString(carReader.GetOrdinal("YakitTuru")),
                                        VitesTuru = carReader.GetString(carReader.GetOrdinal("VitesTuru")),
                                        KasaTuru = carReader.GetString(carReader.GetOrdinal("KasaTuru")),
                                        MotorHacmi = carReader.GetDecimal(carReader.GetOrdinal("MotorHacmi")),
                                        Renk = carReader.GetString(carReader.GetOrdinal("Renk")),
                                        Telefon = carReader.GetString(carReader.GetOrdinal("Telefon")),
                                        Aciklama = carReader.IsDBNull(carReader.GetOrdinal("Aciklama")) ? null : carReader.GetString(carReader.GetOrdinal("Aciklama")),
                                        KayitTarihi = carReader.GetDateTime(carReader.GetOrdinal("KayitTarihi")),
                                        UserId = carReader.GetInt32(carReader.GetOrdinal("UserId")),
                                        Onay = carReader.GetInt32(carReader.GetOrdinal("Onay"))
                                    };
                                    favoriteCars.Add(car);
                                }
                            }
                        }
                    }
                }
            }

            catch (SqlException ex)
            {
                throw new Exception($"Veri tabanı işlemi sırasında bir hata oluştu: {ex.Message}");
            }
            foreach (var car in favoriteCars)
            {
                car.FotografListesi = GetCarPhotos(car.ArabaId);
            }
            return favoriteCars;
        }



    }
}

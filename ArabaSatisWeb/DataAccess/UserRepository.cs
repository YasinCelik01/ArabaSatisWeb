using System;
using ArabaSatisWeb.Models;
using System.Data.SqlClient;
using ArabaSatisWeb.Classes;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace ArabaSatisWeb.DataAccess
{
    public class UserRepository
    {
        public bool Register(User newUser)
        {
            SqlConnectionClass.CheckConnection();

            newUser.PasswordHash = HashPassword(newUser.PasswordHash);

            using (SqlCommand cmdRegister = new SqlCommand("insert into users (Username, PasswordHash, Email, Role, ProfileImageURL,CreatedDate) values (@Username, @PasswordHash, @Email, @Role, @ProfileImageURL,@CreatedDate)", SqlConnectionClass.connection))
            {
                cmdRegister.Parameters.AddWithValue("@Username", newUser.Username);
                cmdRegister.Parameters.AddWithValue("@PasswordHash", newUser.PasswordHash);
                cmdRegister.Parameters.AddWithValue("@Email", newUser.Email);
                cmdRegister.Parameters.AddWithValue("@Role", newUser.Role);
                cmdRegister.Parameters.AddWithValue("@ProfileImageURL", newUser.ProfileImageURL ?? (object)DBNull.Value);
                cmdRegister.Parameters.AddWithValue("@CreatedDate", newUser.CreatedDate);
                try
                {
                    cmdRegister.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601)
                    {
                        return false;
                    }
                    throw;
                }
            }
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public User Login(string Email, string password)
        {
            User user = null;
            SqlConnectionClass.CheckConnection();

            string hashedPassword = HashPassword(password);

            using (SqlCommand cmdLogin = new SqlCommand("select * from users where Email = @Email and PasswordHash = @PasswordHash", SqlConnectionClass.connection))
            {
                cmdLogin.Parameters.AddWithValue("@Email", Email);
                cmdLogin.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                using (SqlDataReader reader = cmdLogin.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            UserID = (int)reader["UserID"],
                            Username = reader["Username"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString(),
                            ProfileImageURL = reader["ProfileImageURL"]?.ToString(),
                            CreatedDate = (DateTime)reader["CreatedDate"],
                            LastLoginDate = reader["LastLoginDate"] as DateTime?
                        };
                    }
                }
            }

            // Kullanıcı başarılı şekilde giriş yaptıysa LastLoginDate'i güncelle
            if (user != null)
            {
                using (SqlCommand cmdUpdateLastLogin = new SqlCommand("update users set LastLoginDate = @LastLoginDate where UserID = @UserID", SqlConnectionClass.connection))
                {
                    cmdUpdateLastLogin.Parameters.AddWithValue("@LastLoginDate", DateTime.Now);
                    cmdUpdateLastLogin.Parameters.AddWithValue("@UserID", user.UserID);
                    cmdUpdateLastLogin.ExecuteNonQuery();
                }
            }

            return user;
        }

        public User GetUserByEmail(string email)
        {
            User user = null;
            SqlConnectionClass.CheckConnection();

            using (SqlCommand cmd = new SqlCommand("select * from users where Email = @Email", SqlConnectionClass.connection))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            UserID = (int)reader["UserID"],
                            Username = reader["Username"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString(),
                            ProfileImageURL = reader["ProfileImageURL"]?.ToString(),
                            CreatedDate = (DateTime)reader["CreatedDate"],
                            LastLoginDate = reader["LastLoginDate"] as DateTime?
                        };
                    }
                }
            }
            return user;
        }

        public bool DeleteUser(User user)
        {
            try
            {
                var query = "DELETE FROM Users WHERE Email = @Email";
                var parameters = new SqlParameter[] {
            new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email }
        };

                using (var cmd = new SqlCommand(query, SqlConnectionClass.connection))
                {
                    cmd.Parameters.AddRange(parameters);
                    var result = cmd.ExecuteNonQuery();

                    return result == 1;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateUser(User user)
        {
            try
            {
                var query = "UPDATE Users SET Username = @Username, Email = @Email WHERE Email = @OldEmail";
                var parameters = new SqlParameter[] {
            new SqlParameter("@Username", SqlDbType.NVarChar) { Value = user.Username },
            new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email},
            new SqlParameter("@OldEmail", SqlDbType.NVarChar) { Value = user.OldEmail }
        };

                using (var cmd = new SqlCommand(query, SqlConnectionClass.connection))
                {
                    cmd.Parameters.AddRange(parameters);
                    var result = cmd.ExecuteNonQuery();

                    return result == 1;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
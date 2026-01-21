using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using GroenKildeApi.Models;
using GroenKildeApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly string _connectionString;

        public LoginController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ReCircleDb")
                ?? throw new InvalidOperationException("Connection string 'ReCircleDb' is missing.");
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = await GetUserByEmail(request.Email);
            if (user == null || !user.IsActive)
            {
                return Unauthorized();
            }

            var incomingHash = ComputeSha256(request.Password);
            if (!string.Equals(incomingHash, user.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized();
            }

            return Ok(new
            {
                userId = user.UserId,
                navn = user.Navn,
                rolle = user.Rolle
            });
        }

        private async Task<User?> GetUserByEmail(string email)
        {
            const string sql = @"SELECT TOP 1 UserId, Navn, Rolle, Email, PasswordHash, IsActive
FROM Users WHERE Email = @Email";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200) { Value = email });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Navn = reader.GetString(reader.GetOrdinal("Navn")),
                    Rolle = reader.GetString(reader.GetOrdinal("Rolle")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }

            return null;
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using GroenKildeApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenbrugspunkterController : ControllerBase
    {
        private readonly string _connectionString;

        public GenbrugspunkterController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ReCircleDb")
                ?? throw new InvalidOperationException("Connection string 'ReCircleDb' is missing.");
        }

        [HttpGet]
        public async Task<IActionResult> GetGenbrugspunkter()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetGenbrugspunkter", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<Genbrugspunkt>();
            while (await reader.ReadAsync())
            {
                list.Add(new Genbrugspunkt
                {
                    GenbrugspunktId = GetValue<Guid>(reader, "GenbrugspunktId"),
                    Navn = GetValue<string>(reader, "Navn"),
                    GPSPosition = GetNullableString(reader, "GPSPosition")
                });
            }

            return Ok(list);
        }

        private static T GetValue<T>(SqlDataReader reader, string column)
        {
            var ordinal = SafeGetOrdinal(reader, column);
            if (ordinal < 0) return default!;
            return reader.IsDBNull(ordinal) ? default! : (T)reader.GetValue(ordinal);
        }

        private static string? GetNullableString(SqlDataReader reader, string column)
        {
            var ordinal = SafeGetOrdinal(reader, column);
            if (ordinal < 0 || reader.IsDBNull(ordinal)) return null;
            return (string)reader.GetValue(ordinal);
        }

        private static int SafeGetOrdinal(SqlDataReader reader, string column)
        {
            try
            {
                return reader.GetOrdinal(column);
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
        }
    }
}

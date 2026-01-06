using System;
using System.Data;
using GroenKildeApi.Models;
using GroenKildeApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GreenScoreController : ControllerBase
    {
        private readonly string _connectionString;

        public GreenScoreController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ReCircleDb")
                ?? throw new InvalidOperationException("Connection string 'ReCircleDb' is missing.");
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetGreenScore(Guid userId)
        {
            var score = await FetchGreenScore(userId);
            if (score == null) return NotFound();
            return Ok(score);
        }

        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateGreenScore(Guid userId, [FromBody] UpdateGreenScoreRequest request)
        {
            if (request == null) return BadRequest("Body is required.");

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_UpdateGreenScore", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
            cmd.Parameters.Add(new SqlParameter("@Points", SqlDbType.Int) { Value = request.Points });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            GronScore? updated = null;
            if (reader.HasRows && await reader.ReadAsync())
            {
                updated = MapGreenScore(reader);
            }

            if (updated == null)
            {
                updated = await FetchGreenScore(userId);
            }

            if (updated == null) return NoContent();
            return Ok(updated);
        }

        private async Task<GronScore?> FetchGreenScore(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetGreenScore", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.HasRows && await reader.ReadAsync())
            {
                return MapGreenScore(reader);
            }

            return null;
        }

        private static GronScore MapGreenScore(SqlDataReader reader)
        {
            return new GronScore
            {
                GronScoreId = GetValue<Guid>(reader, "GrønScoreId"),
                UserId = GetValue<Guid>(reader, "UserId"),
                Points = GetValue<int>(reader, "Points")
            };
        }

        private static T GetValue<T>(SqlDataReader reader, string column)
        {
            var ordinal = SafeGetOrdinal(reader, column);
            if (ordinal < 0) return default!;
            return reader.IsDBNull(ordinal) ? default! : (T)reader.GetValue(ordinal);
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

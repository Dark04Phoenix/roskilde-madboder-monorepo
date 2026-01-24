using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using GroenKildeApi.Models;
using GroenKildeApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly string _connectionString;

        public StationsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ReCircleDb")
                ?? throw new InvalidOperationException("Connection string 'ReCircleDb' is missing.");
        }

        // ✅ GET: api/stations (arrangør-overblik)
        [HttpGet]
        public async Task<IActionResult> GetStations()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetAllStationsWithStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var result = new List<StationWithStatus>();
            while (await reader.ReadAsync())
            {
                result.Add(MapStationWithStatus(reader));
            }

            return Ok(result);
        }

        // ✅ GET: api/stations/{id} (enkelt station – fx QR-scan)
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStationById(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetStationWithStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@StationId", SqlDbType.UniqueIdentifier)
            {
                Value = id
            });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows || !await reader.ReadAsync())
                return NotFound();

            return Ok(MapStationWithStatus(reader));
        }

        // ✅ PUT: api/stations/{id}/status (frivillig opdaterer status)
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStationStatus(
            Guid id,
            [FromBody] UpdateStationStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.StatusType) || request.UserId == Guid.Empty)
            {
                return BadRequest("StatusType and UserId are required.");
            }

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_UpdateStationStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@StationId", SqlDbType.UniqueIdentifier) { Value = id });
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = request.UserId });
            cmd.Parameters.Add(new SqlParameter("@StatusType", SqlDbType.NVarChar, 50) { Value = request.StatusType });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.HasRows && await reader.ReadAsync())
            {
                return Ok(MapStatus(reader));
            }

            return NoContent();
        }

        // -------------------- MAPPERS --------------------

        private static StationWithStatus MapStationWithStatus(SqlDataReader reader)
        {
            var gps = GetNullableString(reader, "GPSPosition");
            double lat = 0, lng = 0;
            if (!string.IsNullOrWhiteSpace(gps))
            {
                var parts = gps.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat);
                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lng);
                }
            }

            return new StationWithStatus
            {
                StationId = GetValue<Guid>(reader, "StationId"),
                Navn = GetValue<string>(reader, "Navn"),
                GPSPosition = gps,
                Latitude = lat,
                Longitude = lng,
                SenesteStatusId = GetNullableStruct<Guid>(reader, "SenesteStatusId")
            };
        }

        private static StationStatus MapStatus(SqlDataReader reader)
        {
            return new StationStatus
            {
                StatusId = GetValue<Guid>(reader, "StatusId"),
                StationId = GetValue<Guid>(reader, "StationId"),
                Type = GetValue<string>(reader, "Type"),
                Tidspunkt = GetValue<DateTime>(reader, "Tidspunkt"),
                OpdateretAf = GetValue<Guid>(reader, "OpdateretAf")
            };
        }

        // -------------------- HELPERS --------------------

        private static T GetValue<T>(SqlDataReader reader, string column)
        {
            var ordinal = SafeGetOrdinal(reader, column);
            if (ordinal < 0 || reader.IsDBNull(ordinal)) return default!;
            return (T)reader.GetValue(ordinal);
        }

        private static T? GetNullableStruct<T>(SqlDataReader reader, string column) where T : struct
        {
            var ordinal = SafeGetOrdinal(reader, column);
            if (ordinal < 0 || reader.IsDBNull(ordinal)) return null;
            return (T)reader.GetValue(ordinal);
        }

        private static string? GetNullableString(SqlDataReader reader, string column)
        {
            var ordinal = SafeGetOrdinal(reader, column);
            if (ordinal < 0 || reader.IsDBNull(ordinal)) return null;
            return (string)reader.GetValue(ordinal);
        }

        private static int SafeGetOrdinal(SqlDataReader reader, string column)
        {
            try { return reader.GetOrdinal(column); }
            catch { return -1; }
        }
    }
}

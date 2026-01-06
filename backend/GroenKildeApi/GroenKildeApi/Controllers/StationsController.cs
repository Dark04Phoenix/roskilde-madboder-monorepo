using System;
using System.Collections.Generic;
using System.Data;
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

        [HttpGet]
        public async Task<IActionResult> GetStations()
        {
            var stations = await GetStationsInternal(null);
            return Ok(stations);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStationById(Guid id)
        {
            var stations = await GetStationsInternal(id);
            var station = stations.FirstOrDefault();
            if (station == null) return NotFound();
            return Ok(station);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStationStatus(Guid id, [FromBody] UpdateStationStatusRequest request)
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

            StationStatus? newStatus = null;
            if (reader.HasRows)
            {
                if (await reader.ReadAsync())
                {
                    newStatus = MapStatus(reader);
                }
            }

            if (newStatus != null) return Ok(newStatus);

            return NoContent();
        }

        private async Task<List<StationWithStatus>> GetStationsInternal(Guid? stationId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetStationWithStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@StationId", SqlDbType.UniqueIdentifier)
            {
                Value = stationId.HasValue ? stationId.Value : DBNull.Value
            });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var result = new List<StationWithStatus>();
            while (await reader.ReadAsync())
            {
                result.Add(MapStationWithStatus(reader));
            }

            return result;
        }

        private static StationWithStatus MapStationWithStatus(SqlDataReader reader)
        {
            return new StationWithStatus
            {
                StationId = GetValue<Guid>(reader, "StationId"),
                Navn = GetValue<string>(reader, "Navn"),
                GPSPosition = GetNullableString(reader, "GPSPosition"),
                SenesteStatusId = GetNullableStruct<Guid>(reader, "SenesteStatusId"),
                StatusId = GetNullableStruct<Guid>(reader, "StatusId"),
                StatusType = GetNullableString(reader, "Type"),
                StatusTidspunkt = GetNullableStruct<DateTime>(reader, "Tidspunkt"),
                OpdateretAf = GetNullableStruct<Guid>(reader, "OpdateretAf")
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

        private static T GetValue<T>(SqlDataReader reader, string column)
        {
            var ordinal = SafeGetOrdinal(reader, column);
            if (ordinal < 0) return default!;
            return reader.IsDBNull(ordinal) ? default! : (T)reader.GetValue(ordinal);
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

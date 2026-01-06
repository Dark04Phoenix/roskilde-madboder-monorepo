using System;
using System.Collections.Generic;
using System.Data;
using GroenKildeApi.Models;
using GroenKildeApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly string _connectionString;

        public ReportsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ReCircleDb")
                ?? throw new InvalidOperationException("Connection string 'ReCircleDb' is missing.");
        }

        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var reports = await ReadReportsAsync();
            return Ok(reports);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
        {
            if (request == null || request.RapporteretAf == Guid.Empty || string.IsNullOrWhiteSpace(request.Kategori))
            {
                return BadRequest("RapporteretAf and Kategori are required.");
            }

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CreateReport", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@RapporteretAf", SqlDbType.UniqueIdentifier) { Value = request.RapporteretAf });
            cmd.Parameters.Add(new SqlParameter("@Beskrivelse", SqlDbType.NVarChar, 500)
            {
                Value = string.IsNullOrWhiteSpace(request.Beskrivelse) ? DBNull.Value : request.Beskrivelse
            });
            cmd.Parameters.Add(new SqlParameter("@Kategori", SqlDbType.NVarChar, 50) { Value = request.Kategori });
            cmd.Parameters.Add(new SqlParameter("@StationId", SqlDbType.UniqueIdentifier)
            {
                Value = request.StationId.HasValue ? request.StationId.Value : DBNull.Value
            });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            Rapport? created = null;
            if (reader.HasRows && await reader.ReadAsync())
            {
                created = MapRapport(reader);
            }

            // Fallback: if procedure does not return the row, echo basic payload
            if (created == null)
            {
                created = new Rapport
                {
                    RapportId = Guid.Empty,
                    StationId = request.StationId,
                    RapporteretAf = request.RapporteretAf,
                    Beskrivelse = request.Beskrivelse,
                    Kategori = request.Kategori,
                    Tidspunkt = DateTime.UtcNow,
                    Haandteret = false
                };
            }

            return Created("/api/reports", created);
        }

        [HttpPut("{id:guid}/handle")]
        public async Task<IActionResult> HandleReport(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_HandleReport", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@RapportId", SqlDbType.UniqueIdentifier) { Value = id });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            Rapport? handled = null;
            if (reader.HasRows && await reader.ReadAsync())
            {
                handled = MapRapport(reader);
            }

            if (handled != null) return Ok(handled);

            return NoContent();
        }

        private async Task<List<Rapport>> ReadReportsAsync()
        {
            // Der er ingen separat stored procedure til at hente alle rapporter,
            // så vi laver et simpelt read direkte mod tabellen.
            const string sql = "SELECT RapportId, StationId, RapporteretAf, Beskrivelse, Kategori, Tidspunkt, Haandteret FROM Rapport ORDER BY Tidspunkt DESC";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<Rapport>();
            while (await reader.ReadAsync())
            {
                list.Add(MapRapport(reader));
            }
            return list;
        }

        private static Rapport MapRapport(SqlDataReader reader)
        {
            return new Rapport
            {
                RapportId = GetValue<Guid>(reader, "RapportId"),
                StationId = GetNullableStruct<Guid>(reader, "StationId"),
                RapporteretAf = GetValue<Guid>(reader, "RapporteretAf"),
                Beskrivelse = GetNullableString(reader, "Beskrivelse"),
                Kategori = GetValue<string>(reader, "Kategori"),
                Tidspunkt = GetValue<DateTime>(reader, "Tidspunkt"),
                Haandteret = GetValue<bool>(reader, "Haandteret")
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

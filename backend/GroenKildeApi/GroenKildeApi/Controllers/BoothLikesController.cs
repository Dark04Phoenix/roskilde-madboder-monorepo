using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoothLikesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public BoothLikesController(IConfiguration config) => _config = config;

        // POST /api/BoothLikes/{boothId}/like?phoneId=123
        [HttpPost("{boothId}/like")]
        [Produces("application/json")]
        public async Task<IActionResult> LikeBooth(Guid boothId, [FromQuery] string phoneId)
        {
            if (boothId == Guid.Empty) return BadRequest("boothId is required.");
            if (string.IsNullOrWhiteSpace(phoneId)) return BadRequest("phoneId is required.");

            try
            {
                var cs = _config.GetConnectionString("GroenKildeDb");
                using var conn = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.BoothLikes WHERE Booth_ID=@Booth_ID AND Phone_ID=@Phone_ID)
BEGIN
    INSERT INTO dbo.BoothLikes (Like_ID, Booth_ID, Phone_ID, Time)
    VALUES (NEWID(), @Booth_ID, @Phone_ID, GETDATE())
END", conn);

                cmd.Parameters.Add(new SqlParameter("@Booth_ID", SqlDbType.UniqueIdentifier) { Value = boothId });
                cmd.Parameters.Add(new SqlParameter("@Phone_ID", SqlDbType.VarChar, 50) { Value = phoneId });

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true });
            }
            catch (SqlException ex)
            {
                return Problem($"SQL error: {ex.Message}", statusCode: 500);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }

        // DELETE /api/BoothLikes/{boothId}/like?phoneId=123
        [HttpDelete("{boothId}/like")]
        [Produces("application/json")]
        public async Task<IActionResult> UnlikeBooth(Guid boothId, [FromQuery] string phoneId)
        {
            if (boothId == Guid.Empty) return BadRequest("boothId is required.");
            if (string.IsNullOrWhiteSpace(phoneId)) return BadRequest("phoneId is required.");

            try
            {
                var cs = _config.GetConnectionString("GroenKildeDb");
                using var conn = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
DELETE FROM dbo.BoothLikes WHERE Booth_ID=@Booth_ID AND Phone_ID=@Phone_ID", conn);

                cmd.Parameters.Add(new SqlParameter("@Booth_ID", SqlDbType.UniqueIdentifier) { Value = boothId });
                cmd.Parameters.Add(new SqlParameter("@Phone_ID", SqlDbType.VarChar, 50) { Value = phoneId });

                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, removed = rows });
            }
            catch (SqlException ex)
            {
                return Problem($"SQL error: {ex.Message}", statusCode: 500);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }

        // GET /api/BoothLikes/{boothId}/likes
        [HttpGet("{boothId}/likes")]
        [Produces("application/json")]
        public async Task<IActionResult> GetBoothLikes(Guid boothId)
        {
            if (boothId == Guid.Empty) return BadRequest("boothId is required.");

            try
            {
                var cs = _config.GetConnectionString("GroenKildeDb");
                using var conn = new SqlConnection(cs);
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.BoothLikes WHERE Booth_ID=@Booth_ID", conn);

                cmd.Parameters.Add(new SqlParameter("@Booth_ID", SqlDbType.UniqueIdentifier) { Value = boothId });

                await conn.OpenAsync();
                var count = (int)await cmd.ExecuteScalarAsync();

                return Ok(new { Booth_ID = boothId, Likes = count });
            }
            catch (SqlException ex)
            {
                return Problem($"SQL error: {ex.Message}", statusCode: 500);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }
    }
}

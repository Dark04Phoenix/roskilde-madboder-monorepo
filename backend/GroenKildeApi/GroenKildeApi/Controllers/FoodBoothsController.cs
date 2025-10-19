using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using GroenKildeApi.Data;
using GroenKildeApi.Models;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodBoothsController : ControllerBase
    {
        private readonly GroenKildeContext _context;
        private readonly IConfiguration _config;

        public FoodBoothsController(GroenKildeContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ✅ GET alle vores madboder
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetFoodBooths()
        {
            var booths = await _context.FoodBooths.ToListAsync();
            return Ok(booths);
        }

        // ✅ POST: Opretter en ny madbod (skal gøres gennem stored procedure)
        [HttpPost]
        public async Task<IActionResult> CreateFoodBooth([FromBody] FoodBooth booth)
        {
            if (booth == null)
                return BadRequest("Invalid booth data.");

            string connectionString = _config.GetConnectionString("GroenKildeDb");
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("CreateFoodBooth", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", booth.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", booth.Latitude);
                cmd.Parameters.AddWithValue("@Longitude", booth.Longitude);
                cmd.Parameters.AddWithValue("@CuisineType", booth.CuisineType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", booth.Description ?? (object)DBNull.Value);

                var outputId = new SqlParameter("@NewBooth_ID", System.Data.SqlDbType.UniqueIdentifier)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                cmd.Parameters.Add(outputId);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                booth.Booth_ID = (Guid)outputId.Value;
            }

            return CreatedAtAction(nameof(GetFoodBooths), new { id = booth.Booth_ID }, booth);
        }

        // ✅ PUT: Opdatere eksisterende madbod (Bruger stored procedure)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodBooth(Guid id, [FromBody] FoodBooth booth)
        {
            if (id != booth.Booth_ID)
                return BadRequest("Booth ID mismatch.");

            string connectionString = _config.GetConnectionString("GroenKildeDb");
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("UpdateFoodBooth", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Booth_ID", booth.Booth_ID);
                cmd.Parameters.AddWithValue("@Name", booth.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", booth.Latitude);
                cmd.Parameters.AddWithValue("@Longitude", booth.Longitude);
                cmd.Parameters.AddWithValue("@CuisineType", booth.CuisineType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", booth.Description ?? (object)DBNull.Value);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            return NoContent();
        }

        // ✅ DELETE: Sletter en madbod (Bruger stored procedure)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodBooth(Guid id)
        {
            string connectionString = _config.GetConnectionString("GroenKildeDb");
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("DeleteFoodBooth", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Booth_ID", id);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            return NoContent();
        }
    }
}

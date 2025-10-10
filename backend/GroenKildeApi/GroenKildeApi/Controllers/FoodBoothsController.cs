using Microsoft.AspNetCore.Mvc;
using GroenKildeApi.Data;
using GroenKildeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GroenKildeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodBoothsController : ControllerBase
    {
        private readonly GroenKildeContext _context;

        public FoodBoothsController(GroenKildeContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<FoodBooth>>> GetFoodBooths()
        {
            var booths = await _context.FoodBooths.ToListAsync();
            return Ok(booths);
        }

    }
}

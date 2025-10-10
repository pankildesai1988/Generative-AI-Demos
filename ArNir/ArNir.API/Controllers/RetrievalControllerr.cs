using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RetrievalController : ControllerBase
    {
        private readonly IRetrievalService _retrieval;
        public RetrievalController(IRetrievalService retrieval) => _retrieval = retrieval;

        [HttpPost("test")]
        public async Task<IActionResult> Test([FromBody] string query)
        {
            var results = await _retrieval.SearchAsync(query);
            return Ok(results);
        }
    }
}

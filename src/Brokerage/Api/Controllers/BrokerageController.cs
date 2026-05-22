using Microsoft.AspNetCore.Mvc;
using Moka.src.Brokerage.Application.Dtos;
using Moka.src.Brokerage.Application.Interfaces;
using Moka.src.Shared;

namespace Moka.src.Brokerage.Api.Controllers
{
    [ApiController]
    [Route("api/brokerage")]
    public class BrokerageController(IBrokerageService service) : ControllerBase
    {
        private readonly IBrokerageService _service = service;

        [HttpPost("create-profile")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto request)
        {
            var profile = await _service.CreateProfileAsync(request);
            return profile.ToActionResult();
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute] int id)
        {
            var profile = await _service.GetProfileByIdAsync(id);
            return profile.ToActionResult();
        }

        [HttpGet("profiles")]
        public async Task<IActionResult> GetProfilesAsync()
        {
            var profiles = await _service.GetProfilesAsync();
            return profiles.ToActionResult();
        }

        [HttpPut("update-profile-status/{id}")]
        public async Task<IActionResult> UpdateProfileStatusAsync([FromRoute] int id, [FromBody] string status)
        {
            var result = await _service.UpdateProfileStatusAsync(id, status);
            return result.ToActionResult();
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moka.src.Authentication.Services;
using Moka.src.Insurance.Application.Dtos;
using Moka.src.Insurance.Application.Interfaces;
using Moka.src.Shared;

namespace Moka.src.Insurance.Api.Controllers
{
    [ApiController]
    [Route("api/insurance")]
    public class InsuranceController(IInsurancePremiumService service) : ControllerBase
    {
        private readonly IInsurancePremiumService _service = service;

        [HttpPost("premiums")]
        public async Task<IActionResult> CreatePremiumAsync([FromBody] CreateInsurancePremiumDto request)
        {
            var result = await _service.CreatePremiumAsync(request);
            return result.ToActionResult();
        }

        [HttpGet("getPremiums")]
        [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
        public async Task<IActionResult> GetPremiumsAsync([FromQuery] GetInsurancePremiumsQueryDto query)
        {
            var tokenUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(tokenUserId, out var authenticatedUserId))
                return Unauthorized();

            var result = await _service.GetPremiumsAsync(authenticatedUserId, query);
            return result.ToActionResult();
        }

        [HttpPut("premiums/{id}")]
        [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
        public async Task<IActionResult> UpdatePremiumAsync([FromRoute] Guid id, [FromBody] UpdateInsurancePremiumDto request)
        {
            var tokenUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(tokenUserId, out var authenticatedUserId))
                return Unauthorized();

            var result = await _service.UpdatePremiumAsync(authenticatedUserId, id, request);
            return result.ToActionResult();
        }
    }
}

using System.Security.Claims;
using FitZone.Application.DTOs.MembershipDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Controllers
{
    [ApiController]
    [Route("api/memberships")]
    public class MembershipsController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpGet("plans")]
        public async Task<ActionResult<ICollection<MembershipPlanDTO>>> GetPlans(CancellationToken ct)
        {
           return Ok(await _membershipService.GetPlansAsync(ct));
        }


        [Authorize]
        [HttpPost("activate")]
        public async Task<ActionResult<UserMembershipDTO>> Activate(
            ActivateMembershipRequestDTO request, CancellationToken ct)
        {
            return Ok(await _membershipService.ActivateAsync(GetUserId(), request, ct));
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<UserMembershipDTO>> GetMy(CancellationToken ct)
        {
            var result = await _membershipService.GetMyActiveAsync(GetUserId(), ct);
            return result is null ? NotFound() : Ok(result);
        }

        private Guid GetUserId()
        {
           return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
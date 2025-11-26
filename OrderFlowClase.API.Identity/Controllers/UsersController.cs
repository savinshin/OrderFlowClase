using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderFlowClase.API.Identity.Dto.Users;
using OrderFlowClase.API.Identity.Services;

namespace OrderFlowClase.API.Identity.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Authorize]
    [Route("/api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }



        [HttpPatch("{userId}/change-password")]
        public async Task<ActionResult<PasswordChangeResponse>> ChangePassword(
            string userId,
            [FromBody] PasswordChangeRequest request,
            [FromServices] IValidator<PasswordChangeRequest> validator
            )
        {
            var validation = await validator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(validation.Errors);
            }

            var result = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            var response = new PasswordChangeResponse
            {
                Success = result
            };

            if (!result)
            {
                return BadRequest(response);
            }

            return Ok(response);

        }

    }
}

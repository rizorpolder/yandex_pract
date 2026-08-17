using Application.Services.Abstraction.Services;
using Domain.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("auth")]
public class UserController(IUserService userService) : ControllerBase
{
	[HttpPost("/register", Name = nameof(Register))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Register(string login, string password)
	{
		var result = await userService.RegisterAsync(login, password);

		if (!result.IsSuccess)
			return BadRequest(new { Message = result.ErrorMessage });

		return Ok();
	}

	[HttpPost("/login", Name = nameof(Login))]
	[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Login(string login, string password)
	{
		var result = await userService.LoginAsync(login, password);

		if (!result.IsSuccess)
			return Unauthorized(new { Message = result.ErrorMessage });

		return Ok(result.Value);
	}
	
	[Authorize(Roles = "Admin")]
	[HttpPost("/admin/create")]
	public async Task<IActionResult> CreateAdmin(string login, string password)
	{
		var result = await userService.RegisterAsync(login, password, UserRole.Admin);
		if (!result.IsSuccess)
			return BadRequest(new { Message = result.ErrorMessage });

		return Ok();
	}
}
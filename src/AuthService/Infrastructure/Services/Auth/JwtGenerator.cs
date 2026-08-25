using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Services.Abstraction.Services.Auth;
using Common.Models;
using Domain.Models.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Auth;

public class JwtGenerator : IJwtGenerator
{
	private readonly JwtOptions _options;

	public JwtGenerator(IOptions<JwtOptions> options)
	{
		_options = options.Value;
	}

	public string GenerateJwtToken(Guid userId, string login, UserRole role)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim("id", userId.ToString()),
			new Claim("login", login),
			new Claim("role", role.ToString())
		};
		var token = new JwtSecurityToken(
			_options.Issuer,
			_options.Audience,
			claims,
			expires: DateTime.UtcNow.AddMinutes(_options.LifetimeMinutes),
			signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
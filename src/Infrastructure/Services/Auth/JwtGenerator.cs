using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Services.Abstraction.Services.Auth;
using Domain.Models.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Auth;

public class JwtGenerator : IJwtGenerator
{
	private readonly string _secret;
	private readonly string _issuer;
	private readonly string _audience;
	private readonly int _lifetimeMinutes;


	public JwtGenerator(IConfiguration config)
	{
		var section = config.GetSection("Jwt");
		_secret = section["Secret"];
		_issuer = section["Issuer"];
		_audience = section["Audience"];
		_lifetimeMinutes = int.Parse(section["LifetimeMinutes"]);
	}

	public string GenerateJwtToken(Guid userId, string login, UserRole role)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.Sha256);

		var claims = new[]
		{
			new Claim("id", userId.ToString()),
			new Claim("login", login),
			new Claim("role", role.ToString())
		};
		var token = new JwtSecurityToken(
			_issuer,
			_audience,
			claims,
			expires: DateTime.UtcNow.AddMinutes(_lifetimeMinutes),
			signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
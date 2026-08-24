using Application.Services.Abstraction.Services;
using Application.Services.Abstraction.Services.Auth;
using Application.Services.UserService;
using Common.Tests.Interfaces;
using Domain.Models.Users;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IntegrationTest.IntegrationTests;

[Collection("Database")]
public sealed class UsersTest : ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["users"];

	public UsersTest(PostgresContainerFixture fixture)
		: base(fixture, options => new AppDbContext(options))
	{
	}

	private static IUserService CreateService(AppDbContext ctx)
	{
		var userRepo = new EfUserRepository(ctx);

		var hasher = new Mock<IPasswordHasher>();
		hasher.Setup(h => h.GetHash(It.IsAny<string>()))
			.Returns((string p) => $"HASH_{p}");
		hasher.Setup(h => h.VerifyHash(It.IsAny<string>(), It.IsAny<string>()))
			.Returns((string p, string hash) => hash == $"HASH_{p}");

		var jwt = new Mock<IJwtGenerator>();
		jwt.Setup(j => j.GenerateJwtToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<UserRole>()))
			.Returns("TOKEN");

		return new UserService(userRepo, hasher.Object, jwt.Object);
	}

	[Fact]
	public async Task Register_ShouldCreateUser_AndReturnToken()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var service = CreateService(ctx);

		var result = await service.RegisterAsync("user", "pass");

		Assert.True(result.IsSuccess);
		Assert.Equal("TOKEN", result.Value);

		var user = await ctx.Users.FirstAsync();
		Assert.Equal("user", user.Login);
		Assert.Equal("HASH_pass", user.PasswordHash);
		Assert.Equal(UserRole.User, user.Role);
	}

	[Fact]
	public async Task Register_DuplicateLogin_ShouldFail()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var service = CreateService(ctx);

		var r1 = await service.RegisterAsync("user", "pass");
		Assert.True(r1.IsSuccess);

		var r2 = await service.RegisterAsync("user", "pass2");

		Assert.False(r2.IsSuccess);
		Assert.Equal("User already exists", r2.ErrorMessage);
	}

	[Fact]
	public async Task Login_ShouldReturnToken_WhenPasswordIsCorrect()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var service = CreateService(ctx);

		await service.RegisterAsync("user", "pass");

		var login = await service.LoginAsync("user", "pass");

		Assert.True(login.IsSuccess);
		Assert.Equal("TOKEN", login.Value);
	}

	[Fact]
	public async Task Login_ShouldFail_WhenPasswordIsWrong()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var service = CreateService(ctx);

		await service.RegisterAsync("user", "pass");

		var login = await service.LoginAsync("user", "wrong");

		Assert.False(login.IsSuccess);
		Assert.Equal("Invalid login or password", login.ErrorMessage);
	}

	[Fact]
	public async Task Login_ShouldFail_WhenUserDoesNotExist()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var service = CreateService(ctx);

		var login = await service.LoginAsync("nope", "pass");

		Assert.False(login.IsSuccess);
		Assert.Equal("Invalid login or password", login.ErrorMessage);
	}
}
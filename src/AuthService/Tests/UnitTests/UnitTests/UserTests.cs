using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.Abstraction.Services.Auth;
using Application.Services.UserService;
using Domain.Models.Users;

using Moq;
using Xunit;

namespace EventTests.Tests;

public class UserTests
{
	private static (Mock<IUserRepository> repo,
		Mock<IPasswordHasher> hasher,
		Mock<IJwtGenerator> jwt,
		IUserService service) Create()
	{
		var repo = new Mock<IUserRepository>();
		var hasher = new Mock<IPasswordHasher>();
		var jwt = new Mock<IJwtGenerator>();

		var service = new UserService(repo.Object, hasher.Object, jwt.Object);

		return (repo, hasher, jwt, service);
	}

	[Fact]
	public async Task Register_ShouldCreateUser_AndReturnToken()
	{
		var (repo, hasher, jwt, service) = Create();

		repo.Setup(r => r.GetByLoginAsync("user"))
			.ReturnsAsync((User?) null);

		hasher.Setup(h => h.GetHash("pass"))
			.Returns("HASH_pass");

		jwt.Setup(j => j.GenerateJwtToken(It.IsAny<Guid>(), "user", UserRole.User))
			.Returns("TOKEN");

		var result = await service.RegisterAsync("user", "pass");

		Assert.True(result.IsSuccess);
		Assert.Equal("TOKEN", result.Value);

		repo.Verify(r => r.AddAsync(It.Is<User>(u =>
				u.Login == "user" &&
				u.PasswordHash == "HASH_pass" &&
				u.Role == UserRole.User)),
			Times.Once);

		repo.Verify(r => r.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task Register_DuplicateLogin_ShouldFail()
	{
		var (repo, hasher, jwt, service) = Create();

		repo.Setup(r => r.GetByLoginAsync("user"))
			.ReturnsAsync(new User("user", "hash", UserRole.User));

		var result = await service.RegisterAsync("user", "pass");

		Assert.False(result.IsSuccess);
		Assert.Equal("User already exists", result.ErrorMessage);

		repo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
		repo.Verify(r => r.SaveChangesAsync(), Times.Never);
	}

	[Fact]
	public async Task Login_ShouldReturnToken_WhenPasswordIsCorrect()
	{
		var (repo, hasher, jwt, service) = Create();

		var user = new User("user", "HASH_pass", UserRole.User);

		repo.Setup(r => r.GetByLoginAsync("user"))
			.ReturnsAsync(user);

		hasher.Setup(h => h.VerifyHash("pass", "HASH_pass"))
			.Returns(true);

		jwt.Setup(j => j.GenerateJwtToken(user.Id, "user", UserRole.User))
			.Returns("TOKEN");

		var result = await service.LoginAsync("user", "pass");

		Assert.True(result.IsSuccess);
		Assert.Equal("TOKEN", result.Value);
	}

	[Fact]
	public async Task Login_ShouldFail_WhenPasswordIsWrong()
	{
		var (repo, hasher, jwt, service) = Create();

		var user = new User("user", "HASH_pass", UserRole.User);

		repo.Setup(r => r.GetByLoginAsync("user"))
			.ReturnsAsync(user);

		hasher.Setup(h => h.VerifyHash("wrong", "HASH_pass"))
			.Returns(false);

		var result = await service.LoginAsync("user", "wrong");

		Assert.False(result.IsSuccess);
		Assert.Equal("Invalid login or password", result.ErrorMessage);
	}

	[Fact]
	public async Task Login_ShouldFail_WhenUserDoesNotExist()
	{
		var (repo, hasher, jwt, service) = Create();

		repo.Setup(r => r.GetByLoginAsync("nope"))
			.ReturnsAsync((User?) null);

		var result = await service.LoginAsync("nope", "pass");

		Assert.False(result.IsSuccess);
		Assert.Equal("Invalid login or password", result.ErrorMessage);
	}
}
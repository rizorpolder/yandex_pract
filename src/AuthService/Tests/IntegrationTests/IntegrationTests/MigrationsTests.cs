using Application.Services.UserService;
using Common.Models;
using Common.Tests.Interfaces;
using Domain.Models.Users;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Infrastructure.Services.Auth;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IntegrationTest.IntegrationTests;

[Collection("Database")]
public class MigrationsTests : ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate => ["users"];

	public MigrationsTests(PostgresContainerFixture fixture) : base(fixture, options => new AppDbContext(options))
	{
	}

	[Fact]
	public async Task Migrations_ShouldApplySuccessfully()
	{
		await using var ctx = CreateContext();

		await ctx.Database.MigrateAsync();

		var applied = await ctx.Database.GetAppliedMigrationsAsync();
		Assert.NotEmpty(applied);
		Assert.Contains(applied, m => m.Contains("InitialAuthCreate"));

		var pending = await ctx.Database.GetPendingMigrationsAsync();
		Assert.Empty(pending);
	}

	[Fact]
	public async Task Migrations_ShouldCreateUsersTable()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var usersExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM information_schema.tables WHERE table_name = 'users'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, usersExists);
	}

	[Fact]
	public async Task Migrations_ShouldCreateUniqueIndexOnLogin()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var indexExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM pg_indexes WHERE indexname = 'ix_users_login'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, indexExists);
	}

	[Fact]
	public async Task Migrations_ShouldEnforceUniqueLoginConstraint()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		ctx.Users.Add(new User("duplicate", "hash1", UserRole.User));
		await ctx.SaveChangesAsync();

		ctx.Users.Add(new User("duplicate", "hash2", UserRole.User));

		await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
	}

	[Fact]
	public async Task Migrations_ShouldCreateCorrectColumnTypes()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var idType = await ctx.Database
			.SqlQueryRaw<int>(
				"SELECT 1 AS \"Value\" FROM information_schema.columns " +
				"WHERE table_name = 'users' AND column_name = 'id' AND data_type = 'uuid'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, idType);

		var loginType = await ctx.Database
			.SqlQueryRaw<int>(
				"SELECT 1 AS \"Value\" FROM information_schema.columns " +
				"WHERE table_name = 'users' AND column_name = 'login' AND data_type = 'character varying'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, loginType);
	}

	[Fact]
	public async Task Migrations_ShouldAllowRepositoryOperations()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var repo = new EfUserRepository(ctx);
		var hasher = new Sha256PasswordHasher();

		var jwtOptions = Options.Create(new JwtOptions
		{
			Secret = "test_secret_key_for_integration_tests_1234567890",
			Issuer = "TestIssuer",
			Audience = "TestAudience",
			LifetimeMinutes = 60
		});

		var jwt = new JwtGenerator(jwtOptions);

		var service = new UserService(repo, hasher, jwt);

		var result = await service.RegisterAsync("integration_user", "pass123");

		Assert.True(result.IsSuccess);

		var savedUser = await ctx.Users.FirstOrDefaultAsync(u => u.Login == "integration_user");
		Assert.NotNull(savedUser);
		Assert.Equal(UserRole.User, savedUser.Role);
	}
}
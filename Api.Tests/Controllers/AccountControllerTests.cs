using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace Api.Tests;

public class AccountControllerTests
{
    private AccountController CreateController(out AppDbContext db)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new AppDbContext(options);

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock
            .Setup(x => x.CreateToken(It.IsAny<AppUser>()))
            .Returns("mock-token");

        return new AccountController(db, tokenServiceMock.Object);
    }

    [Fact]
    public async Task Register_ShouldCreateUser_AndReturnUserDto()
    {
        var controller = CreateController(out var db);

        var dto = new RegisterDto
        {
            DisplayName = "John",
            Email = "john@test.com",
            Password = "pass123"
        };

        var result = await controller.Register(dto);
        var returned = result.Value;

        returned.Should().NotBeNull();
        returned!.DisplayName.Should().Be("John");
        returned.Email.Should().Be("john@test.com");
        returned.Token.Should().Be("mock-token");

        db.Users.Count().Should().Be(1);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenEmailExists()
    {
        var controller = CreateController(out var db);

        db.Users.Add(new AppUser
        {
            DisplayName = "Existing",
            Email = "exist@test.com",
            PasswordHash = [],
            PasswordSalt = []
        });
        await db.SaveChangesAsync();

        var dto = new RegisterDto
        {
            DisplayName = "John",
            Email = "exist@test.com",
            Password = "pass123"
        };

        var result = await controller.Register(dto);

        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();
        bad!.Value.Should().Be("Email taken");
    }

    [Fact]
    public async Task Login_ShouldReturnUserDto_WhenCredentialsAreValid()
    {
        var controller = CreateController(out var db);

        var password = "pass123";
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Email = "john@test.com",
            DisplayName = "John",
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = "john@test.com",
            Password = "pass123"
        };

        var result = await controller.Login(loginDto);
        var returned = result.Value;

        returned.Should().NotBeNull();
        returned!.DisplayName.Should().Be("John");
        returned.Email.Should().Be("john@test.com");
        returned.Token.Should().Be("mock-token");
    }

    [Fact]
    public async Task Login_ShouldFail_WhenEmailIsWrong()
    {
        var controller = CreateController(out var db);

        var loginDto = new LoginDto
        {
            Email = "doesnotexist@test.com",
            Password = "pass123"
        };

        var result = await controller.Login(loginDto);

        var unauthorized = result.Result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
        unauthorized!.Value.Should().Be("Invalid email address");
    }

    [Fact]
    public async Task Login_ShouldFail_WhenPasswordIsWrong()
    {
        var controller = CreateController(out var db);

        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            Email = "john@test.com",
            DisplayName = "John",
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("correct-password"))
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = "john@test.com",
            Password = "wrong-password"
        };

        var result = await controller.Login(loginDto);

        var unauthorized = result.Result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
        unauthorized!.Value.Should().Be("Invalid password");
    }
}
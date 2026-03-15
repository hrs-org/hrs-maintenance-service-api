using System.Security.Claims;
using FluentAssertions;
using HRS.API.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace HRS.Test.API.Services;

public class UserContextServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly UserContextService _svc;

    public UserContextServiceTests()
    {
        _svc = new UserContextService(_httpContextAccessor);
    }

    // ──────────────────────────── Helpers ────────────────────────────

    private static HttpContext BuildContext(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ctx = new DefaultHttpContext { User = principal };
        return ctx;
    }

    // ──────────────────────────── GetUserId ────────────────────────────

    [Fact]
    public void GetUserId_FromSubClaim_ReturnsUserId()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim("sub", "42")]));
        _svc.GetUserId().Should().Be(42);
    }

    [Fact]
    public void GetUserId_FromNameIdentifierClaim_ReturnsUserId()
    {
        _httpContextAccessor.HttpContext.Returns(
            BuildContext([new Claim(ClaimTypes.NameIdentifier, "99")]));
        _svc.GetUserId().Should().Be(99);
    }

    [Fact]
    public void GetUserId_SubClaimTakesPriorityOverNameIdentifier()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("sub", "10"),
            new Claim(ClaimTypes.NameIdentifier, "20")
        ]));
        _svc.GetUserId().Should().Be(10);
    }

    [Fact]
    public void GetUserId_NoMatchingClaim_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([]));
        _svc.GetUserId().Should().Be(0);
    }

    [Fact]
    public void GetUserId_NullHttpContext_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        _svc.GetUserId().Should().Be(0);
    }

    [Fact]
    public void GetUserId_NonNumericSubClaim_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim("sub", "not-a-number")]));
        _svc.GetUserId().Should().Be(0);
    }

    // ──────────────────────────── GetStoreId ────────────────────────────

    [Fact]
    public void GetStoreId_FromStoreIdClaim_ReturnsStoreId()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim("storeId", "5")]));
        _svc.GetStoreId().Should().Be(5);
    }

    [Fact]
    public void GetStoreId_MissingStoreIdClaim_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([]));
        _svc.GetStoreId().Should().Be(0);
    }

    [Fact]
    public void GetStoreId_NullHttpContext_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        _svc.GetStoreId().Should().Be(0);
    }

    // ──────────────────────────── GetEmail ────────────────────────────

    [Fact]
    public void GetEmail_FromEmailClaim_ReturnsEmail()
    {
        _httpContextAccessor.HttpContext.Returns(
            BuildContext([new Claim("email", "user@example.com")]));
        _svc.GetEmail().Should().Be("user@example.com");
    }

    [Fact]
    public void GetEmail_FromClaimTypesEmail_ReturnsEmail()
    {
        _httpContextAccessor.HttpContext.Returns(
            BuildContext([new Claim(ClaimTypes.Email, "alt@example.com")]));
        _svc.GetEmail().Should().Be("alt@example.com");
    }

    [Fact]
    public void GetEmail_EmailClaimTakesPriorityOverClaimTypesEmail()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("email", "primary@example.com"),
            new Claim(ClaimTypes.Email, "secondary@example.com")
        ]));
        _svc.GetEmail().Should().Be("primary@example.com");
    }

    [Fact]
    public void GetEmail_NoEmailClaim_ReturnsNull()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([]));
        _svc.GetEmail().Should().BeNull();
    }

    [Fact]
    public void GetEmail_NullHttpContext_ReturnsNull()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        _svc.GetEmail().Should().BeNull();
    }

    // ──────────────────────────── GetUserAsync ────────────────────────────

    [Fact]
    public async Task GetUserAsync_AllClaimsPresent_ReturnsFullDto()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("sub", "7"),
            new Claim("email", "john@example.com"),
            new Claim("given_name", "John"),
            new Claim("family_name", "Doe"),
            new Claim("role", "Admin")
        ]));

        var result = await _svc.GetUserAsync();

        result.Id.Should().Be(7);
        result.Email.Should().Be("john@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task GetUserAsync_MissingGivenName_FallsBackToUnknown()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("sub", "1"),
            new Claim("email", "a@b.com"),
            new Claim("family_name", "Smith"),
            new Claim("role", "Employee")
        ]));

        var result = await _svc.GetUserAsync();

        result.FirstName.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetUserAsync_MissingFamilyName_FallsBackToUser()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("sub", "1"),
            new Claim("email", "a@b.com"),
            new Claim("given_name", "Jane"),
            new Claim("role", "Employee")
        ]));

        var result = await _svc.GetUserAsync();

        result.LastName.Should().Be("User");
    }

    [Fact]
    public async Task GetUserAsync_MissingEmailClaim_FallsBackToUnknownEmail()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("sub", "1"),
            new Claim("given_name", "Jane"),
            new Claim("family_name", "Doe"),
            new Claim("role", "Employee")
        ]));

        var result = await _svc.GetUserAsync();

        result.Email.Should().Be("unknown@example.com");
    }

    [Fact]
    public async Task GetUserAsync_RoleFromClaimTypesRole_ReturnsRole()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("sub", "3"),
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim("given_name", "Sam"),
            new Claim("family_name", "Lee")
        ]));

        var result = await _svc.GetUserAsync();

        result.Role.Should().Be("Manager");
    }

    [Fact]
    public async Task GetUserAsync_MissingRoleClaim_FallsBackToUser()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext(
        [
            new Claim("sub", "3"),
            new Claim("given_name", "Sam"),
            new Claim("family_name", "Lee")
        ]));

        var result = await _svc.GetUserAsync();

        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task GetUserAsync_NullHttpContext_ReturnsDefaults()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var result = await _svc.GetUserAsync();

        result.Id.Should().Be(0);
        result.Email.Should().Be("unknown@example.com");
        result.FirstName.Should().Be("Unknown");
        result.LastName.Should().Be("User");
        result.Role.Should().Be("User");
    }
}

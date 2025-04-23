using System.Linq.Expressions;
using System.Security.Claims;
using AssistantNest.Exceptions;
using AssistantNest.Models;
using AssistantNest.Repositories;
using AssistantNest.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AssistantNest.Tests.Services;

public class AuthServiceTests
{
    private readonly Guid _userIdRejectedCookies = Guid.NewGuid();
    private readonly Guid _userIdAcceptedCookies = Guid.NewGuid();
    private readonly DateTime _now = DateTime.UtcNow;
    
    public AuthServiceTests()
    {
    }
    
    private AnUser UserAcceptedCookies => new AnUser(_userIdAcceptedCookies)
    {
        EncounteredAt = _now,
        AcceptedCookiesAt = _now,
        UpdatedAt = _now,
    };
    
    private AnUser UserRejectedCookies => new AnUser(_userIdRejectedCookies)
    {
        EncounteredAt = _now,
        AcceptedCookiesAt = default,
        UpdatedAt = _now,
    };

    private ILogger<AuthService> Logger { get; } = A.Fake<ILogger<AuthService>>();
    private IRepository<AnUser> GetUserRepoWithUser(AnUser anUser)
    {
        IRepository<AnUser> fakeRepo = A.Fake<IRepository<AnUser>>();
        A.CallTo(() => fakeRepo.GetAsync(A<Expression<Func<AnUser,bool>>>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily((Expression<Func<AnUser, bool>> predicate, CancellationToken token) =>
            {
                return Task.FromResult((AnUser?)anUser);
            });
        return fakeRepo;
    }
    private HttpContext GetHttpContextWithUserId(Guid userId, out IAuthenticationService authService)
    {
        HttpContext httpContext = A.Fake<HttpContext>();
        ClaimsIdentity claimsIdentity = new([new Claim(Constants.IdClaim, userId.ToString())], Constants.AuthScheme);
        ClaimsPrincipal claimsPrincipal = new(claimsIdentity);
        A.CallTo(() => httpContext.User).Returns(claimsPrincipal);
        authService = A.Fake<IAuthenticationService>();
        IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IAuthenticationService)))
            .Returns(authService);
        A.CallTo(() => httpContext.RequestServices).Returns(serviceProvider);
        return httpContext;
    }

    [Fact]
    public async Task SignInUserAsync_ShouldReturnNull_WhenCookiesNotAccepted()
    {
        HttpContext httpContext = GetHttpContextWithUserId(_userIdRejectedCookies, out _);
        AnUser? result = await new AuthService(GetUserRepoWithUser(UserRejectedCookies), Logger)
            .SignInUserAsync(httpContext, acceptedCookies: false, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task SignInUserAsync_ReturnsUser_WhenCookieValid()
    {
        HttpContext httpContext = GetHttpContextWithUserId(_userIdAcceptedCookies, out IAuthenticationService authService);
        A.CallTo(() => authService.AuthenticateAsync(httpContext, Constants.AuthScheme))
            .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(httpContext.User, Constants.AuthScheme))));
        A.CallTo(() => authService.SignInAsync(httpContext, Constants.AuthScheme, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored))
            .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(httpContext.User, Constants.AuthScheme))));
        AnUser? result = await new AuthService(GetUserRepoWithUser(UserAcceptedCookies), Logger)
            .SignInUserAsync(httpContext, acceptedCookies: true, CancellationToken.None);
        result.Should().NotBeNull();
        result.Should().BeOfType<AnUser>();
        result.Id.Should().Be(UserAcceptedCookies.Id);
        result.HasAcceptedCookies.Should().BeTrue();
    }

    [Fact]
    public async Task SignInUserAsync_CreatesNewUser_WhenNoExistingUserAndCookiesAccepted()
    {
        HttpContext httpContext = GetHttpContextWithUserId(_userIdAcceptedCookies, out IAuthenticationService authService);
        IRepository<AnUser> repo = A.Fake<IRepository<AnUser>>();
        A.CallTo(() => repo.GetAsync(A<Expression<Func<AnUser, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult((AnUser?)null));
        A.CallTo(() => repo.AddAsync(A<AnUser>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult((AnUser?)UserAcceptedCookies));
        A.CallTo(() => authService.AuthenticateAsync(httpContext, Constants.AuthScheme))
            .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(httpContext.User, Constants.AuthScheme))));
        A.CallTo(() => authService.SignInAsync(httpContext, Constants.AuthScheme, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored))
            .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(httpContext.User, Constants.AuthScheme))));
        AnUser? insertedUser = null;
        A.CallTo(() => repo.AddAsync(A<AnUser>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult((AnUser?)(insertedUser = new AnUser(_userIdAcceptedCookies)
            {
                EncounteredAt = _now,
                AcceptedCookiesAt = _now,
                UpdatedAt = _now,
            })));


        AnUser? result = await new AuthService(repo, Logger)
            .SignInUserAsync(httpContext, acceptedCookies: true, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<AnUser>();
        result.Id.Should().Be(_userIdAcceptedCookies);
        result.HasAcceptedCookies.Should().BeTrue();
    }

    [Fact]
    public async Task SignInUserAsync_SignsOutDifferentUser_WhenAuthMismatch()
    {
        var user = UserAcceptedCookies;
        HttpContext httpContext = GetHttpContextWithUserId(user.Id, out var authService);

        var otherIdentity = new ClaimsIdentity(
            [new Claim(Constants.IdClaim, Guid.NewGuid().ToString())], Constants.AuthScheme);
        var otherPrincipal = new ClaimsPrincipal(otherIdentity);

        A.CallTo(() => authService.AuthenticateAsync(httpContext, Constants.AuthScheme))
            .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(otherPrincipal, Constants.AuthScheme))));

        var repo = GetUserRepoWithUser(user);
        var sut = new AuthService(repo, Logger);

        await sut.SignInUserAsync(httpContext, acceptedCookies: true, CancellationToken.None);

        A.CallTo(() => authService.SignOutAsync(httpContext, Constants.AuthScheme, A<AuthenticationProperties>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task SignInUserAsync_ReturnsNull_WhenClaimIdInvalidGuid()
    {
        var httpContext = A.Fake<HttpContext>();
        var identity = new ClaimsIdentity([new Claim(Constants.IdClaim, "not-a-guid")], Constants.AuthScheme);
        A.CallTo(() => httpContext.User).Returns(new ClaimsPrincipal(identity));

        var sut = new AuthService(GetUserRepoWithUser(null!), Logger);

        // should throw an exception
        Func<Task> act = async () => await sut.SignInUserAsync(httpContext, acceptedCookies: true, CancellationToken.None);
        await act.Should().ThrowAsync<AnUserMissingIdClaimException>();
    }

    [Fact]
    public async Task SignInUserAsync_ReturnsNull_WhenCookiesRequiredAndUserDidNotAccept()
    {
        var user = UserAcceptedCookies;
        user.AcceptedCookiesAt = default; // Simulate that the user has not accepted cookies
        var repo = GetUserRepoWithUser(user);
        HttpContext httpContext = GetHttpContextWithUserId(user.Id, out _);

        var sut = new AuthService(repo, Logger);

        var result = await sut.SignInUserAsync(httpContext, acceptedCookies: false, CancellationToken.None);

        result.Should().BeNull();
        httpContext.User.Claims.Should().NotContain(c => c.Type == Constants.IdClaim);
    }
}

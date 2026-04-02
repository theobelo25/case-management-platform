using CaseManagement.Api.Common.Exceptions;
using CaseManagement.Api.Features.Auth.Contracts;
using CaseManagement.Api.Infrastructure.Authentication;
using CaseManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Api.Features.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);
        
        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var isValidPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = _jwtTokenService.CreateAccessToken(
            user.Id,
            user.Email,
            user.FullName);
        
        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName
        };
    }

    public async Task<MeResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);
        
        if (user is null)
        {
            throw new UnauthorizedException("User not found.");
        }

        return new MeResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName
        };
    }
}
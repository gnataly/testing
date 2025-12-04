using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.AspNetCore.Mvc.Testing;
using TheatreCenter.DTOs;
using TheatreCenter.DTOs.Auth;

namespace TheatreCenter.E2ETests;

public class AuthFlowFeature : FeatureFixture, IAsyncLifetime
{
    private readonly MailInboxClient _mailClient = new();
    private TestApplicationFactory? _factory;
    private HttpClient? _client;
    private AuthResponseDto? _loginResponse;
    private AuthResponseDto? _verifiedResponse;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _newPassword = string.Empty;
    private string _originalPassword = string.Empty;
    private string _twoFactorCode = string.Empty;
    private int _accountId;
    private string _jwt = string.Empty;
    private DateTime _timestamp;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    [Scenario]
    public async Task Two_factor_login_from_email()
    {
        try
        {
            await Runner.RunScenarioAsync(
                _ => Given_application_is_running(),
                _ => Given_test_user_credentials(),
                _ => When_user_registers(),
                _ => When_user_requests_login(),
                _ => Then_two_factor_challenge_is_returned(),
                _ => And_email_contains_two_factor_code(),
                _ => When_user_confirms_two_factor_code(),
                _ => Then_jwt_token_is_returned(),
                _ => And_user_is_removed());
        }
        finally
        {
            await CleanupAsync();
        }
    }

    [Scenario]
    public async Task Account_is_locked_after_wrong_codes_and_can_be_unlocked()
    {
        try
        {
            await Runner.RunScenarioAsync(
                _ => Given_application_is_running(),
                _ => Given_test_user_credentials(),
                _ => When_user_registers(),
                _ => When_user_requests_login(),
                _ => Then_two_factor_challenge_is_returned(),
                _ => When_user_submits_wrong_codes_until_locked(),
                _ => When_user_requests_unlock_code(),
                _ => Then_unlock_code_arrives(),
                _ => When_user_verifies_unlock_code(),
                _ => When_user_requests_login(),
                _ => Then_two_factor_challenge_is_returned(),
                _ => And_email_contains_two_factor_code(),
                _ => When_user_confirms_two_factor_code(),
                _ => Then_jwt_token_is_returned(),
                _ => And_user_is_removed());
        }
        finally
        {
            await CleanupAsync();
        }
    }

    [Scenario]
    public async Task Password_change_requires_relogin_with_new_password()
    {
        try
        {
            await Runner.RunScenarioAsync(
                _ => Given_application_is_running(),
                _ => Given_test_user_credentials(),
                _ => When_user_registers(),
                _ => When_user_requests_login(),
                _ => Then_two_factor_challenge_is_returned(),
                _ => And_email_contains_two_factor_code(),
                _ => When_user_confirms_two_factor_code(),
                _ => When_user_changes_password(),
                _ => Then_login_with_old_password_is_rejected(),
                _ => Then_login_with_new_password_succeeds(),
                _ => And_user_is_removed());
        }
        finally
        {
            await CleanupAsync();
        }
    }

    private Task Given_application_is_running()
    {
        _factory = new TestApplicationFactory();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _timestamp = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    private Task Given_test_user_credentials()
    {
        _username = Environment.GetEnvironmentVariable("E2E_MAIL_USERNAME") ?? "testnataly@mail.ru";
        _password = $"P@ss{Guid.NewGuid():N}".Substring(0, 12);
        _newPassword = $"N3w{Guid.NewGuid():N}".Substring(0, 12);
        _originalPassword = _password;
        return Task.CompletedTask;
    }

    private async Task When_user_registers()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new AuthRequestDto
        {
            Username = _username,
            PasswordHash = _password
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        payload.Should().NotBeNull();
        _accountId = payload.Account.Id;
    }

    private async Task When_user_requests_login()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        _timestamp = DateTime.UtcNow;
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new AuthRequestDto
        {
            Username = _username,
            PasswordHash = _password
        });

        response.EnsureSuccessStatusCode();
        _loginResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
    }

    private Task Then_two_factor_challenge_is_returned()
    {
        _loginResponse.Should().NotBeNull();
        _loginResponse!.RequiresTwoFactor.Should().BeTrue();
        _loginResponse.TwoFactorChallengeId.Should().NotBeNullOrWhiteSpace();
        _loginResponse.TwoFactorExpiresAt.Should().NotBeNull();
        return Task.CompletedTask;
    }

    private async Task And_email_contains_two_factor_code()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        _twoFactorCode = await _mailClient.WaitForCodeAsync("verification code", _timestamp, cts.Token);
    }

    private async Task When_user_confirms_two_factor_code()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        var response = await client.PostAsJsonAsync("/api/v1/auth/verify-2fa", new TwoFactorVerifyRequestDto
        {
            Username = _username,
            ChallengeId = _loginResponse!.TwoFactorChallengeId,
            Code = _twoFactorCode
        });

        response.EnsureSuccessStatusCode();
        _verifiedResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        _accountId = _verifiedResponse!.Account.Id;
        _jwt = _verifiedResponse.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwt);
    }

    private Task Then_jwt_token_is_returned()
    {
        _verifiedResponse.Should().NotBeNull();
        _verifiedResponse!.Token.Should().NotBeNullOrWhiteSpace();
        _verifiedResponse.RequiresTwoFactor.Should().BeFalse();
        return Task.CompletedTask;
    }

    private async Task When_user_submits_wrong_codes_until_locked()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        var challengeId = _loginResponse?.TwoFactorChallengeId ?? throw new InvalidOperationException("Challenge is missing.");
        for (var i = 0; i < 6; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/verify-2fa", new TwoFactorVerifyRequestDto
            {
                Username = _username,
                ChallengeId = challengeId,
                Code = "000000"
            });

            if (response.StatusCode == System.Net.HttpStatusCode.Locked)
            {
                break;
            }
        }

        var lockedResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-2fa", new TwoFactorVerifyRequestDto
        {
            Username = _username,
            ChallengeId = challengeId,
            Code = "000000"
        });

        lockedResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Locked);
    }

    private async Task When_user_requests_unlock_code()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        _timestamp = DateTime.UtcNow;
        var response = await client.PostAsJsonAsync("/api/v1/auth/unlock/request", new UnlockRequestDto
        {
            Username = _username
        });

        response.EnsureSuccessStatusCode();
    }

    private async Task Then_unlock_code_arrives()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        _twoFactorCode = await _mailClient.WaitForCodeAsync("unlock code", _timestamp, cts.Token);
    }

    private async Task When_user_verifies_unlock_code()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        var response = await client.PostAsJsonAsync("/api/v1/auth/unlock/verify", new UnlockVerifyRequestDto
        {
            Username = _username,
            Code = _twoFactorCode
        });

        response.EnsureSuccessStatusCode();
    }

    private async Task When_user_changes_password()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        var response = await client.PostAsJsonAsync("/api/v1/auth/change-password", new ChangePasswordRequestDto
        {
            Username = _username,
            CurrentPasswordHash = _password,
            NewPasswordHash = _newPassword
        });

        response.EnsureSuccessStatusCode();
        _password = _newPassword;
    }

    private async Task Then_login_with_old_password_is_rejected()
    {
        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new AuthRequestDto
        {
            Username = _username,
            PasswordHash = _originalPassword
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    private async Task Then_login_with_new_password_succeeds()
    {
        await When_user_requests_login();
        await Then_two_factor_challenge_is_returned();
        await And_email_contains_two_factor_code();
        await When_user_confirms_two_factor_code();
        await Then_jwt_token_is_returned();
    }

    private async Task And_user_is_removed()
    {
        if (_accountId == 0 || string.IsNullOrWhiteSpace(_jwt))
        {
            return;
        }

        var client = _client ?? throw new InvalidOperationException("HttpClient is not initialized.");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwt);
        var response = await client.DeleteAsync($"/api/v1/accounts/{_accountId}");
        response.EnsureSuccessStatusCode();
    }

    private async Task CleanupAsync()
    {
        try
        {
            await And_user_is_removed();
        }
        catch
        {
            // ignore cleanup failures
        }
        finally
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
    }
}

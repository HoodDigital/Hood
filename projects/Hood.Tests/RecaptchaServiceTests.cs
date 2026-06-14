using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.RecaptchaEnterprise.V1;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Decision logic for the Fraud Defence (reCAPTCHA Enterprise) assessment flow.
    /// The Google call itself is faked via IRecaptchaAssessmentClient; these tests cover how
    /// the service maps an assessment onto RecaptchaResponse.Passed.
    /// </summary>
    public class RecaptchaServiceTests
    {
        private class FakeAssessmentClient : IRecaptchaAssessmentClient
        {
            private readonly Assessment _assessment;
            public Event LastEvent { get; private set; }

            public FakeAssessmentClient(Assessment assessment)
            {
                _assessment = assessment;
            }

            public Task<Assessment> CreateAssessmentAsync(
                string projectId,
                string apiKey,
                Event assessmentEvent
            )
            {
                LastEvent = assessmentEvent;
                return Task.FromResult(_assessment);
            }
        }

        private class ThrowingAssessmentClient : IRecaptchaAssessmentClient
        {
            public Task<Assessment> CreateAssessmentAsync(
                string projectId,
                string apiKey,
                Event assessmentEvent
            ) => throw new InvalidOperationException("API key not valid.");
        }

        private static IntegrationSettings ConfiguredSettings(decimal threshold = 0.5m)
        {
            return new IntegrationSettings
            {
                EnableGoogleRecaptcha = true,
                GoogleRecaptchaSiteKey = "site-key",
                GoogleRecaptchaProjectId = "my-project",
                GoogleCloudApiKey = "api-key",
                GoogleRecaptchaThreshold = threshold,
            };
        }

        private static Assessment GoodAssessment(
            float score = 0.9f,
            string action = "login",
            string hostname = "example.com",
            bool valid = true
        )
        {
            return new Assessment
            {
                TokenProperties = new TokenProperties
                {
                    Valid = valid,
                    Action = action,
                    Hostname = hostname,
                },
                RiskAnalysis = new RiskAnalysis { Score = score },
            };
        }

        private static HttpRequest RequestWithToken(string host = "example.com")
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Host = new HostString(host);
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.Form = new FormCollection(
                new Dictionary<string, StringValues> { ["g-recaptcha-response"] = "a-token" }
            );
            return context.Request;
        }

        private class FakeLogService : ILogService
        {
            public List<(string Message, LogType Type)> Entries { get; } = new();

            public Task AddLogAsync<TSource>(
                string message,
                object logObject = null,
                LogType type = LogType.Info
            )
            {
                Entries.Add((message, type));
                return Task.CompletedTask;
            }

            public Task AddExceptionAsync<TSource>(
                string message,
                Exception ex,
                LogType type = LogType.Error
            ) => Task.CompletedTask;

            public Task AddExceptionAsync<TSource>(
                string message,
                object logObject,
                Exception ex,
                LogType type = LogType.Error
            ) => Task.CompletedTask;
        }

        private static RecaptchaService Service(Assessment assessment, IntegrationSettings settings)
        {
            return new RecaptchaService(new FakeAssessmentClient(assessment), () => settings);
        }

        [Fact]
        public async Task Not_configured_short_circuits_to_passed()
        {
            // No keys set — the check must pass so forms aren't blocked on unconfigured sites.
            RecaptchaService service = Service(GoodAssessment(), new IntegrationSettings());
            RecaptchaResponse response = await service.Validate(RequestWithToken());
            Assert.True(response.Passed);
        }

        [Fact]
        public async Task Invalid_token_fails()
        {
            RecaptchaService service = Service(GoodAssessment(valid: false), ConfiguredSettings());
            RecaptchaResponse response = await service.Validate(RequestWithToken());
            Assert.False(response.Passed);
            Assert.Contains("invalid", response.Message);
        }

        [Fact]
        public async Task Action_mismatch_fails_when_expected_action_supplied()
        {
            RecaptchaService service = Service(
                GoodAssessment(action: "login"),
                ConfiguredSettings()
            );
            RecaptchaResponse response = await service.Validate(RequestWithToken(), "register");
            Assert.False(response.Passed);
            Assert.Contains("action", response.Message);
        }

        [Fact]
        public async Task Action_not_checked_when_no_expected_action()
        {
            RecaptchaService service = Service(
                GoodAssessment(action: "anything"),
                ConfiguredSettings()
            );
            RecaptchaResponse response = await service.Validate(RequestWithToken());
            Assert.True(response.Passed);
        }

        [Fact]
        public async Task Hostname_mismatch_fails()
        {
            RecaptchaService service = Service(
                GoodAssessment(hostname: "evil.com"),
                ConfiguredSettings()
            );
            RecaptchaResponse response = await service.Validate(RequestWithToken());
            Assert.False(response.Passed);
            Assert.Contains("host", response.Message);
        }

        [Fact]
        public async Task Score_under_threshold_fails()
        {
            RecaptchaService service = Service(
                GoodAssessment(score: 0.2f),
                ConfiguredSettings(threshold: 0.5m)
            );
            RecaptchaResponse response = await service.Validate(RequestWithToken());
            Assert.False(response.Passed);
            Assert.Contains("threshold", response.Message);
        }

        [Fact]
        public async Task Happy_path_passes_and_sends_expected_action_to_google()
        {
            FakeAssessmentClient client = new FakeAssessmentClient(GoodAssessment());
            RecaptchaService service = new RecaptchaService(client, () => ConfiguredSettings());
            RecaptchaResponse response = await service.Validate(RequestWithToken(), "login");
            Assert.True(response.Passed);
            Assert.Equal("a-token", client.LastEvent.Token);
            Assert.Equal("site-key", client.LastEvent.SiteKey);
            Assert.Equal("login", client.LastEvent.ExpectedAction);
        }

        [Fact]
        public async Task Pass_logs_at_info()
        {
            FakeLogService logs = new FakeLogService();
            RecaptchaService service = new RecaptchaService(
                new FakeAssessmentClient(GoodAssessment()),
                () => ConfiguredSettings(),
                logs
            );
            await service.Validate(RequestWithToken(), "login");
            Assert.Contains(
                logs.Entries,
                e => e.Type == LogType.Info && e.Message.Contains("passed")
            );
        }

        [Fact]
        public async Task Failure_logs_at_warning()
        {
            FakeLogService logs = new FakeLogService();
            RecaptchaService service = new RecaptchaService(
                new FakeAssessmentClient(GoodAssessment(score: 0.2f)),
                () => ConfiguredSettings(threshold: 0.5m),
                logs
            );
            await service.Validate(RequestWithToken());
            Assert.Contains(
                logs.Entries,
                e => e.Type == LogType.Warning && e.Message.Contains("failed")
            );
        }

        [Fact]
        public async Task Assessment_error_fails_closed_and_logs_at_error()
        {
            FakeLogService logs = new FakeLogService();
            RecaptchaService service = new RecaptchaService(
                new ThrowingAssessmentClient(),
                () => ConfiguredSettings(),
                logs
            );
            RecaptchaResponse response = await service.Validate(RequestWithToken(), "login");
            Assert.False(response.Passed);
            Assert.Contains(logs.Entries, e => e.Type == LogType.Error);
        }

        [Fact]
        public async Task Missing_form_token_fails()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Host = new HostString("example.com");
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.Form = new FormCollection(new Dictionary<string, StringValues>());

            RecaptchaService service = Service(GoodAssessment(), ConfiguredSettings());
            RecaptchaResponse response = await service.Validate(context.Request);
            Assert.False(response.Passed);
        }
    }
}

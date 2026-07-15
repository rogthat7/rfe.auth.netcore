using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RFE.Auth.API.Controllers;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Tests
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<IJwtAuthenticationService> _jwtAuthServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<ISmsSender> _smsSenderMock;
        private Mock<IOptions<JwtOptions>> _jwtOptionsMock;
        private UserController _controller;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
            _jwtAuthServiceMock = new Mock<IJwtAuthenticationService>();
            _mapperMock = new Mock<IMapper>();
            _emailSenderMock = new Mock<IEmailSender>();
            _smsSenderMock = new Mock<ISmsSender>();
            _jwtOptionsMock = new Mock<IOptions<JwtOptions>>();

            var jwtOptions = new JwtOptions
            {
                Secret = "SuperSecretKeyForTestingThatIsLongEnoughToNotThrowErrors",
                Issuer = "rfe.auth.service",
                JwtKeyForEmail = "SuperSecretKeyForEmailTestingConfirmation"
            };
            _jwtOptionsMock.Setup(o => o.Value).Returns(jwtOptions);

            _userServiceMock
                .Setup(s => s.GetAllRegisteredUsers())
                .ReturnsAsync(new List<AuthUserByIdGetResponse>());

            _controller = new UserController(
                _userServiceMock.Object,
                _jwtAuthServiceMock.Object,
                _mapperMock.Object,
                _emailSenderMock.Object,
                _smsSenderMock.Object,
                _jwtOptionsMock.Object
            );
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }

        [Test]
        public async Task Login_WithValidUser_ReturnsOk()
        {
            // Arrange
            var request = new LoginRequestModel
            {
                Username = "testuser",
                Password = "Password123"
            };

            var user = new AuthUser
            {
                UserId = 1,
                Username = "testuser",
                IsVerified = true
            };
            var token = new Token { value = "mock-jwt-token" };
            var authResponse = new AuthenticateResponse(user, token);

            _jwtAuthServiceMock
                .Setup(s => s.Authenticate(It.IsAny<AuthenticateRequest>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result;
            var success = GetPropertyValue(okResult.Value, "success");
            Assert.That(success, Is.EqualTo(true));
        }

        [Test]
        public async Task Login_WithUnverifiedUser_ReturnsForbidden()
        {
            // Arrange
            var request = new LoginRequestModel
            {
                Username = "unverifieduser",
                Password = "Password123"
            };

            var user = new AuthUser
            {
                UserId = 2,
                Username = "unverifieduser",
                IsVerified = false
            };
            var token = new Token { value = "mock-jwt-token" };
            var authResponse = new AuthenticateResponse(user, token);

            _jwtAuthServiceMock
                .Setup(s => s.Authenticate(It.IsAny<AuthenticateRequest>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
            var verified = GetPropertyValue(objectResult.Value, "verified");
            Assert.That(verified, Is.EqualTo(false));
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestModel
            {
                Username = "nonexistentuser",
                Password = "Password123"
            };

            _jwtAuthServiceMock
                .Setup(s => s.Authenticate(It.IsAny<AuthenticateRequest>()))
                .ReturnsAsync((AuthenticateResponse)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task Register_WithPhone_ReturnsOk_WithVerificationMethodPhone()
        {
            // Arrange
            var request = new RegisterRequestModel
            {
                Phone = "9876543210",
                Password = "Password123"
            };

            _mapperMock
                .Setup(m => m.Map<AuthUser>(It.IsAny<AuthUserAddPostRequestDto>()))
                .Returns(new AuthUser { Username = "9876543210", Phone = 9876543210 });

            _smsSenderMock
                .Setup(s => s.SendUserConfirmationSms(It.IsAny<AuthUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result;
            var verificationMethod = GetPropertyValue(okResult.Value, "verificationMethod");
            Assert.That(verificationMethod, Is.EqualTo("phone"));
        }

        [Test]
        public async Task Register_WithEmailOnly_ReturnsOk_WithVerificationMethodEmail()
        {
            // Arrange
            var request = new RegisterRequestModel
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            _mapperMock
                .Setup(m => m.Map<AuthUser>(It.IsAny<AuthUserAddPostRequestDto>()))
                .Returns(new AuthUser { Username = "test@example.com", Email = "test@example.com" });

            _emailSenderMock
                .Setup(s => s.SendUserConfirmationEmail(It.IsAny<AuthUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result;
            var verificationMethod = GetPropertyValue(okResult.Value, "verificationMethod");
            Assert.That(verificationMethod, Is.EqualTo("email"));
        }
    }
}

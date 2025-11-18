using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartCareerPath.APIs.Controllers.Auth;
using SmartCareerPath.Application.Abstraction.DTOs.RequestDTOs;
using SmartCareerPath.Application.Abstraction.DTOs.ResponseDTOs;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Auth;
using SmartCareerPath.Domain.Common.ResultPattern;
using Xunit;

namespace SmartCareerPath.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_Returns_BadRequest_When_ModelStateInvalid()
        {
            var mockService = new Mock<IAuthService>();
            var controller = new AuthController(mockService.Object);
            controller.ModelState.AddModelError("Email", "Required");

            var result = await controller.Register(new RegisterRequestDTO());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_Returns_BadRequest_When_Service_Fails()
        {
            var mockService = new Mock<IAuthService>();
            mockService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDTO>()))
                .ReturnsAsync(Result<AuthResponseDTO>.Failure("Error"));

            var controller = new AuthController(mockService.Object);

            var result = await controller.Register(new RegisterRequestDTO());

            var badReq = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("error", badReq.Value.ToString());
        }

        [Fact]
        public async Task Register_Returns_Ok_When_Service_Succeeds()
        {
            var mockService = new Mock<IAuthService>();
            var response = new AuthResponseDTO { Email = "a@b.com", Token = "t" };
            mockService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDTO>()))
                .ReturnsAsync(Result<AuthResponseDTO>.Success(response));

            var controller = new AuthController(mockService.Object);

            var result = await controller.Register(new RegisterRequestDTO());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, ok.Value);
        }
    }
}

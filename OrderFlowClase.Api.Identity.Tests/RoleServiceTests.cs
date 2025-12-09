using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using OrderFlowClase.API.Identity.Services;

namespace OrderFlowClase.Api.Identity.Tests
{
    [TestFixture]
    public class RoleServiceTests
    {
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private RoleService _roleService;

        [SetUp]
        public void Setup()
        {
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null!, null!, null!, null!);

            _roleService = new RoleService(_mockRoleManager.Object);
        }

        #region CreateRoleAsync Tests

        [Test]
        public async Task CreateRoleAsync_WhenRoleDoesNotExist_ShouldCreateAndReturnTrue()
        {
            // Arrange
            var roleName = "Admin";

            _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
                .ReturnsAsync(false);

            _mockRoleManager.Setup(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == roleName)))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _roleService.CreateRoleAsync(roleName);

            // Assert
            Assert.That(result, Is.True);
            _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == roleName)), Times.Once);
        }

        [Test]
        public async Task CreateRoleAsync_WhenRoleAlreadyExists_ShouldReturnFalse()
        {
            // Arrange
            var roleName = "Admin";

            _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
                .ReturnsAsync(true);

            // Act
            var result = await _roleService.CreateRoleAsync(roleName);

            // Assert
            Assert.That(result, Is.False);
            _mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        }

        [Test]
        public async Task CreateRoleAsync_WhenCreationFails_ShouldReturnFalse()
        {
            // Arrange
            var roleName = "Admin";

            _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
                .ReturnsAsync(false);

            _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            // Act
            var result = await _roleService.CreateRoleAsync(roleName);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region RoleExistsAsync Tests

        [Test]
        public async Task RoleExistsAsync_WhenRoleExists_ShouldReturnTrue()
        {
            // Arrange
            var roleName = "Admin";

            _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
                .ReturnsAsync(true);

            // Act
            var result = await _roleService.RoleExistsAsync(roleName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task RoleExistsAsync_WhenRoleDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var roleName = "NonExistent";

            _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
                .ReturnsAsync(false);

            // Act
            var result = await _roleService.RoleExistsAsync(roleName);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region EnsureRolesCreatedAsync Tests

        [Test]
        public async Task EnsureRolesCreatedAsync_WithNoExistingRoles_ShouldCreateAllRoles()
        {
            // Arrange
            var roles = new[] { "Admin", "Customer" };

            _mockRoleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _roleService.EnsureRolesCreatedAsync(roles);

            // Assert
            _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Admin")), Times.Once);
            _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Customer")), Times.Once);
        }

        [Test]
        public async Task EnsureRolesCreatedAsync_WithAllExistingRoles_ShouldNotCreateAnyRoles()
        {
            // Arrange
            var roles = new[] { "Admin", "Customer" };

            _mockRoleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            await _roleService.EnsureRolesCreatedAsync(roles);

            // Assert
            _mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        }

        [Test]
        public async Task EnsureRolesCreatedAsync_WithSomeExistingRoles_ShouldOnlyCreateMissingRoles()
        {
            // Arrange
            var roles = new[] { "Admin", "Customer" };

            _mockRoleManager.Setup(x => x.RoleExistsAsync("Admin"))
                .ReturnsAsync(true);

            _mockRoleManager.Setup(x => x.RoleExistsAsync("Customer"))
                .ReturnsAsync(false);

            _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _roleService.EnsureRolesCreatedAsync(roles);

            // Assert
            _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Admin")), Times.Never);
            _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Customer")), Times.Once);
        }

        [Test]
        public async Task EnsureRolesCreatedAsync_WithEmptyArray_ShouldNotCreateAnyRoles()
        {
            // Arrange
            var roles = Array.Empty<string>();

            // Act
            await _roleService.EnsureRolesCreatedAsync(roles);

            // Assert
            _mockRoleManager.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Never);
            _mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        }

        #endregion

        #region GetAllRolesAsync Tests

        [Test]
        public async Task GetAllRolesAsync_WhenRolesExist_ShouldReturnAllRoleNames()
        {
            // Arrange
            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "Customer" }
            };

            var mockRoles = roles.BuildMock();
            _mockRoleManager.Setup(x => x.Roles).Returns(mockRoles);

            // Act
            var result = await _roleService.GetAllRolesAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result, Does.Contain("Admin"));
            Assert.That(result, Does.Contain("Customer"));
        }

        [Test]
        public async Task GetAllRolesAsync_WhenNoRolesExist_ShouldReturnEmptyList()
        {
            // Arrange
            var roles = new List<IdentityRole>();

            var mockRoles = roles.BuildMock();
            _mockRoleManager.Setup(x => x.Roles).Returns(mockRoles);

            // Act
            var result = await _roleService.GetAllRolesAsync();

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion
    }
}

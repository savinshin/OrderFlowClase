using Microsoft.AspNetCore.Identity;

namespace OrderFlowClase.API.Identity.Services
{
    public interface IRoleService
    {
        Task<bool> CreateRoleAsync(string roleName);
        Task<bool> RoleExistsAsync(string roleName);
        Task EnsureRolesCreatedAsync(params string[] roleNames);
        Task<IEnumerable<string>> GetAllRolesAsync();
    }
}

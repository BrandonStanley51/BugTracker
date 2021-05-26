using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices.Interfaces
{
    public interface IBTRolesService
    {
        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName);
        public Task<IEnumerable<string>> ListUserRolesAsync(BTUser user);
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);
        public Task<List<BTUser>> UsersNotInRoleAsync(string roleName);
        public Task<string> GetRoleNameByIdAsync(string roleName);
    }
}

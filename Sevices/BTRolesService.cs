using BugTracker.Models;
using BugTracker.Sevices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices
{
    public class BTRolesService : IBTRolesService
    {
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameByIdAsync(string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ListUserRolesAsync(BTUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> UsersNotInRoleAsync(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}

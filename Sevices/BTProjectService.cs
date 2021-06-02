using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Sevices.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public BTProjectService(UserManager<BTUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            var proj = _context.Project.Where(t => t.Id == projectId);

            return await AddProjectManagerAsync(userId, projectId); 
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            try 
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
                    if (!await IsUserOnProject(userId, projectId))
                    {
                        try
                        {
                            project.Members.Add(user);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
              }
            catch (Exception Ex)
            {
                Debug.WriteLine($"*** Error *** - Error Adding User To Project. --> {Ex.Message}");
            }
        }

        public async Task<List<BTUser>> DevelopersOnProjectAsync(int projectId)
        {
            Project project = await _context.Project    
                                .Include(project => project.Members)
        }

        public Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BTUser>> GetMembersWithoutPMAsync(int projectId)
        {
            List<BTUser> developers = await DevelopersOnProjectAsync(projectId);
            List<BTUser> submitters = await SubmittersOnProjectAsync(projectId);
            List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, "Admin");

            List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();
            return teamMembers;
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Project
                                .Include(project => project.Members)
                                .FirstOrDefaultAsync(u => u.Id == projectId);
            List<BTUser> members = new();
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsUserOnProject(string userId, int projectId)
        {
            Project project = await _context.Project.FirstOrDefaultAsync(u => u.Id == projectId);

            bool result = project.Members.Any(u => u.Id == userId);
            return result;
        }

        public async Task<List<Project>> ListUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> userProjects = (await _context.Users
                                                .Include(u => u.Projects)
                                                    .ThenInclude(p => p.Company)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(p => p.Members)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(p => p.Tickets)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.DeveloperUser)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.OwnerUser)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketPriorityId)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketStatus)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketType)
                                                .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();
                return userProjects;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error Getting user projects list. -->{ex.Message}"); 
                    throw;
            }
        }

        public Task RemoveProjectManagerAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);

                if (await IsUserOnProject(userId, projectId))
                {
                    try
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** Error *** - Error Adding user to project. --> {ex.Message}");
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (BTUser btUser in members)
                {
                    try
                    {
                        project.Members.Remove(btUser);
                        await _context.SaveChangesAsync();
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                }

            }
            catch(Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error Removing users from project.  --> {ex.Message}");
            }
        }

        public Task<List<BTUser>> SubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<BTUser> users = await _context.Users.Where(u=> u.Projects.All...........)
        }
    }
}

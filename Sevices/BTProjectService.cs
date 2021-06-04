using BugTracker.Controllers;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Sevices.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IBTRolesService _roleService;
        private readonly ILogger<HomeController> _logger;


        public BTProjectService(UserManager<BTUser> userManager, ApplicationDbContext context, IBTRolesService roleService, ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _context = context;
            _roleService = roleService;
            _logger = logger;
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BTUser currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error removing current PM. - Error: {ex.Message}");
                    return false;
                }
            }
            try
            {
                await AddUserToProjectAsync(userId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error adding new PM. - Error: {ex.Message}");
                return false;
            }
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
            return false;
        }

        public async Task<List<BTUser>> DevelopersOnProjectAsync(int projectId)
        {
            Project project = await _context.Project
                                .Include(p => p.Members)
                                .FirstOrDefaultAsync(u => u.Id == projectId);
            List<BTUser> developers = new();

            foreach (var user in project.Members)
            {
                if (await _roleService.IsUserInRoleAsync(user, "Developer"))
                {
                    developers.Add(user);
                }
            }
            return developers;
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            List<Project> projects = new();
            projects = await _context.Project.Include(p => p.Company)
                                             .Include(p => p.Members)
                                             .Include(p => p.ProjectPriority)
                                             .Where(p => p.CompanyId == companyId)
                                             .ToListAsync();

            return projects;
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = new();
            projects = await GetAllProjectsByCompany(companyId);
            return projects;
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = new();
            projects = await GetAllProjectsByCompany(companyId);
            return projects.Where(p => p.Archived == true).ToList();
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
            foreach (BTUser member in project?.Members)
            {
                if (await _roleService.IsUserInRoleAsync(member, "ProjectManager"))
                {
                    return member;
                }
            }
            return null;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await _context.Project
                                .Include(p => p.Members)
                                .FirstOrDefaultAsync(u => u.Id == projectId);
            List<BTUser> members = new();

            foreach (var user in project.Members)
            {
                if (await _roleService.IsUserInRoleAsync(user, role))
                {
                    members.Add(user);
                }
            }
            return members;
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
                                                        .ThenInclude(t => t.TicketPriority)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketStatus)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketType)
                                                .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();
                return userProjects;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error Getting user projects list. -->{ex.Message}");
                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await _context.Project.Include(u=>u.Members).FirstOrDefaultAsync(p => p.Id == projectId);

            foreach(BTUser user in project.Members)
            {
                if(await _roleService.IsUserInRoleAsync(user, "ProjectManager"))
                {
                    await RemoveUserFromProjectAsync(user.Id, projectId); 
                }
            }

            
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
                    catch (Exception)
                    {
                        throw;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error Removing users from project.  --> {ex.Message}");
            }
        }

        public async Task<List<BTUser>> SubmittersOnProjectAsync(int projectId)
        {
            Project project = await _context.Project
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(u => u.Id == projectId);
            List<BTUser> submitters = new();

            foreach (var user in project.Members)
            {
                if (await _roleService.IsUserInRoleAsync(user, "Submitter"))
                {
                    submitters.Add(user);
                }
            }
            return submitters;
        }

        public async Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId)
        {
           // List<BTUser> usersNotOnProject = new();
          //  usersNotOnProject = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId) && u.CompanyId == companyId).ToList();


              //  return usersNotOnProject.ToList();
            

           List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId) && u.CompanyId == companyId).ToListAsync();
           return users;
        }

        public async Task<int> LookUpProjectPriorityId(string priorityName)
        {
            return (await _context.ProjectPriority.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
        }
    }
}

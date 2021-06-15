using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Sevices.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService  _projectService;

        public BTTicketService(ApplicationDbContext context, IBTProjectService projectService)
        {
            _context = context;
            _projectService = projectService;
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            Ticket ticket = await _context.Ticket.FirstOrDefaultAsync(t => t.Id == ticketId);
            
            if (ticket != null)
            {
                try
                {
                    ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                    ticket.DeveloperUserId = userId;
                    await _context.SaveChangesAsync();
                }
                catch(Exception)
                {
                    throw;
                }
            }
        }

        public async Task<List<Ticket>> GetAllPMTicketsAsync(string userId)
        {
            List<Project> projects = await _projectService.ListUserProjectsAsync(userId);
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();
            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> tickets = new();
            try
            {
                tickets = await _context.Project.Include(p => p.Company)
                                                        .Where(p=>p.CompanyId == companyId)
                                                                .SelectMany(p => p.Tickets)
                                                                .Include(t => t.Attachments)
                                                                .Include(t => t.Comments)
                                                                .Include(t => t.History)
                                                                .Include(t => t.DeveloperUser)
                                                                .Include(t => t.OwnerUser)
                                                                .Include(t => t.TicketPriority)
                                                                .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                .Include(t => t.Project)
                                                                .ToListAsync();
            }
            catch
            {
                throw;
            }
                return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;
            List<Ticket> tickets = new();
            try
            {
                tickets = await _context.Project.Where(p=> p.CompanyId == companyId)       
                                                    .SelectMany(p => p.Tickets)
                                                                .Include(t => t.Attachments)
                                                                .Include(t => t.Comments)
                                                                .Include(t => t.History)
                                                                .Include(t => t.DeveloperUser)
                                                                .Include(t => t.OwnerUser)
                                                                .Include(t => t.TicketPriority)
                                                                .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                .Include(t => t.Project)
                                                    .Where(t => t.TicketPriorityId == priorityId).ToListAsync();
            }
            catch
            {
                throw;
            }
            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId)
        {
            List<Ticket> tickets = new();
            if (string.Compare(role, Roles.Developer.ToString()) == 0)
            {
                tickets = await _context.Ticket
                                    .Include(t => t.Comments)
                                    .Include(t => t.History)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.OwnerUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.Members)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.ProjectPriority)
                                    .Where(t => t.DeveloperUserId == userId).ToListAsync();
            }
            else if (string.Compare(role, Roles.Submitter.ToString()) == 0)
            {
                tickets = await _context.Ticket
                                    .Include(t => t.Attachments)
                                    .Include(t => t.Comments)
                                    .Include(t => t.History)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.OwnerUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.Members)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.ProjectPriority)
                                    .Where(t => t.OwnerUserId == userId).ToListAsync();
            }
            else if (string.Compare(role, Roles.ProjectManager.ToString()) ==0)
            {
                tickets = await GetAllPMTicketsAsync(userId);
            }
            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;
            return await _context.Project.Where(p => p.CompanyId == companyId)
                                           .SelectMany(p => p.Tickets)
                                           .Where(t => t.TicketStatusId == statusId).ToListAsync();
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int company = (await LookupTicketStatusIdAsync(typeName)).Value;
            return await _context.Project.Where(p => p.CompanyId == companyId)
                                           .SelectMany(p => p.Tickets)
                                           .Where(t => t.TicketStatusId == companyId).ToListAsync();
        }

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Project.Include(p => p.Company)
                                                             .Where(p => p.CompanyId == companyId)
                                                             .SelectMany(p => p.Tickets)
                                                                .Include(t => t.Attachments)
                                                                .Include(t => t.Comments)
                                                                .Include(t => t.History)
                                                                .Include(t => t.DeveloperUser)
                                                                .Include(t => t.OwnerUser)
                                                                .Include(t => t.TicketPriority)
                                                                .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                .Include(t => t.Project)
                                                                .Where(t => t.Archived == true)
                                                                .ToListAsync();
                return tickets;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();
            tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName))
                .Where(t => t.ProjectId == projectId).ToList();
            return tickets;
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();
            tickets = (await GetAllTicketsByStatusAsync(companyId, priorityName))
                .Where(t => t.ProjectId == projectId).ToList();
            return tickets;
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();
            tickets = (await GetAllTicketsByTypeAsync(companyId, typeName))
                .Where(t => t.ProjectId == projectId).ToList();
            return tickets;
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            BTUser developer = new();

            Ticket ticket = await _context.Ticket.Include(t => t.DeveloperUser).FirstOrDefaultAsync(t=>t.Id == ticketId);
            if (ticket?.DeveloperUserId != null)
            {
                developer = ticket.DeveloperUser;
            }
            return developer;
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                TicketPriority priority = await _context.TicketPriority.FirstOrDefaultAsync(p => p.Name == priorityName);
                return priority?.Id;
            }
            catch
            {
                throw;
            }
            
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatus.FirstOrDefaultAsync(p => p.Name == statusName);
                return status?.Id;
            }
            catch
            {
                throw;
            }
            
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketType.FirstOrDefaultAsync(p => p.Name == typeName);
                return type?.Id;
            }
            catch
            {
                throw;
            }
        }
    }
}

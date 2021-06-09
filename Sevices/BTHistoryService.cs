using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Sevices.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices
{
    public class BTHistoryService : IBTHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            if(oldTicket == null && newTicket != null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    Property = "",
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = "New Ticket Created"
                };
                await _context.TicketHistory.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            else
            {
                if(oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket title: {newTicket.Title}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket description: {newTicket.Description}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketTypeId",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket type: {newTicket.TicketType.Name}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Priority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket priority: {newTicket.TicketPriority.Name}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Status",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket Status: {newTicket.TicketStatus.Name}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser?.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket Developer: {newTicket.DeveloperUser.FullName}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketHistoriesAsync(int companyId)
        {
            Company company = await _context.Company
                                            .Include(c => c.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.History)
                                            .FirstOrDefaultAsync(c => c.Id == companyId);
            List<Ticket> tickets = company.Projects.SelectMany(p => p.Tickets).ToList(); 
            List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();

            return ticketHistories;
                                
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId)
        {
            Project project = await _context.Project
                                            .Include(p => p.Tickets)
                                               .ThenInclude(t => t.History)
                                            .FirstOrDefaultAsync(p => p.Id == projectId);
            List<TicketHistory> ticketHistory = project.Tickets.SelectMany(t => t.History).ToList();

            return ticketHistory;
        }
    }
}

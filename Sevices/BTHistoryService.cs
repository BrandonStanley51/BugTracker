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
        public Task AddHistory(BTTicketService oldTicket, BTTicketService newTicket, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TicketHistory>> GetCompanyTicketHistories(int companyId)
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

        public async Task<List<TicketHistory>> GetProjectTicketsHistories(int projectId)
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

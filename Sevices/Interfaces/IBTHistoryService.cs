using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices.Interfaces
{
    public interface IBTHistoryService
    {
        Task AddHistory(BTTicketService oldTicket, BTTicketService newTicket, string userId);
        Task<List<TicketHistory>> GetProjectTicketsHistories(int projectId);
        Task<List<TicketHistory>> GetCompanyTicketHistories(int companyId);
    }
}

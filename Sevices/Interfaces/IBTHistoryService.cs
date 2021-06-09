﻿using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices.Interfaces
{
    public interface IBTHistoryService
    {
        Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);
        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId);
        Task<List<TicketHistory>> GetCompanyTicketHistoriesAsync(int companyId);
    }
}

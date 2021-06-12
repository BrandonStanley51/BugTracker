using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Sevices.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companyInfoService;

        public BTNotificationService(ApplicationDbContext context, IEmailSender emailSender, IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _emailSender = emailSender;
            _companyInfoService = companyInfoService;
        }

        public async Task AdminsNotificationAsync(Notification notification, int companyId)
        {
            try
            {
                List<BTUser> admins = await _companyInfoService.GetMembersInRoleAsync("Admin", companyId);

                foreach (BTUser btUser in admins)
                {
                    notification.RecipientId = btUser.Id;

                    //await SaveNotificationAsync(notification);
                    await EmailNotificationAsync(notification, notification.Title);

                    //await SmsNotificationAsync("", notification);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task EmailNotificationAsync(Notification notification, string emailSubject)
        {
            BTUser btUser = await _context.Users.FindAsync(notification.RecipientId);

            //send email
            string btUserEmail = btUser.Email;
            string message = notification.Message;
            try
            {
                await _emailSender.SendEmailAsync(btUserEmail, emailSubject, message);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationAsync(string userId)
        {
            List<Notification> notifications = await _context.Notification
                                            .Include(n => n.Recipient)
                                            .Include(n => n.Sender)
                                            .Include(n => n.Ticket)
                                            .ThenInclude(n => n.Project)
                                            .Where(n => n.RecipientId == userId).ToListAsync();
            return notifications;
        }

        public async Task<List<Notification>> GetSentNotificationAsync(string userId)
        {
            {
                List<Notification> notifications = await _context.Notification
                                                .Include(n => n.Recipient)
                                                .Include(n => n.Sender)
                                                .Include(n => n.Ticket)
                                                .ThenInclude(n => n.Project)
                                                .Where(n => n.SenderId == userId).ToListAsync();
                return notifications;
            }
        }

        public async Task MembersNotificationAsync(Notification notification, List<BTUser> members)
        {
            try
            {
                foreach(BTUser btUser in members)
                {
                    await EmailNotificationAsync(notification, notification.Title);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task SaveNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public Task SmsNotificationAsync(string phone, Notification notification, string emailSubject)
        {
            throw new NotImplementedException();
        }
    }
}

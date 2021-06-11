using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Sevices.Interfaces
{
    public interface IBTNotificationService
    {
        public Task SaveNotificationAsync(Notification notification);
        public Task AdminsNotificationAsync(Notification notification, int companyId);
        public Task MembersNotificationAsync(Notification notification, List<BTUser> members);
        public Task EmailNotificationAsync(Notification notification, string emailSubject);
        public Task SmsNotificationAsync(string phone, Notification notification, string emailSubject);
        public Task<List<Notification>> GetReceivedNotificationAsync(string userId);
        public Task<List<Notification>> GetSentNotificationAsync(string userId);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        
        [DisplayName("Ticket")]
        public int TicketId { get; set; }
                
        [DisplayName("Updated Item")]
        public string Property { get; set; }
        
        [DisplayName("Previous")]
        public int OldValue { get; set; }
        
        [DisplayName("Current")]
        public int NewValue { get; set; }

        [DisplayName("Date Modified")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Team Member")]
        public int UserId { get; set; }
        
        [DisplayName("Description of Change")]
        public string Description { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}

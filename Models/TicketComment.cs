using BugTracker.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        ///
        public int PostId { get; set; }
        [Required]
        [DisplayName("Member Comment")]
        public string Comment { get; set; }
        
        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset Created { get; set; }
        
        [DisplayName("Ticket")]
        public int TicketId { get; set; }
        
        [DisplayName("Team Member")]
        public string UserId { get; set; }
        //
        public string ModeratorId { get; set; }
        public DateTime? Moderated { get; set; }
        public string ModeratedBody { get; set; }
        public ModerationType? ModerationType { get; set; }
        public string ModeratedReason { get; set; }
        
        public virtual Ticket Ticket { get; set; }
        //
        public virtual BTUser User { get; set; }
        public virtual BTUser Moderator { get; set; }
        public DateTime Updated { get; internal set; }
    }
}

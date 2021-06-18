using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Sevices.Interfaces;
using Microsoft.AspNetCore.Identity;
using BugTracker.Extensions;

namespace BugTracker.Controllers
{
    public class TicketCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBasicImageService _basicImageService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTNotificationService _notificationService;

        public TicketCommentsController(ApplicationDbContext context, IBasicImageService basicImageService, UserManager<BTUser> userManager, IBTProjectService projectService, IBTNotificationService notificationService)
        {
            _context = context;
            _basicImageService = basicImageService;
            _userManager = userManager;
            _projectService = projectService;
            _notificationService = notificationService;
        }

        // GET: TicketComments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketComment.Include(t => t.Ticket);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // GET: TicketComments/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description");
            return View();
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Comment,TicketId")] TicketComment ticketComment)
        {
            Notification notification;

            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId().Value;                
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticketComment.Ticket.ProjectId);
                BTUser btUser = await _userManager.GetUserAsync(User);
                ticketComment.UserId = btUser.Id;
                ticketComment.Created = DateTimeOffset.Now;
                _context.Add(ticketComment);
                await _context.SaveChangesAsync();

                notification = new()
                {
                    TicketId = ticketComment.TicketId,
                    Title = $"Comment was added to ticket your assigned to.",
                    Message = $"Ticket: Ticket comment added by {btUser?.FullName}",
                    Created = DateTimeOffset.Now,
                    SenderId = btUser?.Id,
                    RecipientId = projectManager?.Id
                };
                if (projectManager != null)
                {
                    await _notificationService.SaveNotificationAsync(notification);
                }
                else
                {
                    await _notificationService.AdminsNotificationAsync(notification, companyId);

                }
                //Alert Dev if ticket is Assigned
                if (ticketComment.Ticket.DeveloperUserId != null)
                {
                    notification = new()
                    {
                        TicketId = ticketComment.Ticket.Id,
                        Title = $"Ticket assigned to you has been modified",
                        Message = $"Ticket: Ticket comment was added by {btUser?.FullName}",
                        Created = DateTimeOffset.Now,
                        SenderId = btUser?.Id,
                        RecipientId = ticketComment.Ticket.DeveloperUserId
                    };
                    await _notificationService.SaveNotificationAsync(notification);

                }
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            
        }

        // GET: TicketComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment.FindAsync(id);
            if (ticketComment == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
            return View(ticketComment);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            if (id != ticketComment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketCommentExists(ticketComment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketComment = await _context.TicketComment.FindAsync(id);
            _context.TicketComment.Remove(ticketComment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketCommentExists(int id)
        {
            return _context.TicketComment.Any(e => e.Id == id);
        }
    }
}

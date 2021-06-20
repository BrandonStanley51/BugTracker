using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Sevices.Interfaces;
using BugTracker.Extensions;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTHistoryService _historyService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBasicImageService _basicImageService;

        

        public TicketsController(ApplicationDbContext context,
                                    UserManager<BTUser> userManager,
                                    IBTProjectService projectService,
                                    IBTTicketService ticketService,
                                    IBTHistoryService historyService,
                                    IBTCompanyInfoService companyInfoService,
                                    IBTNotificationService notificationService, IBasicImageService basicImageService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
            _historyService = historyService;
            _companyInfoService = companyInfoService;
            _notificationService = notificationService;
            _basicImageService = basicImageService;
            
        }

        // GET: Tickets
        public async Task<IActionResult> MyTickets()
        {
            var applicationDbContext = _context.Ticket.Include(t => t.DeveloperUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketStatus).Include(t => t.TicketType).Include(t => t.TicketPriority);
            return View(await applicationDbContext.ToListAsync());
        }

        // public Task<IActionResult> MyTickets()
        //{

        // }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AllTickets()
        {
            var applicationDbContext = await _context.Ticket
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.OwnerUser)
                                            .Include(t => t.Project)
                                            .Include(t => t.Comments)
                                            .Include(t => t.TicketPriority)
                                            .Include(t => t.TicketStatus)
                                            .Include(t => t.TicketType).ToListAsync();
            return View(applicationDbContext);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.OwnerUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.History).ThenInclude(t=>t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            //Get current user
            BTUser btUser = await _userManager.GetUserAsync(User);
            //Get user company Id
            int companyId = User.Identity.GetCompanyId().Value;
            //TODO: Filter List

            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");


            }
            else
            {


                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserProjectsAsync(btUser.Id), "Id", "Name");
            }




            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);


                ticket.Created = DateTimeOffset.Now;

                string userId = _userManager.GetUserId(User);
                ticket.OwnerUserId = userId;

                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;

                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();

                #region Add History
                Ticket newTicket = await _context.Ticket
                                .Include(t => t.TicketPriority)
                                .Include(t => t.TicketStatus)
                                .Include(t => t.TicketType)
                                .Include(t => t.Project)
                                .Include(t => t.DeveloperUser)
                                .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);
                await _historyService.AddHistoryAsync(null, newTicket, btUser.Id);
                #endregion

                #region Notification

                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                int companyId = User.Identity.GetCompanyId().Value;
                Notification notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "New Ticket",
                    Message = $"New Ticket: {ticket?.Title}, was created by {btUser?.FullName}",
                    Created = DateTimeOffset.Now,
                    SenderId = btUser?.Id,
                    RecipientId = projectManager?.Id
                };
                if (projectManager != null)
                {
                    await _notificationService.SaveNotificationAsync(notification);
                    await _notificationService.EmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _notificationService.AdminsNotificationAsync(notification, companyId);

                }
                #endregion

                return RedirectToAction("Details", "Projects", new { id = ticket.ProjectId });
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name", ticket.TicketTypeId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name", ticket.TicketTypeId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedData,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }
            Notification notification;

            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                BTUser btUser = await _userManager.GetUserAsync(User);
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);

                Ticket oldTicket = await _context.Ticket
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                                    .Include(t => t.DeveloperUser)
                                                    .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    //Create and Save a Notification
                    notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = $"Ticket modified on project - {oldTicket.Project.Name}",
                        Message = $"Ticket: [{ticket.Id}]:{ticket.Title} updated by {btUser?.FullName}",
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
                    if(ticket.DeveloperUserId != null)
                    {
                        notification = new()
                        {
                            TicketId = ticket.Id,
                            Title = $"Ticket assigned to you has been modified" ,
                            Message = $"Ticket: [{ticket.Id}]:{ticket.Title} updated by {btUser?.FullName}",
                            Created = DateTimeOffset.Now,
                            SenderId = btUser?.Id,
                            RecipientId = ticket.DeveloperUserId
                        };
                        await _notificationService.SaveNotificationAsync(notification);
                        
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //Add History
                Ticket newTicket = await _context.Ticket
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .Include(t => t.Project)
                                    .Include(t => t.DeveloperUser)
                                    .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);
                return RedirectToAction(nameof(MyTickets));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name", ticket.TicketTypeId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            return View(ticket);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager, Submitter")]
        public async Task<IActionResult> AssignTicket(int? Id)
        {
            if (!Id.HasValue)
            {
                return NotFound();
            }

            AssignDeveloperViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            model.Ticket = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == Id);
            model.Developers = new SelectList(await _projectService.DevelopersOnProjectAsync(model.Ticket.ProjectId), "Id", "FullName");

            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Roles = "Admin, ProjectManager, Submitter")]
        public async Task<IActionResult> AssignTicket(AssignDeveloperViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.DeveloperId))
            {
                int companyId = User.Identity.GetCompanyId().Value;

                BTUser btUser = await _userManager.GetUserAsync(User);
                BTUser developer = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(m => m.Id == viewModel.DeveloperId);
                BTUser projectManager = await _projectService.GetProjectManagerAsync(viewModel.Ticket.ProjectId);

                Ticket oldTicket = await _context.Ticket
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                                    .Include(t => t.DeveloperUser)
                                                    .AsNoTracking().FirstOrDefaultAsync(t => t.Id == viewModel.Ticket.Id);

                await _ticketService.AssignTicketAsync(viewModel.Ticket.Id, viewModel.DeveloperId);



                Ticket newTicket = await _context.Ticket
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                                    .Include(t => t.DeveloperUser)
                                                    .AsNoTracking().FirstOrDefaultAsync(t => t.Id == viewModel.Ticket.Id);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);
                Notification notification = new()
                {
                    TicketId = newTicket.Id,
                    Title = $"You've Been Assigned A Ticket",
                    Message = $"New  Ticket: [{newTicket?.Title} Was Assigned By {btUser?.FullName}",
                    Created = DateTimeOffset.Now,
                    SenderId = btUser?.Id,
                    RecipientId = viewModel.DeveloperId,
                };
                if (viewModel.DeveloperId != null)
                {
                    await _notificationService.SaveNotificationAsync(notification);
                    await _notificationService.EmailNotificationAsync(notification, "message has been sent.");
                }
            }
            return RedirectToAction("Details", new { id = viewModel.Ticket.Id });
        }



        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.TicketPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyTickets));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}

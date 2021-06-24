using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Sevices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBasicImageService _basicImageService;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<BTUser> userManager,
                              IBTCompanyInfoService companyInfoService,
                              IBTProjectService projectService,
                              IBTTicketService ticketService, IBasicImageService basicImageService)
        {
            _logger = logger;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _projectService = projectService;
            _ticketService = ticketService;
            _basicImageService = basicImageService;
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var userId = _userManager.GetUserId(User);
            DashboardViewModel model = new()
            {
                Company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId),
                Projects = await _projectService.GetAllProjectsByCompany(companyId),
                Tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId),
                Members = await _companyInfoService.GetAllMembersAsync(companyId),
                UnassignedTickets = await _ticketService.GetAllTicketsByStatusAsync(companyId, "Unassigned"),
                DevTickets = await _ticketService.GetAllTicketsByRoleAsync("Developer", userId),
                SubTickets = await _ticketService.GetAllTicketsByRoleAsync("Submitter", userId),
        };
            return View(model);
        }        

        public IActionResult Landing()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

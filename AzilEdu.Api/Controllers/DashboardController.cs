using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public DashboardController(AzilEduDbContext context)
    {
        _context = context;
    }
    // Kasnije će admin vidjeti sve podatke, a ostale role samo svoj dio aplikacije.
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var today = DateTime.Today;

        var summary = new DashboardSummaryDto
        {
            AnimalsCount = await _context.Animals.CountAsync(),
            AvailableAnimalsCount = await _context.Animals.CountAsync(animal => animal.AnimalStatusId == 1),
            ActiveVolunteersCount = await _context.Volunteers.CountAsync(volunteer => volunteer.VolunteerStatusId == 2),
            OpenVolunteerTasksCount = await _context.VolunteerTasks.CountAsync(task => task.VolunteerTaskStatusId == 1),
            ActiveDonorsCount = await _context.Donors.CountAsync(donor => donor.DonorStatusId == 2),
            EmployeesCount = await _context.Employees.CountAsync(),

            DonationsCount = await _context.Donations.CountAsync(),

            PendingDonationsCount = await _context.Donations
                .Include(donation => donation.DonationStatus)
                .CountAsync(donation => donation.DonationStatus != null
                    && donation.DonationStatus.Name == "Evidentirana"),

            MoneyDonationsTotal = await _context.Donations
                .Include(donation => donation.DonationType)
                .Include(donation => donation.DonationStatus)
                .Where(donation => donation.DonationType != null
                    && donation.DonationType.Name == "Novčana"
                    && donation.DonationStatus != null
                    && donation.DonationStatus.Name != "Otkazana")
                .SumAsync(donation => donation.Amount ?? 0),

            EstimatedMaterialDonationsTotal = await _context.Donations
                .Include(donation => donation.DonationType)
                .Include(donation => donation.DonationStatus)
                .Where(donation => donation.DonationType != null
                    && donation.DonationType.Name != "Novčana"
                    && donation.DonationStatus != null
                    && donation.DonationStatus.Name != "Otkazana")
                .SumAsync(donation => donation.EstimatedValue ?? 0),

            OverdueVolunteerTasksCount = await _context.VolunteerTasks
                .Include(task => task.VolunteerTaskStatus)
                .CountAsync(task => task.DueDate.HasValue
                    && task.DueDate.Value.Date < today
                    && task.VolunteerTaskStatus != null
                    && task.VolunteerTaskStatus.Name != "Završeno"
                    && task.VolunteerTaskStatus.Name != "Otkazano")
        };

        return Ok(summary);
    }

    [HttpGet("recent-donations")]
    public async Task<ActionResult<List<RecentDonationDto>>> GetRecentDonations()
    {
        var donations = await _context.Donations
            .Include(donation => donation.Donor)
            .Include(donation => donation.DonationType)
            .OrderByDescending(donation => donation.DonationDate)
            .Take(5)
            .ToListAsync();

        var result = donations.Select(donation => new RecentDonationDto
        {
            Id = donation.Id,
            DonorName = donation.Donor != null
        ? (!string.IsNullOrWhiteSpace(donation.Donor.OrganizationName)
            ? donation.Donor.OrganizationName
            : donation.Donor.FirstName + " " + donation.Donor.LastName)
        : string.Empty,
            DonationType = donation.DonationType != null ? donation.DonationType.Name : string.Empty,
            DonationDate = donation.DonationDate,
            Amount = donation.Amount,
            ItemName = donation.ItemName,
            Quantity = donation.Quantity,
            EstimatedValue = donation.EstimatedValue
        }).ToList();

        return Ok(result);
    }
}
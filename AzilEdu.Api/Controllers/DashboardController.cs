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
                .CountAsync(donation => donation.DonationStatusId == 1),

            MoneyDonationsTotal = await _context.Donations
                .Where(donation => donation.DonationTypeId == 1 && donation.Amount.HasValue)
                .SumAsync(donation => donation.Amount!.Value),

            EstimatedMaterialDonationsTotal = await _context.Donations
                .Where(donation => donation.DonationTypeId != 1 && donation.EstimatedValue.HasValue)
                .SumAsync(donation => donation.EstimatedValue!.Value),

            OverdueVolunteerTasksCount = await _context.VolunteerTasks
                .CountAsync(task => task.DueDate.HasValue
                    && task.DueDate.Value.Date < today
                    && task.VolunteerTaskStatusId != 4
                    && task.VolunteerTaskStatusId != 5)
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
            .ThenByDescending(donation => donation.Id)
            .Take(5)
            .ToListAsync();

        var result = donations.Select(donation =>
        {
            string donorName;
            if (donation.Donor is null)
                donorName = string.Empty;
            else if (!string.IsNullOrWhiteSpace(donation.Donor.OrganizationName))
                donorName = donation.Donor.OrganizationName;
            else
                donorName = $"{donation.Donor.FirstName} {donation.Donor.LastName}".Trim();

            return new RecentDonationDto
            {
                Id = donation.Id,
                DonorName = donorName,
                DonationType = donation.DonationType?.Name ?? string.Empty,
                DonationDate = donation.DonationDate,
                Amount = donation.Amount,
                ItemName = donation.ItemName,
                Quantity = donation.Quantity,
                EstimatedValue = donation.EstimatedValue
            };
        }).ToList();

        return Ok(result);
    }
}
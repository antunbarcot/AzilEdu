using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    // DonorId će kasnije biti povezan s prijavljenim korisnikom preko AppUserId.
    private readonly AzilEduDbContext _context;

    public DonationsController(AzilEduDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<DonationDto>>> GetDonations(
        [FromQuery] int? donorId,
        [FromQuery] int? typeId,
        [FromQuery] int? statusId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = _context.Donations
            .Include(donation => donation.Donor)
            .Include(donation => donation.DonationType)
            .Include(donation => donation.DonationStatus)
            .AsQueryable();
        // Kasnije će donator vidjeti samo svoje donacije.
        if (donorId.HasValue)
        {
            query = query.Where(donation => donation.DonorId == donorId.Value);
        }
        // ... ostatak filtera nepromijenjen
        if (typeId.HasValue)
        {
            query = query.Where(donation => donation.DonationTypeId == typeId.Value);
        }

        if (statusId.HasValue)
        {
            query = query.Where(donation => donation.DonationStatusId == statusId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(donation => donation.DonationDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(donation => donation.DonationDate <= dateTo.Value);
        }

        var donations = await query
            .OrderByDescending(donation => donation.DonationDate)
            .ToListAsync();

        var result = donations.Select(ToDto).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DonationDto>> GetDonationById(int id)
    {
        var donation = await _context.Donations
            .Include(item => item.Donor)
            .Include(item => item.DonationType)
            .Include(item => item.DonationStatus)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (donation is null)
        {
            return NotFound();
        }

        return Ok(ToDto(donation));
    }

    [HttpPost]
    public async Task<ActionResult<DonationDto>> CreateDonation(SaveDonationDto request)
    {
        var validationError = await ValidateBusinessRules(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var donation = new Donation
        {
            DonorId = request.DonorId,
            DonationTypeId = request.DonationTypeId,
            DonationStatusId = request.DonationStatusId,
            DonationDate = request.DonationDate,
            Amount = request.Amount,
            ItemName = request.ItemName,
            Quantity = request.Quantity,
            EstimatedValue = request.EstimatedValue,
            Notes = request.Notes
        };

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        var createdDonation = await _context.Donations
            .Include(item => item.Donor)
            .Include(item => item.DonationType)
            .Include(item => item.DonationStatus)
            .FirstAsync(item => item.Id == donation.Id);

        return CreatedAtAction(
            nameof(GetDonationById),
            new { id = donation.Id },
            ToDto(createdDonation));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDonation(int id, SaveDonationDto request)
    {
        var validationError = await ValidateBusinessRules(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var donation = await _context.Donations.FindAsync(id);

        if (donation is null)
        {
            return NotFound();
        }

        donation.DonorId = request.DonorId;
        donation.DonationTypeId = request.DonationTypeId;
        donation.DonationStatusId = request.DonationStatusId;
        donation.DonationDate = request.DonationDate;
        donation.Amount = request.Amount;
        donation.ItemName = request.ItemName;
        donation.Quantity = request.Quantity;
        donation.EstimatedValue = request.EstimatedValue;
        donation.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDonation(int id)
    {
        var donation = await _context.Donations.FindAsync(id);

        if (donation is null)
        {
            return NotFound();
        }

        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateBusinessRules(SaveDonationDto request)
    {
        var donor = await _context.Donors.FindAsync(request.DonorId);
        if (donor is null)
        {
            return "Donator je obavezan i mora biti valjan.";
        }

        var type = await _context.DonationTypes.FindAsync(request.DonationTypeId);
        if (type is null)
        {
            return "Tip donacije je obavezan i mora biti valjan.";
        }

        var status = await _context.DonationStatuses.FindAsync(request.DonationStatusId);
        if (status is null)
        {
            return "Status donacije je obavezan i mora biti valjan.";
        }

        var isMonetary = type.Name == "Novčana";

        if (isMonetary && !request.Amount.HasValue)
        {
            return "Za novčanu donaciju iznos je obavezan.";
        }

        if (!isMonetary && string.IsNullOrWhiteSpace(request.ItemName))
        {
            return "Za materijalnu donaciju naziv stvari je obavezan.";
        }

        return null;
    }

    private static DonationDto ToDto(Donation donation)
    {
        return new DonationDto
        {
            Id = donation.Id,
            DonorId = donation.DonorId,
            DonorName = donation.Donor != null
                ? (!string.IsNullOrWhiteSpace(donation.Donor.OrganizationName)
                    ? donation.Donor.OrganizationName
                    : donation.Donor.FirstName + " " + donation.Donor.LastName)
                : string.Empty,
            DonationTypeId = donation.DonationTypeId,
            Type = donation.DonationType != null ? donation.DonationType.Name : string.Empty,
            DonationStatusId = donation.DonationStatusId,
            Status = donation.DonationStatus != null ? donation.DonationStatus.Name : string.Empty,
            DonationDate = donation.DonationDate,
            Amount = donation.Amount,
            ItemName = donation.ItemName,
            Quantity = donation.Quantity,
            EstimatedValue = donation.EstimatedValue,
            Notes = donation.Notes
        };
    }
}
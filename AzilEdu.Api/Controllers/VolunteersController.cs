// AzilEdu.Api/Controllers/VolunteersController.cs
using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(
    Policy = AzilEdu.Api.Security.AuthorizationPolicies.Staff)]
[ApiController]
[Route("api/[controller]")]
public class VolunteersController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public VolunteersController(AzilEduDbContext context)
    {
        _context = context;
    }

    private static VolunteerDto ToDto(Volunteer volunteer) => new()
    {
        Id = volunteer.Id,
        FirstName = volunteer.FirstName,
        LastName = volunteer.LastName,
        Email = volunteer.Email,
        Phone = volunteer.Phone,
        Skills = volunteer.Skills,
        AvailableFrom = volunteer.AvailableFrom,
        Notes = volunteer.Notes,
        VolunteerStatusId = volunteer.VolunteerStatusId,
        Status = volunteer.VolunteerStatus != null ? volunteer.VolunteerStatus.Name : string.Empty
    };

    [HttpGet]
    public async Task<ActionResult<List<VolunteerDto>>> GetVolunteers()
    {
        var volunteers = await _context.Volunteers
            .Include(volunteer => volunteer.VolunteerStatus)
            .OrderBy(volunteer => volunteer.LastName)
            .Select(volunteer => ToDto(volunteer))
            .ToListAsync();

        return Ok(volunteers);
    }
    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetVolunteersLookup()
    {
        var result = await _context.Volunteers
            .OrderBy(volunteer => volunteer.LastName)
            .Select(volunteer => new LookupDto
            {
                Id = volunteer.Id,
                Name = volunteer.FirstName + " " + volunteer.LastName
            })
            .ToListAsync();

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<VolunteerDto>> GetVolunteerById(int id)
    {
        var volunteer = await _context.Volunteers
            .Include(volunteer => volunteer.VolunteerStatus)
            .FirstOrDefaultAsync(volunteer => volunteer.Id == id);

        if (volunteer is null)
            return NotFound();

        return Ok(ToDto(volunteer));
    }

    [HttpPost]
    public async Task<ActionResult<VolunteerDto>> CreateVolunteer(SaveVolunteerDto dto)
    {
        var volunteer = new Volunteer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Skills = dto.Skills,
            AvailableFrom = dto.AvailableFrom,
            Notes = dto.Notes,
            VolunteerStatusId = dto.VolunteerStatusId
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var saved = await _context.Volunteers
            .Include(v => v.VolunteerStatus)
            .FirstOrDefaultAsync(v => v.Id == volunteer.Id);

        if (saved is null)
            return NotFound();

        return CreatedAtAction(nameof(GetVolunteerById), new { id = volunteer.Id }, ToDto(saved));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVolunteer(int id, SaveVolunteerDto dto)
    {
        var volunteer = await _context.Volunteers.FindAsync(id);

        if (volunteer is null)
            return NotFound();

        volunteer.FirstName = dto.FirstName;
        volunteer.LastName = dto.LastName;
        volunteer.Email = dto.Email;
        volunteer.Phone = dto.Phone;
        volunteer.Skills = dto.Skills;
        volunteer.AvailableFrom = dto.AvailableFrom;
        volunteer.Notes = dto.Notes;
        volunteer.VolunteerStatusId = dto.VolunteerStatusId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVolunteer(int id)
    {
        var volunteer = await _context.Volunteers.FindAsync(id);

        if (volunteer is null)
            return NotFound();

        _context.Volunteers.Remove(volunteer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
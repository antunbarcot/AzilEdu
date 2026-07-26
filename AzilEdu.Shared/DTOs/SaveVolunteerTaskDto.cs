using System.ComponentModel.DataAnnotations;

namespace AzilEdu.Shared.DTOs;

public class SaveVolunteerTaskDto
{
    [Required(ErrorMessage = "Naslov zadatka je obavezan.")]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int? VolunteerId { get; set; }
    public int? AnimalId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Status zadatka je obavezan.")]
    public int VolunteerTaskStatusId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Tip zadatka je obavezan.")]
    public int VolunteerTaskTypeId { get; set; }
}
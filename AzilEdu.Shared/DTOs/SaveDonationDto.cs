using System.ComponentModel.DataAnnotations;

namespace AzilEdu.Shared.DTOs;

public class SaveDonationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Donator je obavezan.")]
    public int DonorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Tip donacije je obavezan.")]
    public int DonationTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Status donacije je obavezan.")]
    public int DonationStatusId { get; set; }

    [Required(ErrorMessage = "Datum donacije je obavezan.")]
    public DateTime DonationDate { get; set; }

    public decimal? Amount { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string Notes { get; set; } = string.Empty;
}
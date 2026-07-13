using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


using BTLWEB.Models;

namespace BTLWEB.Models.Competition;

public class Competition

{
  [Key]
  public int Id { get; set; }

  [Required]
  [MaxLength(255)]
  public string Name { get; set; } = string.Empty;

  public string? Description { get; set; }

  public string? Rules { get; set; }

  [Required]
  public DateTime SubmissionStartDate { get; set; }

  [Required]
  public DateTime SubmissionEndDate { get; set; }

  public int Status { get; set; } = (int)CompetitionStatus.Pending;

  public DateTime CreatedAt { get; set; } = DateTime.Now;

  public int? CreatedByUserId { get; set; }

  public DateTime? UpdatedAt { get; set; }

  [ForeignKey(nameof(CreatedByUserId))]
  public User? CreatedByUser { get; set; }

  [MaxLength(500)]
  public string? ImageUrl { get; set; }

  public ICollection<CompetitionEntry> Entries { get; set; } = new List<CompetitionEntry>();


}


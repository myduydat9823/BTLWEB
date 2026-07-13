using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BTLWEB.Models;



namespace BTLWEB.Models.Competition;


public class CompetitionEntry
{
  [Key]
  public int Id { get; set; }

  public int CompetitionId { get; set; }

  public int UserId { get; set; }

  public int PhotoId { get; set; }

  [Required]
  public DateTime SubmittedAt { get; set; } = DateTime.Now;

  public int Status { get; set; } = (int)EntryStatus.Pending;

  public double? AverageScore { get; set; }

  public int? Rank { get; set; }

  public string? AdminNote { get; set; }

  [ForeignKey(nameof(CompetitionId))]
  public Competition? Competition { get; set; }

  [ForeignKey(nameof(UserId))]
  public User? User { get; set; }

  [ForeignKey(nameof(PhotoId))]
  public Photo? Photo { get; set; }
}


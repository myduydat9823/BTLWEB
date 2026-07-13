using System.ComponentModel.DataAnnotations;
using BTLWEB.Models.Competition;

namespace BTLWEB.ViewModels.Entry;

public class ReviewEntryViewModel
{
  public int EntryId { get; set; }

  public int CompetitionId { get; set; }

  public int UserId { get; set; }

  public EntryStatus Status { get; set; }

  public double? AverageScore { get; set; }

  public int? Rank { get; set; }

  [Display(Name = "Ghi chú")]
  public string Note { get; set; } = string.Empty;
}


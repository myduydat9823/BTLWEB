using System.ComponentModel.DataAnnotations;
using BTLWEB.Models.Competition;

namespace BTLWEB.ViewModels.Competition;

public class CompetitionListViewModel
{
  public int Id { get; set; }

  [Display(Name = "Tên cuộc thi")]
  public string Name { get; set; } = string.Empty;

  public DateTime SubmissionStartDate { get; set; }
  public DateTime SubmissionEndDate { get; set; }

  public int EntryCount { get; set; }

  public CompetitionStatus Status { get; set; }

  public string? ImageUrl { get; set; }
}


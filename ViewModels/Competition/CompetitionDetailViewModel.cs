using System.ComponentModel.DataAnnotations;
using BTLWEB.Models.Competition;
using BTLWEB.ViewModels.Entry;


namespace BTLWEB.ViewModels.Competition;

public class CompetitionDetailViewModel
{
  public int Id { get; set; }

  [Display(Name = "Tên cuộc thi")]
  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string Rules { get; set; } = string.Empty;

  public DateTime SubmissionStartDate { get; set; }
  public DateTime SubmissionEndDate { get; set; }

  public CompetitionStatus Status { get; set; }

  public int EntryCount { get; set; }

  public string? ImageUrl { get; set; }

  public List<EntryListViewModel> Entries { get; set; } = new();
}


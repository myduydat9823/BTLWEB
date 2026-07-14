using System.ComponentModel.DataAnnotations;
using BTLWEB.Models.Competition;

namespace BTLWEB.ViewModels.Entry;

public class EntryListViewModel
{
  public int Id { get; set; }

  public int CompetitionId { get; set; }

  public int UserId { get; set; }

  public int PhotoId { get; set; }

  public Guid? EntryGroupId { get; set; }

  public DateTime SubmittedAt { get; set; }

  public EntryStatus Status { get; set; }

  public double? AverageScore { get; set; }

  public int? Rank { get; set; }

  [Display(Name = "Ghi chú admin")]
  public string AdminNote { get; set; } = string.Empty;

  public string PhotoTitle { get; set; } = string.Empty;
  public string PhotoDescription { get; set; } = string.Empty;
  public string PhotoImagePath { get; set; } = string.Empty;

  [Display(Name = "Người dùng")]
  public string UserFullName { get; set; } = string.Empty;

  [Display(Name = "Tên đăng nhập")]
  public string UserName { get; set; } = string.Empty;
}


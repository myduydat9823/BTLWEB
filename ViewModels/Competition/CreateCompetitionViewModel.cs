using System.ComponentModel.DataAnnotations;
using BTLWEB.Models.Competition;

namespace BTLWEB.ViewModels.Competition;

public class CreateCompetitionViewModel
{
  public int Id { get; set; }

  [Required]
  [MaxLength(255)]
  [Display(Name = "Tên cuộc thi")]
  public string Name { get; set; } = string.Empty;

  [Display(Name = "Mô tả")]
  public string Description { get; set; } = string.Empty;

  [Display(Name = "Thể lệ")]
  public string Rules { get; set; } = string.Empty;

  [Required]
  [DataType(DataType.DateTime)]
  [Display(Name = "Thời gian mở gửi bài")]
  public DateTime SubmissionStartDate { get; set; }

  [Required]
  [DataType(DataType.DateTime)]
  [Display(Name = "Thời gian kết thúc")]
  public DateTime SubmissionEndDate { get; set; }

  [Range(0, 4)]
  [Display(Name = "Trạng thái")]
  public int Status { get; set; } = (int)CompetitionStatus.Pending;

  [Display(Name = "URL ảnh bìa")]
  [MaxLength(500)]
  public string? ImageUrl { get; set; }
}


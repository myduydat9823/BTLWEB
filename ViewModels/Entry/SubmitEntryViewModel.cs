using System.ComponentModel.DataAnnotations;

namespace BTLWEB.ViewModels.Entry;

public class SubmitEntryViewModel
{
  [Required]
  public int CompetitionId { get; set; }

  [Required]
  [MaxLength(255)]
  [Display(Name = "Tiêu đề tác phẩm")]
  public string PhotoTitle { get; set; } = string.Empty;

  [Display(Name = "Mô tả tác phẩm")]
  public string PhotoDescription { get; set; } = string.Empty;

  [Required]
  [Display(Name = "Ảnh dự thi")]
  public IFormFile PhotoFile { get; set; } = default!;
}


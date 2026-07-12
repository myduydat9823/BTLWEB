using System.ComponentModel.DataAnnotations;

namespace BTLWEB.ViewModels;

public class EditProfileViewModel
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(150)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(500)]
    [Url(ErrorMessage = "Đường dẫn ảnh đại diện không hợp lệ.")]
    [Display(Name = "Ảnh đại diện")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Tải ảnh đại diện")]
    public IFormFile? AvatarFile { get; set; }

    [StringLength(1000)]
    [Display(Name = "Giới thiệu")]
    public string? Bio { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(300)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime? DateOfBirth { get; set; }
}

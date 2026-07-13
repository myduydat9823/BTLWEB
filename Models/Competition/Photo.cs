using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BTLWEB.Models;



namespace BTLWEB.Models.Competition;

public class Photo
{
  [Key]
  public int Id { get; set; }

  [Required]
  [MaxLength(255)]
  public string Title { get; set; } = string.Empty;

  public string? Description { get; set; }

  [Required]
  public string ImagePath { get; set; } = string.Empty;

  public int UserId { get; set; }

  public int Status { get; set; } = 0;

  public DateTime UploadedAt { get; set; } = DateTime.Now;

  public long FileSize { get; set; }

  public string? FileExtension { get; set; }

  [ForeignKey(nameof(UserId))]
  public User? User { get; set; }
}


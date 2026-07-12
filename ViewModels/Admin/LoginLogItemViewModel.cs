namespace BTLWEB.ViewModels;

public class LoginLogItemViewModel
{
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

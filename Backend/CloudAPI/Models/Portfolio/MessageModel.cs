namespace CloudAPI.Models.Portfolio;

public class MessageModel
{
    public string SenderName { get; set; }
    public string Content { get; set; }
    public string IpAddress { get; set; }

    public string RecaptchaToken { get; set; }
}

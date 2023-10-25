using Microsoft.Azure.Cosmos.Table;

namespace CloudAPI.Models.Portfolio;

public class PortfolioMessage : TableEntity
{
    public string IpAddress { get; set; }
    public string SenderName { get; set; }
    public string Content { get; set; }
}
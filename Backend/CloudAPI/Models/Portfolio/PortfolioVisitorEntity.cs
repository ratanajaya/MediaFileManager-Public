using Microsoft.Azure.Cosmos.Table;

namespace CloudAPI.Models.Portfolio;

public class PortfolioVisitorEntity : TableEntity
{
    public string IpAddress { get; set; }
    public string IspName { get; set; }
    public string OrganizationName { get; set; }
    public bool? IsVpn { get; set; }
    public string Country { get; set; }
    public string City { get; set; }

    public string FullJsonData { get; set; }
}
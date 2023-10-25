using CloudAPI.Models.Portfolio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudAPI.Controllers.Public;

[ApiController]
[Route("Portfolio")]
public class PortfolioController : ControllerBase
{
    ILogger _logger;
    string _recaptchaSecretKey = "[]";
    string _azureStorageConString = "[azure storage conn string]";

    public PortfolioController(ILogger logger) {
        _logger = logger;
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage(MessageModel model) {
        bool isHuman = new Func<bool>(() => {
            using(var client = new WebClient()) {
                var url = $"https://www.google.com/recaptcha/api/siteverify?secret={_recaptchaSecretKey}&response={model.RecaptchaToken}";

                var GoogleReply = client.DownloadString(url);

                var captchaResponse = JsonConvert.DeserializeObject<RecaptchaModel>(GoogleReply);

                return captchaResponse.Success.ToLower() == "true";
            }
        })();

        if(!isHuman) { return Unauthorized("Recaptcha verivication fail"); }

        var message = new PortfolioMessage {
            PartitionKey = "FromWkr",
            RowKey = Guid.NewGuid().ToString(),

            IpAddress = model.IpAddress,
            SenderName = model.SenderName,
            Content = model.Content
        };

        var storageAccount = CloudStorageAccount.Parse(_azureStorageConString);
        var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        var table = tableClient.GetTableReference("PortfolioMessage");
        var insertOrMerge = TableOperation.InsertOrMerge(message);
        var result = await table.ExecuteAsync(insertOrMerge);

        return Ok("Message sent");
    }

    [HttpPost("Diagnosis")]
    public async Task<IActionResult> Diagnosis(DiagnosisModel model) {
        var visitor = new PortfolioVisitorEntity {
            PartitionKey = "FromWkr",
            RowKey = Guid.NewGuid().ToString(),

            IpAddress = model.ip_address,
            IspName = model.connection?.isp_name,
            OrganizationName = model.connection?.isp_name,
            IsVpn = model.security?.is_vpn,

            Country = model.country,
            City = model.city,

            FullJsonData = JsonConvert.SerializeObject(model)
        };

        var storageAccount = CloudStorageAccount.Parse(_azureStorageConString);
        var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        var table = tableClient.GetTableReference("PortfolioVisitor");
        var insertOrMerge = TableOperation.InsertOrMerge(visitor);
        var result = await table.ExecuteAsync(insertOrMerge);

        return Ok();
    }
}
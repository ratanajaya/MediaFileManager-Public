using Newtonsoft.Json;
using System.Collections.Generic;

namespace CloudAPI.Models.Portfolio;

public class RecaptchaModel
{
    [JsonProperty("success")]
    public string Success {
        get { return m_Success; }
        set { m_Success = value; }
    }

    private string m_Success;
    [JsonProperty("error-codes")]
    public List<string> ErrorCodes {
        get { return m_ErrorCodes; }
        set { m_ErrorCodes = value; }
    }

    private List<string> m_ErrorCodes;
}

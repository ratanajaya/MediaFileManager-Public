using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models.ExtraDb;

public class Comment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ScrapOperationId { get; set; }

    public string Author { get; set; }
    public string Content { get; set; }
    public double? Score { get; set; }
    public DateTime? PostedDate { get; set; }
}

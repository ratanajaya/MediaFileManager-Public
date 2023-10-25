using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models.Dashboard;

public class TablePaginationModel<T>
{
    public int TotalItem { get; set; }
    public int TotalPage { get; set; }
    public List<T> Records { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.AI.Interfaces
{
    public interface INaturalQueryService
    {
        Task<string> TranslateQueryAsync(string userQuery);
        Task<object> ExecuteAnalyticsQueryAsync(string sqlQuery);
    }
}

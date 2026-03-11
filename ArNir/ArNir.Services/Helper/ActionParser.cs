using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArNir.Services.Helper
{
    public static class ActionParser
    {
        public static List<string> ExtractActions(string text)
        {
            var actions = new List<string>();
            if (Regex.IsMatch(text, @"compare", RegexOptions.IgnoreCase))
                actions.Add("Compare Models");
            if (Regex.IsMatch(text, @"trend", RegexOptions.IgnoreCase))
                actions.Add("View Trends");
            if (Regex.IsMatch(text, @"SLA", RegexOptions.IgnoreCase))
                actions.Add("SLA Summary");

            return actions;
        }
    }
}

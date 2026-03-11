using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Chat
{
    public class ChatQueryDto
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string UserQuery { get; set; } = string.Empty;
        public string? Provider { get; set; }
    }
}

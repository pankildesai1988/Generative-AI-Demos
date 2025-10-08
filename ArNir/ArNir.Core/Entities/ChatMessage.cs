using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";

        public ChatSession Session { get; set; } = null!;
    }
}

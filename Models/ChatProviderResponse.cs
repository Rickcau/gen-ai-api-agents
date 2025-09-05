using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gen_ai_api_agents.Models
{
    public class ChatProviderResponse
    {
        public string? ChatResponse { get; set; }
        public string? CoordinatorResponse { get; set; }
        public string? DevOpsResponse { get; set; }
        public string? ServiceNowResponse { get; set; }

    }
}


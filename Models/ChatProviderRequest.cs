using System.ComponentModel.DataAnnotations;

namespace gen_ai_api_agents.Models
{
    public class ChatProviderRequest
    {
        public string? SessionId { get; set; }

        public string? UserId { get; set; }
        [Required]
        public required string Prompt { get; set; }
    }
}

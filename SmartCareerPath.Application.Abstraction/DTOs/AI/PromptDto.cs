using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    /// <summary>
    /// AI prompt request
    /// </summary>
    public class AIPromptRequest
    {
        public string Prompt { get; set; }
        public string SystemPrompt { get; set; }
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
    }

    /// <summary>
    /// AI prompt response
    /// </summary>
    public class AIPromptResponse
    {
        public string Response { get; set; }
        public int TokensUsed { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}

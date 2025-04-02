using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Service for managing conversation contexts with Gemini API
    /// </summary>
    public class GeminiConversationService : GeminiServiceBase
    {
        public GeminiConversationService(ILogger<GeminiConversationService> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
        }

        /// <summary>
        /// Starts a new conversation context, making it the current active conversation
        /// </summary>
        /// <param name="metadata">Optional metadata for the conversation</param>
        /// <returns>The ID of the new conversation</returns>
        public string StartNewConversation(Dictionary<string, string> metadata = null)
        {
            return base.StartNewConversation(metadata);
        }
        
        /// <summary>
        /// Switches to an existing conversation context
        /// </summary>
        /// <param name="conversationId">ID of the conversation to switch to</param>
        /// <returns>True if the conversation was found, false otherwise</returns>
        public bool SwitchConversation(string conversationId)
        {
            return base.SwitchConversation(conversationId);
        }
        
        /// <summary>
        /// Clears the message history for the current conversation
        /// </summary>
        public void ClearCurrentConversation()
        {
            base.ClearCurrentConversation();
        }
        
        /// <summary>
        /// Gets the current conversation context
        /// </summary>
        /// <returns>The current conversation context</returns>
        public ConversationContext GetCurrentConversation()
        {
            return base.GetCurrentConversation();
        }
        
        /// <summary>
        /// Send a prompt to Gemini in the context of an ongoing conversation
        /// </summary>
        /// <param name="prompt">The user's prompt or question</param>
        /// <param name="useHistory">Whether to include conversation history (defaults to true)</param>
        /// <returns>Gemini's response as a string</returns>
        public async Task<string> GetConversationPromptAsync(string prompt, bool useHistory = true)
        {
            _logger.LogInformation("Processing conversational prompt with history={UseHistory}", useHistory);
            
            try
            {
                // Call Gemini API with the conversation prompt
                var response = await CallGeminiApiAsync(prompt, null, "ConversationalPrompt", useHistory);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing conversational prompt: {ErrorMessage}", ex.Message);
                return $"I encountered an error processing your request: {ex.Message}";
            }
        }
    }
}

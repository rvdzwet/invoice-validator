namespace BouwdepotValidationValidator.Infrastructure.Abstractions
{
    /// <summary>
    /// Interface for clients that interact with the Gemini API
    /// </summary>
    public interface ILLMProvider
    {
        /// <summary>
        /// Gets the name of the model used for text-only prompts
        /// </summary>
        /// <returns>The model name</returns>
        string GetTextModelName();

        /// <summary>
        /// Gets the name of the model used for multimodal prompts
        /// </summary>
        /// <returns>The model name</returns>
        string GetMultimodalModelName();

        /// <summary>
        /// Sends a text-only prompt to the Gemini API
        /// </summary>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="conversationContext">Optional conversation context to maintain history</param>
        /// <returns>The response from the API</returns>
        Task<string> SendTextPromptAsync(string prompt, ConversationContext conversationContext = null);

        /// <summary>
        /// Sends a multimodal prompt (text + images) to the Gemini API
        /// </summary>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="imageStreams">The image streams to include in the prompt</param>
        /// <param name="mimeTypes">The MIME types of the images</param>
        /// <param name="conversationContext">Optional conversation context to maintain history</param>
        /// <returns>The response from the API</returns>
        Task<string> SendMultimodalPromptAsync(
            string prompt, 
            IEnumerable<Stream> imageStreams, 
            IEnumerable<string> mimeTypes,
            ConversationContext conversationContext = null);

        /// <summary>
        /// Sends a structured prompt to the Gemini API and gets a structured response
        /// </summary>
        /// <typeparam name="TResponse">The type of the response object</typeparam>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="conversationContext">Optional conversation context to maintain history</param>
        /// <param name="stepName">Optional step name for the conversation context</param>
        /// <returns>The structured response from the API</returns>
        Task<TResponse> SendStructuredPromptAsync<TResponse>(
            string prompt,
            ConversationContext conversationContext = null,
            string stepName = null)
            where TResponse : class, new();

        /// <summary>
        /// Sends a multimodal structured prompt to the Gemini API and gets a structured response
        /// </summary>
        /// <typeparam name="TResponse">The type of the response object</typeparam>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="imageStreams">The image streams to include in the prompt</param>
        /// <param name="mimeTypes">The MIME types of the images</param>
        /// <param name="conversationContext">Optional conversation context to maintain history</param>
        /// <param name="stepName">Optional step name for the conversation context</param>
        /// <returns>The structured response from the API</returns>
        Task<TResponse> SendMultimodalStructuredPromptAsync<TResponse>(
            string prompt, 
            IEnumerable<Stream> imageStreams, 
            IEnumerable<string> mimeTypes,
            ConversationContext conversationContext = null,
            string stepName = null)
            where TResponse : class, new();
            
        /// <summary>
        /// Sends a structured prompt to the Gemini API and gets a structured response
        /// </summary>
        /// <param name="responseType">The type of the response object</param>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="conversationContext">Optional conversation context to maintain history</param>
        /// <param name="stepName">Optional step name for the conversation context</param>
        /// <returns>The structured response from the API</returns>
        Task<object> SendStructuredPromptAsync(
            Type responseType,
            string prompt,
            ConversationContext conversationContext = null,
            string stepName = null);

        /// <summary>
        /// Sends a multimodal structured prompt to the Gemini API and gets a structured response
        /// </summary>
        /// <param name="responseType">The type of the response object</param>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="imageStreams">The image streams to include in the prompt</param>
        /// <param name="mimeTypes">The MIME types of the images</param>
        /// <param name="conversationContext">Optional conversation context to maintain history</param>
        /// <param name="stepName">Optional step name for the conversation context</param>
        /// <returns>The structured response from the API</returns>
        Task<object> SendMultimodalStructuredPromptAsync(
            Type responseType,
            string prompt, 
            IEnumerable<Stream> imageStreams, 
            IEnumerable<string> mimeTypes,
            ConversationContext conversationContext = null,
            string stepName = null);
            
        /// <summary>
        /// Creates a new conversation context
        /// </summary>
        /// <returns>A new conversation context</returns>
        ConversationContext CreateConversationContext();
    }
}

namespace BouwdepotValidationValidator.Infrastructure.Abstractions
{
    /// <summary>
    /// Represents a message in a conversation with the LLM
    /// </summary>
    public class ConversationMessage
    {
        /// <summary>
        /// The role of the message sender (user or model)
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The content of the message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The step that generated this message (for user messages)
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Represents the context of a conversation with the LLM
    /// </summary>
    public class ConversationContext
    {
        /// <summary>
        /// Unique identifier for this conversation
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// List of messages in the conversation
        /// </summary>
        public List<ConversationMessage> Messages { get; } = new List<ConversationMessage>();

        /// <summary>
        /// Adds a user message to the conversation
        /// </summary>
        /// <param name="content">The content of the message</param>
        /// <param name="stepName">The step that generated this message</param>
        public void AddUserMessage(string content, string stepName)
        {
            Messages.Add(new ConversationMessage
            {
                Role = "user",
                Content = content,
                StepName = stepName,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        /// <summary>
        /// Adds a model message to the conversation
        /// </summary>
        /// <param name="content">The content of the message</param>
        public void AddModelMessage(string content)
        {
            Messages.Add(new ConversationMessage
            {
                Role = "model",
                Content = content,
                StepName = null,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        /// <summary>
        /// Gets the conversation history as a formatted string
        /// </summary>
        /// <returns>The conversation history</returns>
        public string GetFormattedHistory()
        {
            var history = new System.Text.StringBuilder();
            foreach (var message in Messages)
            {
                history.AppendLine($"{message.Role.ToUpperInvariant()}: {message.Content}");
                history.AppendLine();
            }
            return history.ToString();
        }

        /// <summary>
        /// Gets the conversation history for a specific step
        /// </summary>
        /// <param name="stepName">The name of the step</param>
        /// <returns>The conversation history for the step</returns>
        public string GetStepHistory(string stepName)
        {
            var history = new System.Text.StringBuilder();
            var relevantMessages = Messages.Where(m => m.StepName == stepName || m.StepName == null).ToList();
            foreach (var message in relevantMessages)
            {
                history.AppendLine($"{message.Role.ToUpperInvariant()}: {message.Content}");
                history.AppendLine();
            }
            return history.ToString();
        }
    }
}

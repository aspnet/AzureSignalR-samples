namespace AIStreaming
{
    public class OpenAIOptions
    {
        /// <summary>
        /// The endpoint of Azure OpenAI service. Only available for Azure OpenAI.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// The key of OpenAI service.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The model to use.
        /// </summary>
        public string? Model { get; set; }
    }
}

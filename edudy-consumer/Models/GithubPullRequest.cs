using System.Text.Json.Serialization;

namespace edudy_consumer.Models
{
    public class GithubPullRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("")]
        public int PRNumber { get; set; }
    }
}

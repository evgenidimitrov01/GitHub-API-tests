using Newtonsoft.Json;

namespace ApiTestNakov_Homework
{
    public class Issue
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string IssueState { get; set; }

        public Issue() { }

    }
}

using Newtonsoft.Json;

namespace ApiTestNakov_Homework
{
    public class Comment
    {
        [JsonProperty(PropertyName = "body")]
        public string Content { get; set; }

        public Comment() { }
    }
}

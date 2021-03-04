using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTestNakov_Homework
{
    public class IssueResponse
    {
        public long id { get; set; }
        public long number { get; set; }
        public string title { get; set; }
        public string body { get; set; }

        public IssueResponse() { }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization.Json;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ApiTestNakov_Homework
{
    [TestFixture]
    public class APITests
    {
        string baseUrl = string.Empty;
        string authUserName = string.Empty;
        string authUserToken = string.Empty;

        [SetUp]
        public void Setup()
        {
            this.baseUrl = "https://api.github.com/repos/testnakov/test-nakov-repo";
            XDocument authSettings = XDocument.Load("AuthSettings.xml");
            this.authUserName = authSettings.Root.Element("AuthUserName").Attribute("value").Value;
            this.authUserToken = authSettings.Root.Element("AuthUserToken").Attribute("value").Value;
        }

        #region GET request
        [Test, Order(1)]
        [Category("GET Requests")]
        public void RetrieveAllIssuesFromRepo()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues");
            client.Timeout = 3000;
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            IRestRequest request = new RestRequest(Method.GET);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);

            List<IssueResponse> issues = new JsonDeserializer().Deserialize<List<IssueResponse>>(response);
            foreach (IssueResponse issue in issues)
            {
                Assert.IsTrue(issue.id > 0);
                Assert.IsTrue(issue.number > 0);
                Assert.IsFalse(string.IsNullOrEmpty(issue.title));
                Assert.IsFalse(string.IsNullOrEmpty(issue.body));
            }
        }

        [Test, Order(1)]
        [Category("GET Requests")]
        public void RetrieveIssueByNumber()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues/5");
            client.Timeout = 3000;
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            IRestRequest request = new RestRequest(Method.GET);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test, Order(1)]
        [Category("GET Requests")]
        public void RetrieveIssueByUnexistingNumber()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues/-5");
            client.Timeout = 3000;
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            IRestRequest request = new RestRequest(Method.GET);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(404, (int)response.StatusCode);

            JObject message = (JObject)JsonConvert.DeserializeObject(response.Content);
            string res = message["message"].ToString();
            Assert.AreEqual("Not Found", res);
        }

        [Test, Order(1)]
        [Category("GET Requests")]
        public void RetrieveAllLabelsForIssue()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues/1245/labels");
            client.Timeout = 3000;
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            IRestRequest request = new RestRequest(Method.GET);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test, Order(1)]
        [Category("GET Requests")]
        public void RetrieveAllCommentsForIssue()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues/1264/comments");
            client.Timeout = 3000;
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            IRestRequest request = new RestRequest(Method.GET);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test, Order(1)]
        [Category("GET Requests")]
        public void RetrieveCommentById()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues/comments/763689309");
            client.Timeout = 3000;
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            IRestRequest request = new RestRequest(Method.GET);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);
        }
        #endregion

        #region POST requests
        [Test, Order(2)]
        [Category("POST Requests")]
        public void CreatingNewIssue()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues");
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");
            Issue issue = new Issue() 
            { 
                Title = "Some title from VS", 
                Content = "Some content from VS"
            };
            string body = JsonConvert.SerializeObject(issue,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(201, (int)response.StatusCode);

            IssueResponse newIssue = new JsonDeserializer().Deserialize<IssueResponse>(response);

            Assert.AreEqual(issue.Title, newIssue.title);
            Assert.AreEqual(issue.Content, newIssue.body);
            Assert.IsTrue(newIssue.id > 0);

            XDocument xmlIssueData = XDocument.Load("IssueData.xml");
            xmlIssueData.Root.Element("IssueNumber").SetAttributeValue("value", newIssue.number);
            xmlIssueData.Root.Element("IssueContent").SetAttributeValue("value", newIssue.body);
            xmlIssueData.Save("IssueData.xml");
        }

        [Test, Order(2)]
        [Category("POST Requests")]
        public void CreatingNewIssueWithoutTitle()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues");
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");
            Issue issue = new Issue()
            {
                Content = "Some content from VS"
            };
            string body = JsonConvert.SerializeObject(issue,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(422, (int)response.StatusCode);

            JObject message = (JObject)JsonConvert.DeserializeObject(response.Content);
            string res = message["message"].ToString();
            Assert.IsTrue(res.StartsWith("Invalid request."));
        }

        [Test, Order(2)]
        [Category("POST Requests")]
        public void CreatingNewIssueEmptyBody()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues");
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(422, (int)response.StatusCode);

            JObject message = (JObject)JsonConvert.DeserializeObject(response.Content);
            string res = message["message"].ToString();
            Assert.IsTrue(res.Contains("nil is not an object"));
        }

        [Test, Order(2)]
        [Category("POST Requests")]
        public void CreatingNewIssueWrongAuthorization()
        {
            //Arrange
            IRestClient client = new RestClient(baseUrl + "/issues");
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator("InvalidUserName", "ab357ca3565affffff4454235639ceeab237b772");
            request.AddHeader("Content-Type", "application/json");

            Issue issue = new Issue()
            {
                Title = "Some title from VS",
                Content = "Some content from VS"
            };
            string body = JsonConvert.SerializeObject(issue,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(401, (int)response.StatusCode);

            JObject message = (JObject)JsonConvert.DeserializeObject(response.Content);
            string res = message["message"].ToString();
            Assert.IsTrue(res == "Bad credentials");
        }

        [Test, Order(2)]
        [Category("POST Requests")]
        public void CreatingCommentForIssue()
        {
            //Arrange
            XDocument authSettings = XDocument.Load("IssueData.xml");
            string lastIssueNum = authSettings.Root.Element("IssueNumber").Attribute("value").Value;
            IRestClient client = new RestClient(baseUrl + "/issues/" + lastIssueNum + "/comments");
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            Comment comment = new Comment() { Content = "Some comment added from VS" };
            string body = JsonConvert.SerializeObject(comment,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(201, (int)response.StatusCode);

            CommentResponse newComment = new JsonDeserializer().Deserialize<CommentResponse>(response);

            Assert.AreEqual(comment.Content, newComment.body);

            XDocument xmlCommentData = XDocument.Load("CommentData.xml");
            xmlCommentData.Root.Element("CommentID").SetAttributeValue("value", newComment.id);
            xmlCommentData.Root.Element("CommentContent").SetAttributeValue("value", newComment.body);
            xmlCommentData.Save("CommentData.xml");
        }
        #endregion

        #region PATCH requests
        [Test, Order(3)]
        [Category("PATCH Requests")]
        public void EditExistingIssue()
        {
            //Arrange
            XDocument authSettings = XDocument.Load("IssueData.xml");
            string lastIssueNum = authSettings.Root.Element("IssueNumber").Attribute("value").Value;
            IRestClient client = new RestClient(baseUrl + "/issues/" + lastIssueNum);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.PATCH);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            Issue issue = new Issue() { Title = "Edited title from VS" };
            string body = JsonConvert.SerializeObject(issue,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test, Order(3)]
        [Category("PATCH Requests")]
        public void EditExistingIssueWithoutAuthorization()
        {
            //Arrange
            XDocument authSettings = XDocument.Load("IssueData.xml");
            string lastIssueNum = authSettings.Root.Element("IssueNumber").Attribute("value").Value;
            IRestClient client = new RestClient(baseUrl + "/issues/" + lastIssueNum);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.PATCH);
            request.AddHeader("Content-Type", "application/json");

            Issue issue = new Issue() { Title = "Edited title from VS" };
            string body = JsonConvert.SerializeObject(issue,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(401, (int)response.StatusCode);

            JObject message = (JObject)JsonConvert.DeserializeObject(response.Content);
            string res = message["message"].ToString();
            Assert.AreEqual("Requires authentication", res);
        }

        [Test, Order(3)]
        [Category("PATCH Requests")]
        public void EditUnexistingIssue()
        {
            //Arrange
            int unexistingNumber = -8;
            IRestClient client = new RestClient(baseUrl + "/issues/" + unexistingNumber);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.PATCH);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            Issue issue = new Issue() { Title = "Edited title from VS" };
            string body = JsonConvert.SerializeObject(issue,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(404, (int)response.StatusCode);
        }

        [Test, Order(3)]
        [Category("PATCH Requests")]
        public void CloseExistingIssue()
        {
            //Arrange
            XDocument authSettings = XDocument.Load("IssueData.xml");
            string lastIssueNum = authSettings.Root.Element("IssueNumber").Attribute("value").Value;
            IRestClient client = new RestClient(baseUrl + "/issues/" + lastIssueNum);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.PATCH);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            Issue issue = new Issue() { IssueState = "closed" };
            string body = JsonConvert.SerializeObject(issue, 
                Formatting.None, 
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test, Order(4)]
        [Category("PATCH Requests")]
        public void OpenExistingIssue()
        {
            //Arrange
            XDocument authSettings = XDocument.Load("IssueData.xml");
            string lastIssueNum = authSettings.Root.Element("IssueNumber").Attribute("value").Value;
            IRestClient client = new RestClient(baseUrl + "/issues/" + lastIssueNum);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.PATCH);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            Issue issue = new Issue() { IssueState = "open" };
            string body = JsonConvert.SerializeObject(issue,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test, Order(4)]
        [Category("PATCH Requests")]
        public void EditExistingComment()
        {
            //Arrange
            XDocument authSettings = XDocument.Load("CommentData.xml");
            string lastCommentID = authSettings.Root.Element("CommentID").Attribute("value").Value;
            IRestClient client = new RestClient(baseUrl + "/issues/comments/" + lastCommentID);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.PATCH);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            Comment comment = new Comment() { Content = "Edited from VS" };
            string body = JsonConvert.SerializeObject(comment,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(200, (int)response.StatusCode);

            CommentResponse lastCreatedComment = new JsonDeserializer().Deserialize<CommentResponse>(response);
            Assert.AreEqual(comment.Content, lastCreatedComment.body);
        }
        #endregion

        #region DELETE requests
        [Test, Order(5)]
        [Category("DELETE Requests")]
        public void DeleteExistingComment()
        {
            //Arrange
            XDocument authSettings = XDocument.Load("CommentData.xml");
            string lastCommentID = authSettings.Root.Element("CommentID").Attribute("value").Value;
            IRestClient client = new RestClient(baseUrl + "/issues/comments/" + lastCommentID);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.DELETE);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(204, (int)response.StatusCode);
        }

        [Test, Order(5)]
        [Category("DELETE Requests")]
        public void DeleteUnexistingComment()
        {
            //Arrange
            int unexistingNumber = -5;
            IRestClient client = new RestClient(baseUrl + "/issues/comments/" + unexistingNumber);
            client.Timeout = 3000;
            IRestRequest request = new RestRequest(Method.DELETE);
            client.Authenticator = new HttpBasicAuthenticator(authUserName, authUserToken);
            request.AddHeader("Content-Type", "application/json");

            //Act
            IRestResponse response = client.Execute(request);

            //Assert
            Assert.AreEqual(404, (int)response.StatusCode);

            JObject message = (JObject)JsonConvert.DeserializeObject(response.Content);
            string res = message["message"].ToString();
            Assert.AreEqual("Not Found", res);
        }
        #endregion

        [TearDown]
        public void TearDown()
        { 

        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace MandantCrypt.Tests
{
    [TestClass()]
    public class ApiClientTests
    {
        int stdPort = 8080;

        [TestMethod()]
        public void ApiClientTest()
        {
            ApiClient server = new ApiClient("http://localhost:" + stdPort);
            Assert.AreEqual(server.serviceUrl, "http://localhost:"+ stdPort + "/api/v1/");
        }

        [TestMethod()]
        public void getActiveMandantListTest()
        {
            int myPort = stdPort;
            TestWebServer ws = new TestWebServer(SendMandantList, "http://localhost:"+ myPort + "/api/v1/");
            ws.Run();
            System.Threading.Thread.Sleep(500);
            ApiClient server = new ApiClient("http://localhost:"+ myPort);
            List<Mandant> mlist = server.getActiveMandantList();
            Assert.IsNotNull(mlist);
            Assert.IsTrue(mlist.Count > 1);
            ws.Stop();
        }

        [TestMethod()]
        public void getPasswordListTest()
        {
            int myPort = stdPort + 1;
            TestWebServer ws = new TestWebServer(SendPassword, "http://localhost:" + myPort + "/api/v1/");
            ws.Run();
            System.Threading.Thread.Sleep(500);
            ApiClient server = new ApiClient("http://localhost:" + myPort);
            string password = server.getDecryptedPassword(1234567);
            Assert.IsNotNull(password,"returned decrypted password was null");
            Assert.AreEqual("secret+2018-06-14 02:14:03.654015+somesalt", password, "Password is different to what i would expect");
            ws.Stop();
        }


        public TestResponse SendMandantList(HttpListenerRequest request)
        {
            TestResponse myResp = new TestResponse();
            myResp.addHeader("Content-Type", "application/json");
            myResp.content = "[{\"id\":2,\"name\":\"mandant3\",\"number\":233,\"create_date\":\"2018-06-13T13:28:12\",\"last_modified\":\"2018-06-13T14:09:11.224197\",\"note\":\"another test\",\"active\":true},{\"id\":4,\"name\":\"sdfsdafsd\",\"number\":44444444,\"create_date\":\"2018-06-13T14:19:44.144244\",\"last_modified\":\"2018-06-13T14:19:44.144244\",\"note\":\"\",\"active\":true},{\"id\":1,\"name\":\"test1\",\"number\":4533,\"create_date\":\"2018-06-13T13:24:56\",\"last_modified\":\"2018-06-13T14:09:11.224197\",\"note\":\"a test mandant\",\"active\":true}]";
            return myResp;
        }

        public TestResponse SendPassword(HttpListenerRequest request)
        {
            Assert.AreEqual(request.HttpMethod, "GET", "This was not a GET request");
            string url = request.Url.ToString();
            NameValueCollection qs = request.QueryString;
            Assert.IsTrue(qs.HasKeys(),"no query parameters in request");
            Assert.IsTrue(request.QueryString.Get("mandant_id").Length == 7, "mandant_id not in query string");
            Assert.IsTrue(request.QueryString.Get("mandant_id").Equals("1234567"), "mandant_id not 1234567");
            TestResponse myResp = new TestResponse();
            myResp.addHeader("Content-Type", "application/json");

            //create password entry
            Password pwd = new Password();
            pwd.password = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            pwd.id = 23;
            pwd.name = "Noname";
            pwd.password_decrypted = "secret+2018-06-14 02:14:03.654015+somesalt";
            pwd.created_by = "Hans Dampf";
            pwd.create_date = DateTime.Parse("2018-06-14T02:14:03.657011");

            //send this password entry back
            myResp.content = pwd.ToJsonString();
            return myResp;
        }
    }
}
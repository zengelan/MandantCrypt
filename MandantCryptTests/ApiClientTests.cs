using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;

namespace MandantCrypt.Tests
{
    [TestClass()]
    public class ApiClientTests
    {
        [TestMethod()]
        public void ApiClientTest()
        {
            ApiClient server = new ApiClient("http://localhost:8000");
            Assert.AreEqual(server.serviceUrl, "http://localhost:8000/api/v1/");
        }

        [TestMethod()]
        public void getActiveMandantListTest()
        {
            TestWebServer ws = new TestWebServer(SendMandantList, "http://localhost:8080/api/v1/");
            ws.Run();
            System.Threading.Thread.Sleep(20000);
            ApiClient server = new ApiClient("http://localhost:8080");
            List<Mandant> mlist = server.getActiveMandantList();
            Assert.IsNotNull(mlist);
            Assert.IsTrue(mlist.Count > 1);
            ws.Stop();
        }


        public TestResponse SendMandantList(HttpListenerRequest request)
        {
            TestResponse myResp = new TestResponse();
            myResp.addHeader("Content-Type", "application/json");
            myResp.content = "[{\"id\":2,\"name\":\"mandant3\",\"number\":233,\"create_date\":\"2018-06-13T13:28:12\",\"last_modified\":\"2018-06-13T14:09:11.224197\",\"note\":\"another test\",\"active\":true},{\"id\":4,\"name\":\"sdfsdafsd\",\"number\":44444444,\"create_date\":\"2018-06-13T14:19:44.144244\",\"last_modified\":\"2018-06-13T14:19:44.144244\",\"note\":\"\",\"active\":true},{\"id\":1,\"name\":\"test1\",\"number\":4533,\"create_date\":\"2018-06-13T13:24:56\",\"last_modified\":\"2018-06-13T14:09:11.224197\",\"note\":\"a test mandant\",\"active\":true}]";
            return myResp;
        }

        public string SendPasswordList(HttpListenerRequest reqeust)
        {
            string resp = "[{\"id\":18,\"name\":\"\",\"create_date\":\"2018-06-14T02:14:03.657011\",\"password\":\"secret+2018-06-14 02:14:03.654015+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":17,\"name\":\"\",\"create_date\":\"2018-06-14T02:14:00.541601\",\"password\":\"secret+2018-06-14 02:14:00.538626+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":16,\"name\":\"\",\"create_date\":\"2018-06-14T02:13:57.313670\",\"password\":\"secret+2018-06-14 02:13:57.311173+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":15,\"name\":\"\",\"create_date\":\"2018-06-14T02:13:56.744582\",\"password\":\"secret+2018-06-14 02:13:56.741089+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":14,\"name\":\"\",\"create_date\":\"2018-06-14T02:13:55.989790\",\"password\":\"secret+2018-06-14 02:13:55.986795+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":13,\"name\":\"\",\"create_date\":\"2018-06-14T02:13:51.180367\",\"password\":\"secret+2018-06-14 02:13:51.178867+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":12,\"name\":\"\",\"create_date\":\"2018-06-14T02:06:55.854370\",\"password\":\"secret+2018-06-14 02:06:55.849379+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":11,\"name\":\"\",\"create_date\":\"2018-06-14T02:06:54.820843\",\"password\":\"secret+2018-06-14 02:06:54.817832+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":10,\"name\":\"\",\"create_date\":\"2018-06-14T02:06:53.881190\",\"password\":\"secret+2018-06-14 02:06:53.879694+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":9,\"name\":\"\",\"create_date\":\"2018-06-14T02:06:52.377559\",\"password\":\"secret+2018-06-14 02:06:52.376050+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":8,\"name\":\"\",\"create_date\":\"2018-06-14T02:06:51.355764\",\"password\":\"secret+2018-06-14 02:06:51.353270+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":7,\"name\":\"\",\"create_date\":\"2018-06-14T02:06:47.838451\",\"password\":\"secret+2018-06-14 02:06:47.835936+somesalt\",\"created_by\":\"<UNKNOWN>\"},{\"id\":6,\"name\":\"\",\"create_date\":\"2018-06-14T02:06:47.022977\",\"password\":\"secret+2018-06-14 02:06:47.020980+somesalt\",\"created_by\":\"<UNKNOWN>\"}]";
            return resp;
        }
    }
}
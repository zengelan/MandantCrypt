using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MandantCrypt

{
    public class Mandant
    {
        public double id { get; set; }
        public String name { get; set; }
        public int number { get; set; }
        public DateTime create_date { get; set; }
        public DateTime last_modified { get; set; }
        public DateTime latest_password_created { get; set; }
        public String note { get; set; }
        public bool active { get; set; }
        public override string ToString()
        {
            return String.Format("{0} {1}", this.number,this.name);
        }
    }

    public class Password
    {
        public double id { get; set; }
        public String name { get; set; }
        public DateTime create_date { get; set; }
        public String password { get; set; }
        public String password_decrypted { get; set; }
        public String created_by { get; set; }
    }

    public class ApiClient
    {

        private const String ApiPath = "/api/v1/";
        private const String JsonContentType = "application/json; charset=utf-8";
        public String serviceUrl { get; }
        private RestClient client;
        private string username;
        private string password;


        public ApiClient(String serviceBaseUrl)
        {
            this.serviceUrl = serviceBaseUrl + ApiPath;
            // TODO: Implement auth
            this.username = "admin";
            this.password = "Password123!";

        }

        public List<Mandant> getActiveMandantList()
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "activeMandantList/";
            request.RootElement = "Mandant";
            List<Mandant> mlist = this.Execute<List<Mandant>>(request);
            return mlist;
        }

        internal string getDecryptedPassword(double mandantId)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "currentMandantPassword/";
            request.AddQueryParameter("mandant_id", String.Format("{0}",mandantId));
            request.RootElement = "Password";
            Password pass = this.Execute<Password>(request);
            return pass.password_decrypted;
        }

        private RestClient getclient()
        {
            client = new RestClient(this.serviceUrl);
            client.Authenticator = new HttpBasicAuthenticator(this.username, this.password);
            client.BaseUrl = new System.Uri(this.serviceUrl);
            return client;

        }

        private T Execute<T>(RestRequest request) where T : new()
        {
            var client = getclient();
            request.AddHeader("Content-Type", JsonContentType);
            request.AddHeader("Accept", JsonContentType);
            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var twilioException = new ApplicationException(message, response.ErrorException);
                throw twilioException;
            }
            return response.Data;
        }
    }
}

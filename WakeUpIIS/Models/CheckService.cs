namespace Models
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Net;

   using ServiceStack.ServiceClient.Web;
   using ServiceStack.Text;

   public class CheckService
   {
      private ExchangeGateway gateway;

      private JsonServiceClient serviceClient;

      private DateTime lastLogDate;


      public CheckService(ServiceData serviceData)
      {
         this.lastLogDate = DateTime.Now.Date;

         this.gateway = new ExchangeGateway(serviceData.OutlookUrl, serviceData, false);
         this.serviceClient = new JsonServiceClient(serviceData.UrlToWakeUp)
         {
            Credentials = CredentialCache.DefaultNetworkCredentials,
            Timeout = TimeSpan.FromMinutes(10)
         };

      }

      public void StartCheckServer()
      {
         var message = this.CheckServerStatus();
         if (!string.IsNullOrEmpty(message))
         {
            this.gateway.SendMessage(message);
         }

         var logMessage = this.CheckLogForServer();
         if (!string.IsNullOrEmpty(logMessage))
         {
            this.gateway.SendMessage(logMessage);
         }
      }

      private string CheckLogForServer()
      {
         Console.WriteLine("Checking Logs in the server");
         var response = string.Empty;
         try
         {
            var responseJson = this.serviceClient.Get<string>(string.Format("logs?Date={0}", DateTime.Now.ToString("yyyy/MM/dd")));
            var jsonResponse = JsonObject.Parse(responseJson);
            if (jsonResponse.Get<bool>("hasError"))
            {
               var errorMessage = jsonResponse.Get<string>("message");
               var innerException = jsonResponse.Get<Exception>("exception");
               Console.WriteLine("Error when retrieve Logs: " + errorMessage);
               throw new Exception(errorMessage, innerException);
            }
            var logs = jsonResponse.Get<List<Log>>("Logs");
            if (!logs.Any())
            {
               response = "There is not Log for today, please check the Office365ManagerService";
            }

            if (logs.Any(x => x.Level.Equals("Error") && x.TimeStamp > this.lastLogDate))
            {
               response = "<p>Errors for today</p>";
               foreach (Log log in logs.Where(x => x.Level.Equals("Error") && x.TimeStamp > this.lastLogDate))
               {

                  response += "<p>" + log.Message + "</p>";
                  response += "<p>" + log.Level + "</p>";
                  response += "<p>" + log.TimeStamp + "</p>";
                  response += "<p>" + log.Properties + "</p>";
                  response += "<p></p>";
               }

               this.lastLogDate = DateTime.Now;
            }
         }
         catch (Exception ex)
         {
            response = ex.Message;
         }

         Console.WriteLine("Check Logs in the server finished: " + response);
         return response;
      }

      private string CheckServerStatus()
      {
         Console.WriteLine("Checking Server Status");
         var response = string.Empty;
         try
         {
            var responseJson = this.serviceClient.Get<string>("serverstats");
            var jsonResponse = JsonObject.Parse(responseJson);
            if (jsonResponse.Get<bool>("hasError"))
            {
               response = jsonResponse.Get<string>("message");
               Console.WriteLine("Wrong Server Status: " + response);
            }
         }
         catch (Exception ex)
         {
            response = ex.Message;
            Console.WriteLine("Wrong Server Status: " + ex.Message);
         }

         return response;
      }
   }
}

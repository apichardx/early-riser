namespace Models
{
   using System;

   using Microsoft.Exchange.WebServices.Data;
   using System.Net;

   public class ExchangeGateway : IDisposable
   {
      private string exchangeUrl;

      private bool enabledTrace;

      private ExchangeService service;

      private ServiceData serviceData;


      public ExchangeGateway(string exchangeWebServiceUrl, ServiceData serviceData, bool enabledTrace)
      {
         this.enabledTrace = enabledTrace;
         this.exchangeUrl = exchangeWebServiceUrl;
         this.serviceData = serviceData;
      }

      public ExchangeService Service
      {
         get
         {
            return this.service
                   ?? (this.service =
                       new ExchangeService(ExchangeVersion.Exchange2010_SP2)
                          {
                             TraceEnabled = this.enabledTrace,
                             Credentials =
                                CredentialCache
                                .DefaultNetworkCredentials,
                             EnableScpLookup = false,
                             Url = new Uri(this.exchangeUrl)
                          });
         }
      }

      public bool SendMessage(string message)
      {
         try
         {
            Console.WriteLine("Sending message to : {0}", this.serviceData.EmailTo);
            var emailMessage = new EmailMessage(this.Service)
                                  {
                                     From = new EmailAddress("Checkin Services", this.serviceData.EmailFrom),
                                     Subject = "Report from Checking Services"
                                  };
            var toRecipients = this.serviceData.EmailTo.Split(';');
            foreach (var recipient in toRecipients)
            {
               emailMessage.ToRecipients.Add(new EmailAddress(recipient));
            }

            var messageBody = new MessageBody(BodyType.HTML, message);
            emailMessage.Body = messageBody;
            emailMessage.Importance = Importance.High;
            emailMessage.Send();
            Console.WriteLine("Message sended");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error when send mail: {0}", ex.Message);
            return false;
         }
      }

      public void Dispose()
      {
         this.service = null;
         this.exchangeUrl = string.Empty;
      }

   }
}

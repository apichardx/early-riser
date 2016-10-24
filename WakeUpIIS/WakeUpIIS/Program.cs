namespace WakeUpIIS
{
   using System;
   using System.Threading;

   using Models;

   class Program
   {
      static void Main(string[] args)
      {
         var serviceData = new ServiceData();
         Console.WriteLine("Outlook URL:");
         serviceData.OutlookUrl = Console.ReadLine();
         Console.WriteLine("Enter Email From:");
         serviceData.EmailFrom = Console.ReadLine();
         Console.WriteLine("Enter Email to notifi separed by ';':");
         serviceData.EmailTo = Console.ReadLine();
         Console.WriteLine("Enter URL to wake up:");
         serviceData.UrlToWakeUp = Console.ReadLine();
         
         var service = new CheckService(serviceData);
         Console.WriteLine("Starting Wake Up IIS");

         var callBack = new TimerCallback(Tick);
         var timer = new Timer(callBack, service, 0, 300000);
         
         for (;;)
         {
            Thread.Sleep(30000);
         }
      }

      static public void Tick(Object stateInfo)
      {
         Console.WriteLine("Processing... {0}", DateTime.Now.ToShortTimeString());
         ((CheckService)stateInfo).StartCheckServer();
      }
   }
}

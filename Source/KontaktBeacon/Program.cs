using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http.Headers;
using KontaktBeacon.StaticTools;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace KontaktBeacon
{
    class Program
    {

        private static HttpClient _client = new HttpClient();

        static void Main(string[] args)
        {
            SetWebAddress();
            SetHeaders();

            Thread handleConsoleInputThread = new Thread(_ => HandleConsoleInput());
            handleConsoleInputThread.IsBackground = true;
            handleConsoleInputThread.Start();

            ManualResetEvent readExistingEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eArgs) => { readExistingEvent.Set(); eArgs.Cancel = true; };
            readExistingEvent.WaitOne();
        }

        private static void HandleConsoleInput()
        {
            ConsoleKeyInfo keyinfo;
            while (true)
            {
                keyinfo = Console.ReadKey(true);
                if ((keyinfo.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    if (keyinfo.Key == ConsoleKey.Q) GetDevices();
                }
            }
        }


        private static void SetWebAddress()
        {
            _client.BaseAddress = new Uri(Config.WebAddr);
            _client.DefaultRequestHeaders.Clear();
        }

        private static void SetHeaders()
        {
            _client.DefaultRequestHeaders.Add("Accept", Config.AcceptHeader);
            _client.DefaultRequestHeaders.Add("Api-Key", Config.PrivateAPIKey);
        }

        private static void GetDevices()
        {
            HttpResponseMessage response = _client.GetAsync("/device?q=uniqueId==2TEY").GetAwaiter().GetResult();
            string content = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).ToString();
            Console.Write(content);
        }
    }
}

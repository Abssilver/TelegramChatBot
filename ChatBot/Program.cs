using System;
using System.Net;
using Telegram.Bot;
using System.Xml.Linq;
using System.Linq;
using System.Net.Http;

namespace ChatBot
{
    class Program
    {
        static TelegramBotClient botClient;
        static void Main(string[] args)
        {
            string token = "yourTelegramBotToken";

            var proxy = new WebProxy(Address: new Uri("http://163.172.189.32:8811"));
            var httpClientProxy = new HttpClientHandler()
            {
                Proxy = proxy
            };
            var hc = new HttpClient(handler: httpClientProxy, disposeHandler: true);


            botClient = new TelegramBotClient(token, hc);
            botClient.OnMessage += BotClient_OnMessage;
            
            botClient.StartReceiving();
            Console.ReadLine();
        }
        private static void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string [] city = e.Message.Text.ToLower().Split(' ');
            Console.WriteLine($"{e.Message.Chat.FirstName} {city}");

            WebClient client = new WebClient();
            var xml = "";

            if (city[0] == "москва") xml = client.DownloadString(@"https://xml.meteoservice.ru/export/gismeteo/point/32277.xml");
            else if (city[0] == "санкт-петербург") xml = client.DownloadString(@"https://xml.meteoservice.ru/export/gismeteo/point/69.xml");
            else xml = "Такой город не знаю";

            if (xml != "Такой город не знаю")
            {
                var colllection = XDocument.Parse(xml)
                    .Descendants("MMWEATHER")
                    .Descendants("REPORT")
                    .Descendants("TOWN")
                    .Descendants("FORECAST")
                    .ToList();

                string res = "";
                if (city.Length>1)
                {
                    if (city[1] == "т") res += $"{colllection[0].Element("TEMPERATURE").Attribute("max").Value}°C\n";
                    if (city[1] == "д") res += $"{colllection[0].Element("PRESSURE").Attribute("max").Value}мм. рт. ст.\n";
                    if (city.Length>2)
                    {
                        if (city[2] == "т") res += $"{colllection[0].Element("TEMPERATURE").Attribute("max").Value}°C\n";
                        if (city[2] == "д") res += $"{colllection[0].Element("PRESSURE").Attribute("max").Value}мм. рт. ст.\n";
                    }
                }
                botClient.SendTextMessageAsync(e.Message.Chat.Id, res);

            }
            else botClient.SendTextMessageAsync(e.Message.Chat.Id, xml);
        }
    }
}

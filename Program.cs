using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using System.Linq;

namespace ColorsInfo_Bot
{
    class Program
    {
        private static TelegramBotClient? Bot;

        public static async Task Main()
        {
            Bot = new TelegramBotClient("5098945657:AAH1ianS-uP018YcT41_NlIAHL5gFMB-sG4");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "Colors Info Bot";
            using var cts = new CancellationTokenSource();
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
            cts.Cancel();

        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }


        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }



        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;


            var action = message.Text switch
            {
                "/help" or "/start" => help(botClient, message),
                _ => getColorInfo(botClient, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");


            static async Task<Message> help(ITelegramBotClient botClient, Message message)
            {


                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Hi!!\n" +
                                                                  "please type the name of the color you want to know information about."
                                                            );
            }



            static async Task<Message> getColorInfo(ITelegramBotClient botClient, Message message)
            {


                StreamReader r = new StreamReader("D://colors.json");

                string json = r.ReadToEnd();
                var jsonD = JsonConvert.DeserializeObject<List<Root>>(json);
                Console.WriteLine($"message.Text: {message.Text.ToUpper()}");
                List<string> generalAnswer = new List<string>();
                List<string> childs = new List<string>();

                //string[] generalAnswer;
                //string[] childs;

                //List<string> childCompany = new List<string>();


                foreach (var i in jsonD)
                {
                    if ((message.Text.ToUpper().Equals(i.name.ToUpper())))
                    {
                        Console.WriteLine($"dsfsdf");
                        generalAnswer.Add(i.name);
                        generalAnswer.Add(i.munsell_hue);
                        generalAnswer.Add(i.hex);
                        foreach (var j in i.children)
                        {
                            childs.Add(j.name + " - " + j.company + " - " + j.hex);
                        }
              
                    }

                }
              


                Console.WriteLine($"1111: {generalAnswer.Count()}");


                if (generalAnswer.Count() != 0)
                {
                    Console.WriteLine($"055555555: {!generalAnswer.Any()}");
                    return await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text:   "<b>Color Name:</b> " + generalAnswer[0] + "\n" +
                                                    "<b>Munsell Hue:</b> " + generalAnswer[1] + "\n" +
                                                    "<b>Hex Code:</b> " + generalAnswer[2] + "\n" +
                                                    "<b>Color Childrens:</b>\n" + string.Join(",\n ", childs)
                                            ,
                                            disableNotification: true,
                                            parseMode: ParseMode.Html,
                                            replyToMessageId: message.MessageId);
                }



                Console.WriteLine($"else");
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                       text: "/help - Get help\n" +
                                                             "Please make sure to write the color name correctly\n");





            }
        }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Child
    {
        public string name { get; set; }
        public string company { get; set; }
        public string media { get; set; }
        public string hue_bias { get; set; }
        public string value { get; set; }
        public string chroma { get; set; }
        public string temp { get; set; }
        public string opacity { get; set; }
        public string pigment { get; set; }
        public string paint_chip { get; set; }
        public string pantone { get; set; }
        public string hex { get; set; }
        public string rgb { get; set; }
        public string htmlname { get; set; }
        public List<object> children { get; set; }
    }

    public class Root
    {
        public string name { get; set; }
        public string munsell_hue { get; set; }
        public string hex { get; set; }
        public List<Child> children { get; set; }
    }



}

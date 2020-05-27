using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotsLibrary;
using System.IO;
using Newtonsoft.Json;

namespace FinLitBot
{
    class Program
    {
        static Random random = new Random();

        static Dictionary<long, UserInfo> userValues = new Dictionary<long, UserInfo>();

        static ITelegramBotClient botClient;




        static void Main()
        {
            botClient = new TelegramBotClient(Data.Token);

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"FinLitBot is running. BotID: {me.Id}; BotName: {me.FirstName}.\n\nLogs will be written here.");

            string dir = Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            try
            {
                //string key = System.IO.File.ReadAllText("config.txt");
                foreach (var file in directoryInfo.GetFiles()) //проходим по файлам
                {
                    string[] name = file.Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    //получаем расширение файла и проверяем подходит ли оно нам 
                    if (name[1] == "json")
                    {
                        long id = long.Parse(name[0]);

                        UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(System.IO.File.ReadAllText(file.FullName));
                        //Console.WriteLine("Added info " + user.id);
                        userValues.Add(id, userInfo);
                    }

                }
            }
            catch (IOException)
            {
                Console.WriteLine("Ошибка чтения файла");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            botClient.OnMessage += Bot_OnMessage;

            botClient.OnCallbackQuery += GettingTestAnswers;
            botClient.OnCallbackQuery += GettingInlineKeyboardCallbacks;

            botClient.StartReceiving();

            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            var text = e.Message.Text;
            var chat = e.Message.Chat;

            bool understood = false;

            if (!userValues.TryGetValue(chat.Id, out UserInfo _userInfo))
                userValues.Add(chat.Id, new UserInfo(chat.Id));

            if (text == "/start")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received a start command message in chat {chat.Id} {chat.FirstName} {chat.LastName} {chat.Username}.");
                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text: "Привет! Я помогу тебе улучшить свои знания по финансовой грамотности " +
                  "и перестать делать ненужные покупки",
                  replyMarkup: Keyboards.MainKeyboardMarkup
                );
            }

            UserInfo userInfo = userValues[chat.Id];

            if (text == "Финансовая грамотность")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received a finlit command message in chat {chat.Id}.");
                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text: "Здесь ты можешь узнать новое из мира финансов и проверить себя"
                );

                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text: "Выбери интересующую функцию:",
                  replyMarkup: Keyboards.FinLitReplyKeyboardMarkup
                );
            }
            if (text == "Контроль расходов")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                if (userInfo.DailyLimit == -1)
                {
                    Console.WriteLine($"Received a first expenses control command message in chat {chat.Id}.");
                    await botClient.SendTextMessageAsync(
                      chatId: chat.Id,
                      text: "Я помогу тебе перестать делать ненужные покупки. Установи свой дневной лимит, записывай все траты — а я напомню, когда ты превысишь лимит",
                      replyMarkup: new ReplyKeyboardRemove()
                    );

                    await botClient.SendTextMessageAsync(
                      chatId: chat.Id,
                      text: "Введи свой дневной лимит"
                    );
                    userInfo.isEdittingLimit = true;
                }
                else
                {
                    Console.WriteLine($"Received an expenses control command message in chat {chat.Id}.");

                    string newText;

                    if (userInfo.DailyLimit - userInfo.SumOfExpenses(DateTime.Today) >= 0)
                        newText = $"Можешь потратить ещё {userInfo.DailyLimit - userInfo.SumOfExpenses(DateTime.Today)}₽";
                    else
                        newText = $"Превышение дневного лимита на {userInfo.SumOfExpenses(DateTime.Today) - userInfo.DailyLimit}₽";

                    await botClient.SendTextMessageAsync(
                      chatId: chat.Id,
                      text: $"Твой дневной лимит: {userInfo.DailyLimit}₽.\n\nСегодня ты потратил {userInfo.SumOfExpenses(DateTime.Today)}₽\n" + newText,
                      replyMarkup: Keyboards.ControlReplyKeyboardMarkup
                    );
                }
            }

            if (text == "Изменить лимит")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                await botClient.SendTextMessageAsync(
                      chatId: chat.Id,
                      text: "Введи новый дневной лимит"
                    );

                userInfo.isEdittingLimit = true;
            }

            if (double.TryParse(text, out double newLimit) && userInfo.isEdittingLimit)
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                userInfo.DailyLimit = newLimit;

                Console.WriteLine($"Received a set daily limit command in chat {chat.Id}.");
                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text: $"Отлично! Теперь твой дневной лимит {userInfo.DailyLimit}₽.\nНе забывай отправлять мне все свои расходы ;)",
                  replyMarkup: Keyboards.ControlReplyKeyboardMarkup
                );

                userInfo.isEdittingLimit = false;

                userInfo.Save();
            }

            if (text == "Записать расход")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                await botClient.SendTextMessageAsync(
                      chatId: chat.Id,
                      text: "Введи сумму расхода",
                      replyMarkup: new ReplyKeyboardRemove()
                );

                userInfo.isAddingExpense = true;
            }

            if (double.TryParse(text, out double amount) && userInfo.isAddingExpense)
            {
                understood = true;

                userInfo.UserExpenses.Add(new Expenses { Amount = amount, Date = DateTime.Now });

                Console.WriteLine($"Received a start command message in chat {chat.Id}.");

                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text: $"Записал трату. " +
                  $"\nСумма {userInfo.UserExpenses[userInfo.UserExpenses.Count - 1].Amount}₽. Дата {userInfo.UserExpenses[userInfo.UserExpenses.Count - 1].Date.ToShortDateString()}"
                );

                double balance = userInfo.DailyLimit - userInfo.SumOfExpenses(userInfo.UserExpenses[userInfo.UserExpenses.Count - 1].Date);
                string msgText;
                if (balance > 0)
                {
                    msgText = $"Ты укладываешься в лимит. На сегодня осталось ещё {userInfo.DailyLimit - userInfo.SumOfExpenses(userInfo.UserExpenses[userInfo.UserExpenses.Count - 1].Date)}₽";
                }
                else if (balance == 0)
                {
                    msgText = $"Воу, ты уложился ровно в лимтит. Расходы за сегодня: {userInfo.SumOfExpenses(userInfo.UserExpenses[userInfo.UserExpenses.Count - 1].Date)}₽";
                }
                else
                {
                    msgText = $"Эх, ты превысил дневной лимит на {-1 * balance}₽";
                }

                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text = msgText,
                  replyMarkup: Keyboards.ControlReplyKeyboardMarkup
                  );

                userInfo.isAddingExpense = false;

                userInfo.Save();
            }

            if (text == "Получить совет")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received a request for an Advice #{userInfo.AdviceId} in chat {chat.Id}.");

                SendAdvice(null, chat.Id);
            }

            if (text == "Где узнать больше?")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received a request for an Info #{userInfo.ResourceId} in chat {chat.Id}.");

                SendResource(null, chat.Id);
            }

            if (text == "Финансовый словарь")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received a request for an FinDictinary #{userInfo.AdviceId} in chat {chat.Id}.");

                SendFinTerm(null, chat.Id);
            }

            if (text == "Пройти тест")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received a Test request in chat {chat.Id}.");

                if (userInfo.QuestionId >= (Data.Questions.Length - 1))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chat.Id,
                        text: "Ты ответил на все вопросы. Ура!",
                        replyMarkup: Keyboards.FinLitReplyKeyboardMarkup
                    );
                }
                else
                    SendTest(null, chat.Id);
            }

            if (text == "Дать отпор мошенникам")
            {
                understood = true;

                Console.WriteLine($"Received a Game request in chat {chat.Id}.");

                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"На часах {DateTime.Now.ToShortTimeString()}. Тебе звонят с незнакомого номера.",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new ReplyKeyboardRemove()
                        );

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_Добрый день, {chat.FirstName} {chat.LastName}! Это служба безопасности Вашего банка._",
                            parseMode: ParseMode.Markdown
                        );

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_В нашу систему реагирования поступила информация о попытке снятия денежных средств с Вашей карты._",
                            parseMode: ParseMode.Markdown
                        );

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_Транзакция подозрительная, решили обратиться к Вам. Если не отменить перевод, деньги навсегда уйдут._",
                            parseMode: ParseMode.Markdown
                        );

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_Вы переводили 15700 Наталье Шевченко?_",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: Keyboards.Game1Keyboard
                        );
                userInfo.isPlayingInGame1 = true;
            }

            if (text == "Нет" && userInfo.isPlayingInGame1)
            {
                understood = true;

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_Понял Вас. Не переживайте, мы Вам поможем. Сумма крупная, нужно торопиться, пока есть возможность отменить._",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new ReplyKeyboardRemove()
                        );

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_Отправил Вам СМС с кодом отмены. Введите код из СМС, наша программа его проверит, и мы сможем заморозить транзакцию." +
                            $"\nВ целях конфиденциальности я включаю программу-робот, которая защитит ваши конфиденциальные данные._",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: Keyboards.Game2Keyboard
                        );
            }

            if (text == "Держите код: 1533" && userInfo.isPlayingInGame1)
            {
                understood = true;

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_Спасибо. Сейчас система проверит код._",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new ReplyKeyboardRemove()
                        );

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1500);
                await botClient.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: $"_Отлично, всё верно, отменили транзакцию. Теперь наконец жене шубу куплю. Ой, это не Вам._",
                    parseMode: ParseMode.Markdown
                );

                Message message = await botClient.SendPhotoAsync(
                    chatId: chat.Id,
                    photo: "https://i.ibb.co/2t3Qctw/2.jpg"
                );

                await botClient.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: $"Ты не смог дать отпор мошенникам :(\n\nВот несколько советов:\nЗапиши номер банка в адресную книгу своего телефона: если звонок будет с другого номера, он отобразится как неизвестный." +
                    $"\nНе совершай никаких операций по инструкциям звонящего. Все операции для защиты карты сотрудник банка делает сам." +
                    $"\nСразу заканчивай разговор. Работник банка никогда не попросит у тебя секретные данные от карты или интернет-банка." +
                    $"\nПроверь, не было ли сомнительных операций за время разговора. Если успел что-то сообщить мошенникам, сразу позвони в банк и сообщи о случившемся.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: Keyboards.FinLitReplyKeyboardMarkup
                );

                userInfo.isPlayingInGame1 = false;
            }

            if ((text == "Да" || text == "Ясно, до свидания") && userInfo.isPlayingInGame1)
            {
                understood = true;

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1000);
                await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"_Правильно понимаю, что Вы просекли развод?_",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new ReplyKeyboardRemove()
                        );

                await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);
                await Task.Delay(1000);
                await botClient.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: $"_Всего Вам доброго, пойду искать более наивных людей. Вы молодец!_",
                    parseMode: ParseMode.Markdown
                );

                Message message = await botClient.SendPhotoAsync(
                    chatId: chat.Id,
                    photo: "https://i.ibb.co/XWpR0Fx/d204e263c72e0b0a488c5404bb6bd5a1-min-2.jpg"
                );

                await botClient.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: $"Супер! Ты смог устоять перед напором мошенников :)\n\nНа всякий случай вот тебе несколько советов:\nЗапиши номер банка в адресную книгу своего телефона: если звонок будет с другого номера, он отобразится как неизвестный." +
                    $"\nНе совершай никаких операций по инструкциям звонящего. Все операции для защиты карты сотрудник банка делает сам." +
                    $"\nСразу заканчивай разговор. Работник банка никогда не попросит у тебя секретные данные от карты или интернет-банка." +
                    $"\nПроверь, не было ли сомнительных операций за время разговора. Если успел что-то сообщить мошенникам, сразу позвони в банк и сообщи о случившемся.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: Keyboards.FinLitReplyKeyboardMarkup
                );

                userInfo.isPlayingInGame1 = false;
            }

            if (text == "Назад")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received a return back request in chat {chat.Id}.");

                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text: "Я помогу тебе улучшить свои знания по финансовой грамотности " +
                  "и перестать делать ненужные покупки",
                  replyMarkup: Keyboards.MainKeyboardMarkup
                );
            }

            if (text == "info")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                Console.WriteLine($"Received an Info request in chat {chat.Id}.");
                GetInfo(chat.Id);
            }

            if (text == "/systemInfo")
            {
                understood = true;

                // Simulate longer running task
                BotTypes(chat.Id);

                long countOfUsers = 0;
                if (chat.Id == Data.AdminId)
                {
                    string systemInfo = "";
                    foreach (KeyValuePair<long, UserInfo> keyValue in userValues)
                    {
                        Console.WriteLine(keyValue.Key + " - " + keyValue.Value.ToString());
                        systemInfo += $"{keyValue.Key}  -  {keyValue.Value.ToString()}";
                        countOfUsers++;
                    }

                    Console.WriteLine($"Received a SystemInfo command from admin in chat {chat.Id}.");
                    await botClient.SendTextMessageAsync(
                      chatId: chat.Id,
                      text: $"Count of users in the System: {countOfUsers} \n{systemInfo}"
                    );
                }
                else
                {
                    Console.WriteLine($"Received a SystemInfo command from stanger in chat {chat.Id}. Access denied.");
                    await botClient.SendTextMessageAsync(
                      chatId: chat.Id,
                      text: "Извините, данная команда предназначена только для администратора."
                    );

                }
            }

            if (!understood)
            {
                BotTypes(chat.Id);

                Console.WriteLine($"Received not recognisded command in chat {chat.Id}. Access denied.");
                await botClient.SendTextMessageAsync(
                  chatId: chat.Id,
                  text: "Извини, я тебя не понимаю :(",
                  replyMarkup: Keyboards.MainKeyboardMarkup
                );

                userInfo.isAddingExpense = false;
                userInfo.isEdittingLimit = false;
                userInfo.isPlayingInGame1 = false;
                userInfo.Save();
            }
        }

        // Process Inline Keyboard callback data    
        private static async void GettingInlineKeyboardCallbacks(object sender, CallbackQueryEventArgs e)
        {
            var inlineKey = e.CallbackQuery.Data;
            var chatID = e.CallbackQuery.Message.Chat.Id;

            if (!userValues.TryGetValue(chatID, out UserInfo _userInfo))
                userValues.Add(chatID, new UserInfo(chatID));

            UserInfo userInfo = userValues[chatID];

            if (inlineKey == "nextAdvice")
            {
                // Simulate longer running task
                BotTypes(chatID);

                Console.WriteLine($"Received a request for an Advice #{userInfo.AdviceId} in chat {chatID}.");

                SendAdvice(e, chatID);
            }

            if (inlineKey == "nextTerm")
            {
                // Simulate longer running task
                BotTypes(chatID);

                Console.WriteLine($"Received a request for an Term #{userInfo.AdviceId} in chat {chatID}.");

                SendFinTerm(e, chatID);
            }

            if (inlineKey == "nextResource")
            {
                // Simulate longer running task
                BotTypes(chatID);

                Console.WriteLine($"Received a request for a Resource #{userInfo.AdviceId} in chat {chatID}.");

                SendResource(e, chatID);
            }



            if (inlineKey == "afterAdviceBack" || inlineKey == "afterTermBack"  || inlineKey == "afterResource")
            {
                // Simulate longer running task
                BotTypes(chatID);

                if (e != null)
                    await botClient.DeleteMessageAsync(chatID, e.CallbackQuery.Message.MessageId);

                await botClient.SendTextMessageAsync(
                  chatId: chatID,
                  text: "Выбери интересующую функцию:",
                  replyMarkup: Keyboards.FinLitReplyKeyboardMarkup
                );
            }
        }

        // Process Inline Keyboard callback data    
        private static void GettingTestAnswers(object sender, CallbackQueryEventArgs e)
        {
            var data = e.CallbackQuery.Data;
            var chatID = e.CallbackQuery.Message.Chat.Id;

            if (!userValues.TryGetValue(chatID, out UserInfo _userInfo))
                userValues.Add(chatID, new UserInfo(chatID));

            UserInfo userInfo = userValues[chatID];



            if (data == "answer00" || data == "answer01" || data == "answer02" ||
                data == "answer10" || data == "answer11" || data == "answer12" ||
                data == "answer20" || data == "answer21" || data == "answer22" ||
                data == "answer30" || data == "answer31" || data == "answer32")
            {
                userInfo.UsersAnswers[userInfo.QuestionId] = data;
                userInfo.QuestionId++;
                userInfo.Save();
                SendTest(e, chatID);
            }

            if (data == "answer40" || data == "answer41" || data == "answer42")
            {
                userInfo.UsersAnswers[userInfo.QuestionId] = data;
                userInfo.Save();
                SendScore(e, chatID);
            }
        }

        private static async void BotTypes(long chatID)
        {
            // Simulate longer running task
            await botClient.SendChatActionAsync(chatID, ChatAction.Typing);
            await Task.Delay(500);
        }

        private static async void BotTypesLong(long chatID)
        {
            // Simulate longer running task
            await botClient.SendChatActionAsync(chatID, ChatAction.Typing);
            await Task.Delay(3000);
        }

        private static async void SendAdvice(CallbackQueryEventArgs e, long chatID)
        {
            UserInfo userInfo = userValues[chatID];

            if (e != null)
                await botClient.DeleteMessageAsync(chatID, e.CallbackQuery.Message.MessageId);

            if (userInfo.AdviceId == Data.Advices.Length)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatID,
                    text: "Ты прочитал все советы. Ура!",
                    replyMarkup: Keyboards.FinLitReplyKeyboardMarkup
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatID,
                    text: Data.Advices[userInfo.AdviceId],
                    replyMarkup: new ReplyKeyboardRemove()
                );

                await botClient.SendTextMessageAsync(
                    chatId: chatID,
                    text: "Выбери следующее действие:",
                    replyMarkup: Keyboards.AfterAdviceInlineKeyboard
                );

                userInfo.AdviceId++;
                userInfo.Save();
            }
        }

        private static async void SendResource(CallbackQueryEventArgs e, long chatID)
        {
            UserInfo userInfo = userValues[chatID];

            if (e != null)
                await botClient.DeleteMessageAsync(chatID, e.CallbackQuery.Message.MessageId);


            await botClient.SendTextMessageAsync(
                chatId: chatID,
                text: Data.SourcesOfNewInformation[userInfo.ResourceId % Data.SourcesOfNewInformation.Length],
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.Html
            );

            await botClient.SendTextMessageAsync(
                chatId: chatID,
                text: "Выбери следующее действие:",
                replyMarkup: Keyboards.AfterResourceInlineKeyboard
            );

            userInfo.ResourceId++;
            userInfo.Save();

        }

        private static async void SendFinTerm(CallbackQueryEventArgs e, long chatID)
        {
            UserInfo userInfo = userValues[chatID];

            if (e != null)
                await botClient.DeleteMessageAsync(chatID, e.CallbackQuery.Message.MessageId);

            if (userInfo.FinTermId == Data.FinTerms.Length)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatID,
                    text: "Ты узнал все термины. Ура!",
                    replyMarkup: Keyboards.FinLitReplyKeyboardMarkup
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatID,
                    text: Data.FinTerms[userInfo.FinTermId],
                    replyMarkup: new ReplyKeyboardRemove()
                );

                await botClient.SendTextMessageAsync(
                    chatId: chatID,
                    text: "Выбери следующее действие:",
                    replyMarkup: Keyboards.AfterFinTermInlineKeyboard
                );

                userInfo.FinTermId++;
                userInfo.Save();
            }
        }

        private static async void SendTest(CallbackQueryEventArgs e, long chatID)
        {
            UserInfo userInfo = userValues[chatID];


            var answer = new InlineKeyboardMarkup(new[]
            {
                    // first row - first answer
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"{Data.Answers[userInfo.QuestionId,0]}", $"answer{userInfo.QuestionId}0")
                    },
                    // second row - second answer
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"{Data.Answers[userInfo.QuestionId,1]}", $"answer{userInfo.QuestionId}1")
                    },
                    // third row - third answer
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"{Data.Answers[userInfo.QuestionId,2]}", $"answer{userInfo.QuestionId}2")
                    }
                });

            await botClient.SendTextMessageAsync(
                chatId: chatID,
                text: Data.Questions[userInfo.QuestionId],
                replyMarkup: answer
            );

            if (e != null)
                await botClient.EditMessageReplyMarkupAsync(chatID, e.CallbackQuery.Message.MessageId, replyMarkup: null);
        }

        private static async void SendScore(CallbackQueryEventArgs e, long chatID)
        {
            UserInfo userInfo = userValues[chatID];

            if (e != null)
                await botClient.EditMessageReplyMarkupAsync(chatID, e.CallbackQuery.Message.MessageId, replyMarkup: null);

            string[] correctAnswers = { "answer00", "answer10", "answer22", "answer31", "answer42" };
            for (int i = 0; i < correctAnswers.Length; i++)
            {
                if (correctAnswers[i] == userInfo.UsersAnswers[i])
                {
                    Console.WriteLine($"Answer #{i} is correct in chat {chatID}.");
                    userInfo.Score++;
                }
                else
                    Console.WriteLine($"Answer #{i} is incorrect in chat {chatID}.");
            }
            userInfo.Save();

            Console.WriteLine($"Score of the user {chatID} is {userInfo.Score}");
            await botClient.SendTextMessageAsync(
                chatId: chatID,
                text: "Ваш результат: " + userInfo.Score
            );
        }

        private static async void GetInfo(ChatId chatID)
        {
            string info = "";

            foreach (KeyValuePair<long, UserInfo> keyValue in userValues)
            {
                info += keyValue.Key + " - " + keyValue.Value.ToString() + "\n";
            }

            await botClient.SendTextMessageAsync(
                chatId: chatID,
                text: info,
                replyMarkup: null
            );

            Console.WriteLine(info);
        }
    }
}
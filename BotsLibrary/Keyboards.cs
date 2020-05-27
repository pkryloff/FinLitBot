using Telegram.Bot.Types.ReplyMarkups;
namespace BotsLibrary
{
    public class Keyboards
    {
        private static InlineKeyboardMarkup afterAdviceInlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            // first row - advice
            new []
            {
                InlineKeyboardButton.WithCallbackData("Получить ещё совет", "nextAdvice"),
                InlineKeyboardButton.WithCallbackData("Назад", "afterAdviceBack")
            }
        }

            );
        public static InlineKeyboardMarkup AfterAdviceInlineKeyboard
        {
            get => afterAdviceInlineKeyboard;
            set => afterAdviceInlineKeyboard = value;
        }

        private static InlineKeyboardMarkup afterResourceInlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            // first row - advice
            new []
            {
                InlineKeyboardButton.WithCallbackData("Ещё", "nextResource"),
                InlineKeyboardButton.WithCallbackData("Назад", "afterResource")
            }
        }

            );
        public static InlineKeyboardMarkup AfterResourceInlineKeyboard
        {
            get => afterResourceInlineKeyboard;
            set => afterResourceInlineKeyboard = value;
        }

        private static InlineKeyboardMarkup afterFinTermInlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            // first row - advice
            new []
            {
                InlineKeyboardButton.WithCallbackData("Новый термин", "nextTerm"),
                InlineKeyboardButton.WithCallbackData("Назад", "afterTermBack")
            }
        }

            );
        public static InlineKeyboardMarkup AfterFinTermInlineKeyboard
        {
            get => afterFinTermInlineKeyboard;
            set => afterFinTermInlineKeyboard = value;
        }

        private static ReplyKeyboardMarkup finLitReplyKeyboard = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                new KeyboardButton[] { "Получить совет", "Финансовый словарь" },
                new KeyboardButton[] { "Где узнать больше?" , "Пройти тест" },
                new KeyboardButton[] { "Дать отпор мошенникам", "Назад" }
            },
            resizeKeyboard: true
        );
        public static ReplyKeyboardMarkup FinLitReplyKeyboardMarkup
        {
            get => finLitReplyKeyboard;
            set => finLitReplyKeyboard = value;
        }

        private static ReplyKeyboardMarkup controlReplyKeyboard = new ReplyKeyboardMarkup(
           new KeyboardButton[][]
           {
                new KeyboardButton[] { "Записать расход", "Изменить лимит"},
                new KeyboardButton[] { "Назад"}
           },
           resizeKeyboard: true
        );
        public static ReplyKeyboardMarkup ControlReplyKeyboardMarkup
        {
            get => controlReplyKeyboard;
            set => controlReplyKeyboard = value;
        }

        private static ReplyKeyboardMarkup mainKeyboardMarkup = new ReplyKeyboardMarkup(
           new KeyboardButton[][]
           {
                    new KeyboardButton[] { "Финансовая грамотность", "Контроль расходов"},
           },
           resizeKeyboard: true
       );
        public static ReplyKeyboardMarkup MainKeyboardMarkup
        {
            get => mainKeyboardMarkup;
            set => mainKeyboardMarkup = value;
        }

        private static ReplyKeyboardMarkup game1Keyboard = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                new KeyboardButton[] { "Да", "Нет" }
            },
            resizeKeyboard: true
        );
        public static ReplyKeyboardMarkup Game1Keyboard
        {
            get => game1Keyboard;
            set => game1Keyboard = value;
        }

        private static ReplyKeyboardMarkup game2Keyboard = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                new KeyboardButton[] { "Держите код: 1533", "Ясно, до свидания" }
            },
            resizeKeyboard: true
        );
        public static ReplyKeyboardMarkup Game2Keyboard
        {
            get => game2Keyboard;
            set => game2Keyboard = value;
        }
    }
}

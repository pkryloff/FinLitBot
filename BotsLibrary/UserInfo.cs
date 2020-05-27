using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BotsLibrary
{
    public class UserInfo
    {
        public long UserId { get; set; }

        public int QuestionId { get; set; }

        public int AdviceId { get; set; }

        public int FinTermId { get; set; }

        public int ResourceId { get; set; }

        public int Score { get; set; }

        public double DailyLimit { get; set; }

        public string[] UsersAnswers { get; set; }

        public List<Expenses> UserExpenses { get; set; }

        public bool isEdittingLimit { get; set; }

        public bool isAddingExpense { get; set; }

        public bool isPlayingInGame1 { get; set; }

        public UserInfo(long userId)
        {
            UserId = userId;
            QuestionId = 0;
            AdviceId = 0;
            FinTermId = 0;
            ResourceId = 0;
            Score = 0;
            DailyLimit = -1;
            UsersAnswers = new string[Data.Questions.Length];
            UserExpenses = new List<Expenses>();
            isAddingExpense = false;
            isEdittingLimit = false;
            isPlayingInGame1 = false;
        }

        public double SumOfExpenses(DateTime date)
        {
            double sum = 0;

            foreach (var exp in UserExpenses)
            {
                if (exp.Date.ToShortDateString() == date.ToShortDateString())
                    sum += exp.Amount;
            }

            return sum;
        }

        public override string ToString()
        {
            return $"User with AdviceId = {AdviceId}; QuestionId = {QuestionId}; Score = {Score}";
        }

        public void Save()
        {
            try
            {
                // serialize JSON to a string and then write string to a file
                File.WriteAllText(this.UserId.ToString() + ".json", JsonConvert.SerializeObject(this));
            }
            catch
            {
                Console.WriteLine("Error while writing in file. Some data can be lost");
            }
        }
    }
}

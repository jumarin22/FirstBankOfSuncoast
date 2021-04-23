using System;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace FirstBankOfSuncoast
{
    class Program
    {
        // Transaction class supports both checking and savings, and deposits and withdraws.
        class Transaction
        {
            // Properties.
            public string Account { get; set; } // Checking or Savings. 
            public string Action { get; set; } // Deposit or Withdraw. 
            public double Amount { get; set; }
            public DateTime TransactionDate { get; set; } = DateTime.Now;

            // Methods.
            public string When()
            {
                var WhenV = TransactionDate.ToString("yyyy/MM/dd @ HH:mm:ss");
                return WhenV;
            }
        }

        static void Main(string[] args)
        {

            // Creates a stream reader to get information from our file (if empty). 
            TextReader reader;

            // If the file exists.
            if (File.Exists("transactions.csv"))
            {
                // Assign a StreamReader to read from the file. 
                reader = new StreamReader("transactions.csv");
            }
            else
            {
                // Assign a StringReader to read from an empty string. 
                reader = new StringReader("");
            }
            Console.WriteLine("Welcome to First Bank of Suncoast!");

            // Create a CSV reader to parse the stream into CSV format.
            var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Get the records from the CSV reader, as `string` and finally as a `List`.
            var transactions = csvReader.GetRecords<Transaction>().ToList();

            // Close the reader.
            reader.Close();

            // Bool to keep running program. 
            var keepRunning = true;

            while (keepRunning)
            {
                Console.WriteLine("What would you like to do?");
                Console.WriteLine("(D)eposit, (W)ithdraw, (B)alance, (Q)uit");
                var userChoice = Console.ReadLine().ToLower();

                switch (userChoice)
                {
                    case "d" or "deposit":
                        transactions.Add(Deposit());
                        break;
                    case "w" or "withdraw":
                        transactions.Add(Withdraw(transactions));
                        break;
                    case "b" or "balance":
                        Console.WriteLine($"Checking Account Balance: ${Balance(transactions, "Checking")}");
                        Console.WriteLine($"$Savings Account Balance: ${Balance(transactions, "Savings")}");
                        break;
                    case "q" or "quit":
                        keepRunning = false;
                        break;
                    default:
                        Console.WriteLine("Sorry, I didn't understand.");
                        break;
                }
            }

            // Create a stream for writing information into a file.
            var fileWriter = new StreamWriter("transactions.csv");

            // Create an object that can write CSV to the fileWriter.
            var csvWriter = new CsvWriter(fileWriter, CultureInfo.InvariantCulture);

            // Ask our csvWriter to write.
            csvWriter.WriteRecords(transactions);

            // Tell the file we are done.
            fileWriter.Close();

            Console.WriteLine("Goodbye!");
        }

        private static double Balance(List<Transaction> transactions, string accountType)
        {
            var cList = transactions.Where(line => line.Account == accountType).ToList();
            double aBalance = 0;
            foreach (var line in cList)
            {
                if (line.Action == "Deposit")
                    aBalance += line.Amount;
                else if (line.Action == "Withdraw")
                    aBalance -= line.Amount;
            }

            return aBalance;

            // Console.WriteLine($"Checking Account Balance: ${cBalance}");
            // Console.WriteLine($"Savings Account Balance: ${sBalance}");
        }

        private static Transaction Deposit()
        {
            Transaction deposit = new Transaction();
            deposit.Action = "Deposit";
            string answer = "";
            bool keepAsking = true;
            while (keepAsking)
            {
                answer = PromptForString("In which Account would you like to make a Deposit: (C)hecking or (S)avings?\n").ToLower();
                if (answer == "c" || answer == "checking")
                {
                    deposit.Account = "Checking";
                    keepAsking = false;
                }
                else if (answer == "s" || answer.Contains("saving"))
                {
                    deposit.Account = "Savings";
                    keepAsking = false;
                }
                else
                    Console.WriteLine("Sorry, I don't understand.");
            }

            deposit.Amount = PromptForDub($"How much are you Despositing in your {deposit.Account} Account?\n");

            Console.WriteLine($"You Deposited {deposit.Amount} in your {deposit.Account} Account on {deposit.When()}");

            return deposit;
        }

        private static Transaction Withdraw(List<Transaction> transactions)
        {
            Transaction withdraw = new Transaction();
            withdraw.Action = "Withdraw";
            string answer = "";
            bool keepAsking = true;
            while (keepAsking)
            {
                answer = PromptForString("Select an Account to Withdraw from: (C)hecking or (S)avings?\n").ToLower();
                if (answer == "c" || answer == "checking")
                {
                    withdraw.Account = "Checking";
                    Console.WriteLine($"Your {withdraw.Account} Account Balance is ... ");
                    keepAsking = false;
                }
                else if (answer == "s" || answer.Contains("saving"))
                {
                    withdraw.Account = "Savings";
                    Console.WriteLine($"Your {withdraw.Account} Account Balance is ... ");
                    keepAsking = false;
                }
                else
                    Console.WriteLine("Sorry, I don't understand.");
            }

            double aBalance = Balance(transactions, withdraw.Account);

            bool overDraft = true;
            while (overDraft)
            {
                withdraw.Amount = PromptForDub($"How much are you Withdrawing from your {withdraw.Account} Account?\n");
                if (withdraw.Amount > aBalance)
                {
                    Console.WriteLine($"You can only Withdraw up to {aBalance}");
                }
                else
                    overDraft = false;
            }

            Console.WriteLine($"You Withdrew {withdraw.Amount} from your {withdraw.Account} Account on {withdraw.When()}");

            return withdraw;
        }

        private static double PromptForDub(string prompt)
        {
            Console.Write(prompt);
            var userInput = Console.ReadLine();
            var numberInput = double.Parse(userInput);
            if (numberInput < 0)
            {
                Console.WriteLine("Only positive amounts allowed. Taking the absolute value.");
                numberInput = Math.Abs(numberInput);
            }
            return numberInput;
        }

        private static string PromptForString(string prompt)
        {
            Console.Write(prompt);
            var userInput = Console.ReadLine();
            return userInput;
        }
    }
}

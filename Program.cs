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
        class Account
        {
            // Properties. 
            public string Name { get; set; }
            public double Balance { get; set; }

            // Constructor. 
            public Account(string name, double balance)
            {
                Name = name;
                Balance = balance;
            }
        }

        // Transaction class supports both checking and savings ...
        // ... as well as deposits and withdraws.
        class Transaction
        {
            // Properties.

            public string Account { get; set; } // Checking or Savings. 
            public string Action { get; set; } // Deposit or Withdraw. 
            public double Amount { get; set; }
            public string WhenV { get; set; }
            public double Total { get; set; }

            // Methods.
            public string When()
            {
                WhenV = DateTime.Now.ToString();
                return WhenV;
            }

            public string Log()
            {
                string line = $"{Account}, {Amount}, {WhenV}, {Total}";
                return line;
            }
        }

        static void Main(string[] args)
        {
            // Creates a stream reader to get information from our file (if empty). 
            TextReader reader;
            // If the file exists
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

            // Creates a stream reader to get information from our file.
            var fileReader = new StreamReader("transactions.csv");

            // Create a CSV reader to parse the stream into CSV format.
            var csvReader = new CsvReader(fileReader, CultureInfo.InvariantCulture);

            // Get the records from the CSV reader, as `string` and finally as a `List`.
            var transactions = csvReader.GetRecords<int>().ToList();
            var transactions2 = new List<string>();

            // Tell the CSV reader not to interpret the first row as a header, otherwise the first number will be skipped.
            // csvReader.Configuration.HasHeaderRecord = false;
            // ^ error saying it is read-only.

            // Close the reader.
            fileReader.Close();

            // Bool to keep running program. 
            var keepRunning = true;

            var checkingAct = new Account("Checking", 0);
            var savingsAct = new Account("Savings", 0);

            while (keepRunning)
            {
                Console.WriteLine("What would you like to do?");
                Console.WriteLine("(D)eposit, (W)ithdraw, (B)alance, (Q)uit");
                var userChoice = Console.ReadLine().ToLower();

                switch (userChoice)
                {
                    case "d" or "deposit":
                        Deposit(checkingAct, savingsAct);
                        break;
                    case "w" or "withdraw":
                        Withdraw(checkingAct, savingsAct);
                        break;
                    case "b" or "balance":
                        Balance(checkingAct, savingsAct);
                        break;
                    case "q" or "quit":
                        keepRunning = false;
                        break;
                    case "s":
                        Console.WriteLine(transactions2);
                        break;
                }
            }

            // Create a stream for writing information into a file.
            var fileWriter = new StreamWriter("transactions.csv");

            // Create an object that can write CSV to the fileWriter.
            var csvWriter = new CsvWriter(fileWriter, CultureInfo.InvariantCulture);

            // Ask our csvWriter to write out our list of numbers.
            csvWriter.WriteRecords(transactions);

            // Tell the file we are done.
            fileWriter.Close();

            Console.WriteLine("Goodbye!");
        }

        private static void Balance(Account checkingAct, Account savingsAct)
        {
            Console.WriteLine($"Checking Account Balance: {checkingAct.Balance}");
            Console.WriteLine($"Savings Account Balance: {savingsAct.Balance}");
        }

        private static void Withdraw(Account checkingAct, Account savingsAct)
        {
            Transaction withdraw = new Transaction();
            string answer = "";
            bool keepAsking = true;
            while (keepAsking)
            {
                answer = PromptForString("From which Account would you like to make a Withdrawal: (C)hecking or (S)avings?\n").ToLower();
                if (answer == "c" || answer == "checking")
                {
                    withdraw.Account = "Checking";
                    Console.WriteLine($"Your Checking Account Balance is {checkingAct.Balance}");
                    keepAsking = false;
                }
                else if (answer == "s" || answer.Contains("saving"))
                {
                    withdraw.Account = "Savings";
                    Console.WriteLine($"Your Checking Account Balance is {savingsAct.Balance}");
                    keepAsking = false;
                }
                else
                    Console.WriteLine("Sorry, I don't understand.");
            }

            var overDraft = true;
            while (overDraft)
            {
                withdraw.Amount = PromptForDub($"How much are you Withdrawing from your {withdraw.Account.ToString()} account?\n");
                if (withdraw.Account == "Checking")
                {
                    if (checkingAct.Balance - withdraw.Amount < 0)
                    {
                        Console.WriteLine($"You can only withdraw up to {checkingAct.Balance}");
                    }
                    else
                    {
                        checkingAct.Balance -= withdraw.Amount;
                        withdraw.Total = checkingAct.Balance;
                        overDraft = false;
                    }
                }
                else if (withdraw.Account == "Savings")
                {
                    if (savingsAct.Balance - withdraw.Amount < 0)
                    {
                        Console.WriteLine($"You can only withdraw up to {savingsAct.Balance}");
                    }
                    else
                    {
                        savingsAct.Balance -= withdraw.Amount;
                        withdraw.Total = savingsAct.Balance;
                        overDraft = false;
                    }
                }
            }

            Console.WriteLine($"You Withdrew {withdraw.Amount} in your {withdraw.Account} Account on {withdraw.When()}");
            Console.WriteLine(withdraw.Log());
        }

        private static void Deposit(Account checkingAct, Account savingsAct)
        {
            Transaction deposit = new Transaction();
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

            deposit.Amount = PromptForDub($"How much are you Despositing in your {deposit.Account.ToString()} account?\n");

            if (deposit.Account == "Checking")
            {
                checkingAct.Balance += deposit.Amount;
                deposit.Total = checkingAct.Balance;
            }
            else if (deposit.Account == "Savings")
            {
                savingsAct.Balance += deposit.Amount;
                deposit.Total = savingsAct.Balance;
            }

            Console.WriteLine($"You Deposited {deposit.Amount} in your {deposit.Account} Account on {deposit.When()}");
            Console.WriteLine(deposit.Log());
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

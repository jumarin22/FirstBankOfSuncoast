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
            public DateTime TimeStamp { get; set; } = DateTime.Now;

            // Methods.
            public string When()
            {
                return TimeStamp.ToString("yyyy/MM/dd @ HH:mm:ss");
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

            // Get the records from the CSV reader.
            var transactions = csvReader.GetRecords<Transaction>().ToList();

            // Close the reader.
            reader.Close();

            // Bool to keep running program. 
            var keepRunning = true;

            // Main interface.
            while (keepRunning)
            {
                Console.WriteLine("What would you like to do?");
                Console.WriteLine("(D)eposit \n(W)ithdraw \n(B)alance  \n(H)istory  \n(T)ransfer  \n(Q)uit");
                var userChoice = Console.ReadLine().ToLower();

                switch (userChoice)
                {
                    case "d" or "deposit":
                        Deposit(transactions);
                        break;
                    case "w" or "withdraw":
                        Withdraw(transactions);
                        break;
                    case "b" or "balance":
                        Console.WriteLine($"Checking Account Balance: ${Balance(transactions, "Checking")}");
                        Console.WriteLine($"$Savings Account Balance: ${Balance(transactions, "Savings")}");
                        break;
                    case "h" or "history":
                        History(transactions);
                        break;
                    case "t" or "transfer":
                        Transfer(transactions);
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

        // Method to check Balance of an Account.
        private static double Balance(List<Transaction> transactions, string accountType)
        {
            // Pull objects from List filtered by Transaction.Account result.
            var cList = transactions.Where(line => line.Account == accountType).ToList();
            double aBalance = 0; // Create temporary balance sum. 
            foreach (var line in cList)
            {
                if (line.Action == "Deposit")
                    aBalance += line.Amount;
                else if (line.Action == "Withdraw")
                    aBalance -= line.Amount;
            }

            return aBalance;
        }

        // Method to Deposit.
        private static void Deposit(List<Transaction> transactions)
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

            Console.WriteLine($"You Deposited ${deposit.Amount} in your {deposit.Account} Account on {deposit.When()}");

            transactions.Add(deposit);
        }

        // Method to Withdraw.
        private static void Withdraw(List<Transaction> transactions)
        {
            Transaction withdraw = new Transaction();
            withdraw.Action = "Withdraw";
            double aBalance = 0; // Create temporary balance amount. 
            string answer = "";
            bool keepAsking = true;
            while (keepAsking)
            {
                answer = PromptForString("Select an Account to Withdraw from: (C)hecking or (S)avings?\n").ToLower();
                if (answer == "c" || answer == "checking")
                {
                    withdraw.Account = "Checking";
                    keepAsking = false;
                }
                else if (answer == "s" || answer.Contains("saving"))
                {
                    withdraw.Account = "Savings";
                    keepAsking = false;
                }
                else
                    Console.WriteLine("Sorry, I don't understand.");
            }

            aBalance = Balance(transactions, withdraw.Account); // Run Balance check to avoid overDraft.
            Console.WriteLine($"Your {withdraw.Account} Account Balance is ${aBalance}");
            if (aBalance == 0)
            {
                Console.WriteLine("You cannot Withdraw from an Account with 0 dollars!");
                return;
            }

            bool overDraft = true;
            while (overDraft)
            {
                withdraw.Amount = PromptForDub($"How much are you Withdrawing from your {withdraw.Account} Account?\n");
                if (withdraw.Amount > aBalance)
                {
                    Console.WriteLine($"You can only Withdraw up to ${aBalance}");
                }
                else
                    overDraft = false;
            }

            Console.WriteLine($"You Withdrew {withdraw.Amount} from your {withdraw.Account} Account on {withdraw.When()}");

            transactions.Add(withdraw);
        }

        // Method to see History of Transactions for an Account.
        private static void History(List<Transaction> transactions)
        {
            var answer = "";
            bool keepAsking = true;

            while (keepAsking)
            {
                Console.WriteLine("For which Account would you like to view the History?");
                answer = PromptForString("(C)hecking, (S)avings \n").ToLower();
                if (answer == "c" || answer == "checking")
                {
                    foreach (var line in transactions)
                    {
                        if (line.Account == "Checking")
                            Console.WriteLine($"{line.Account}, {line.Action}, {line.Amount}, {line.TimeStamp}");
                    }
                    keepAsking = false;
                }

                else if (answer == "s" || answer.Contains("saving"))
                {
                    foreach (var line in transactions)
                    {
                        if (line.Account == "Savings")
                            Console.WriteLine($"{line.Account}, {line.Action}, {line.Amount}, {line.TimeStamp}");
                    }
                    keepAsking = false;
                }

                else
                    Console.WriteLine("Sorry, I don't understand.");
            }
        }

        // Adventure Mode: Add the ability to transfer funds from my checking to my saving (and vice versa).
        // Creates 2 transactions, one Withdraw(account) then one Deposit(otherAccount). 
        // Repeats most code from Withdraw(), then Deposits() same amount in other Account. Suffers from wet code. 
        private static void Transfer(List<Transaction> transactions)
        {
            Transaction transferW = new Transaction();
            Transaction transferD = new Transaction();
            transferW.Action = "Withdraw";
            transferD.Action = "Deposit";
            double aBalance = 0; // Create temporary balance amount. 
            string answer = "";
            bool keepAsking = true;
            while (keepAsking)
            {
                answer = PromptForString("Select an Account to Transfer from: (C)hecking or (S)avings?\n").ToLower();
                if (answer == "c" || answer == "checking")
                {
                    transferW.Account = "Checking";
                    transferD.Account = "Savings";
                    keepAsking = false;
                }
                else if (answer == "s" || answer.Contains("saving"))
                {
                    transferW.Account = "Savings";
                    transferD.Account = "Checking";
                    keepAsking = false;
                }
                else
                    Console.WriteLine("Sorry, I don't understand.");
            }

            aBalance = Balance(transactions, transferW.Account); // Run Balance check to avoid overDraft.
            Console.WriteLine($"Your {transferW.Account} Account Balance is ${aBalance}");
            if (aBalance == 0)
            {
                Console.WriteLine("You cannot Transfer from an Account with 0 dollars!");
                return;
            }

            bool overDraft = true;
            while (overDraft)
            {
                transferW.Amount = PromptForDub($"How much are you Transferring from your {transferW.Account} Account to your {transferD.Account}?\n");
                if (transferW.Amount > aBalance)
                {
                    Console.WriteLine($"You can only Transfer up to ${aBalance}");
                }
                else
                    overDraft = false;
            }

            transactions.Add(transferW);
            transferD.Amount = transferW.Amount;
            transactions.Add(transferD);

            Console.WriteLine($"You transferred ${transferW.Amount} from your {transferW.Account} Account to your {transferD.Account} Account on {transferW.When()}");
        }

        // Method when asking for numbers. 
        private static double PromptForDub(string prompt)
        {
            Console.Write(prompt);
            var userInput = Console.ReadLine();
            var numberInput = double.Parse(userInput);
            if (numberInput < 0)
            {
                Console.WriteLine("Only positive amounts allowed. Taking the absolute value."); // Do not allow negative numbers. 
                numberInput = Math.Abs(numberInput);
            }
            return numberInput;
        }

        // Method when asking for strings. 
        private static string PromptForString(string prompt)
        {
            Console.Write(prompt);
            var userInput = Console.ReadLine();
            return userInput;
        }
    }
}

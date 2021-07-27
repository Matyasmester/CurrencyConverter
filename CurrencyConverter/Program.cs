using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.IO;
using static CurrencyConverter.CommandError;

namespace CurrencyConverter
{
    class Program
    {
        static string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "defaults.txt");
        const string link = "https://cdn.jsdelivr.net/gh/fawazahmed0/currency-api@1/latest/currencies/currFrom/currTo.json";

        static string[] defaultValues = GetDefaultValues();

        static string currencyFrom = defaultValues[0];
        static string currencyTo = defaultValues[1];

        private static void WriteColoredString(ConsoleColor color, string msg)
        {
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }

        private static bool IsValidCurrencyPair(string first, string second)
        {
            return first.Length == 3 && second.Length == 3;
        }

        private static string[] GetDefaultValues()
        {
            if (!File.Exists(defaultPath)) return new[] { "eur", "huf" };
            return File.ReadAllLines(defaultPath);
        }

        private static CommandError ExecuteCommand(string[] args)
        {
            string first = string.Empty; 
            string second = string.Empty;
            string command = args[0];

            if(command == "cc" || command == "cd")
            {
                first = args[1];
                second = args[2];
            }

            switch (command)
            {
                // Change Currency
                case "cc":
                    if (args.Length != 3) return SyntaxError;
                    if (!IsValidCurrencyPair(first, second)) return InvalidCurrency;
                    currencyFrom = first;
                    currencyTo = second;
                    return OK;
                // Swap currencyTo and currencyFrom | no arguments
                case "swap":
                    string temp = currencyFrom;
                    currencyFrom = currencyTo;
                    currencyTo = temp;
                    return OK;
                // Change Defaults
                case "cd":
                    if (args.Length != 3) return SyntaxError;
                    if (!IsValidCurrencyPair(first, second)) return InvalidCurrency;
                    File.WriteAllLines(defaultPath, new[] { first, second });
                    return OK;
                case "?":
                case "help":
                    WriteColoredString(ConsoleColor.Green, "cc = Change Currency | usage: cc [currFrom] [currTo] | e.g: cc btc eur\n" +
                                                           "swap | Swaps the two currencies\n" +
                                                           "cd = Change Defaults | usage: cd [currency] [currency] | e.g: cd usd eur\n" +
                                                           "General: Type a number and it will convert it to the other currency.\n");
                    return OK;
                default:
                    return UnknownCommand;
            }
        }

        static void Main()
        {
            WriteColoredString(ConsoleColor.Green, "Type 'help' or '?' for a list of commands and their descriptions.\n\n");

            while (true)
            {
                WriteColoredString(ConsoleColor.Yellow, $"[{currencyFrom} --> {currencyTo}] : ");
                string[] args = Console.ReadLine().TrimEnd(' ').Split(' ');

                double input;
                bool isNum = double.TryParse(args[0], out input);
                if (!isNum)
                {
                    CommandError commandResult = ExecuteCommand(args);
                    switch (commandResult)
                    {
                        case SyntaxError:
                            WriteColoredString(ConsoleColor.Red, "Syntax error in command. \n");
                            continue;
                        case InvalidCurrency:
                            WriteColoredString(ConsoleColor.Red, "Invalid currency / currencies. \n");
                            continue;
                        case UnknownCommand:
                            WriteColoredString(ConsoleColor.Red, "Unknown command. \n");
                            continue;
                        default:
                            continue;
                    }
                }
                
                string finalURL = link.Replace("currFrom", currencyFrom).Replace("currTo", currencyTo);

                string json = string.Empty;

                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        json = wc.DownloadString(finalURL);
                    }
                    catch(WebException e)
                    {
                        var response = (HttpWebResponse)e.Response;
                        WriteColoredString(ConsoleColor.Red, $"Web Service returned {response.StatusCode}, probably non-existent currency names. \n");
                        continue;
                    }
                }
                JsonDocument document = JsonDocument.Parse(json);

                JsonElement root = document.RootElement;

                JsonElement valElement = root.GetProperty(currencyTo);
                double converted = valElement.GetDouble() * input;

                WriteColoredString(ConsoleColor.Cyan, $"{input} {currencyFrom} = {converted} {currencyTo} \n");

                document.Dispose();
            }
        }
    }
}

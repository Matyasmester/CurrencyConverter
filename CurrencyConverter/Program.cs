using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using static CurrencyConverter.CommandError;

namespace CurrencyConverter
{
    class Program
    {
        static string currencyFrom = "eur";
        static string currencyTo = "huf";

        const string link = "https://cdn.jsdelivr.net/gh/fawazahmed0/currency-api@1/latest/currencies/currFrom/currTo.json";
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

        private static CommandError ExecuteCommand(string[] args)
        {
            string command = args[0];
            switch (command)
            {
                // Change Currency
                case "cc":
                    if (args.Length != 3) return SyntaxError;
                    string first = args[1];
                    string second = args[2];
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
                case "?":
                case "help":
                    WriteColoredString(ConsoleColor.Green, "cc = Change Currency | usage: cc [currFrom] [currTo] | e.g: cc btc eur\n" +
                                                           "swap | Swaps the two currencies\n" +
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

                double input = 0.0;
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

                using (WebClient wc = new WebClient())
                {
                    string json = string.Empty;
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    public enum CommandError
    {
        OK,
        UnknownCommand,
        SyntaxError,
        InvalidCurrency
    }
}

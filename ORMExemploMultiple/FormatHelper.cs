using System;
using System.Text;

namespace ORMExemploMultiple
{
    internal class FormatHelper
    {
        internal static string WrapInBrackets(string mappedName)
        {
            StringBuilder sb = new StringBuilder('`');
            sb.Append(mappedName);
            sb.Append('`');
            return sb.ToString();
        }
    }
}
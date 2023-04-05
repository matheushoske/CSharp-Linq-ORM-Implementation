using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    internal class DadosBDMysql : IDadosBD
    {
        public string ConverterParaDataType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "TINYINT(1)";
                case TypeCode.Byte:
                    return "TINYINT UNSIGNED";
                case TypeCode.SByte:
                    return "TINYINT";
                case TypeCode.Int16:
                    return "SMALLINT";
                case TypeCode.UInt16:
                    return "SMALLINT UNSIGNED";
                case TypeCode.Int32:
                    return "INT";
                case TypeCode.UInt32:
                    return "INT UNSIGNED";
                case TypeCode.Int64:
                    return "BIGINT";
                case TypeCode.UInt64:
                    return "BIGINT UNSIGNED";
                case TypeCode.Single:
                    return "FLOAT";
                case TypeCode.Double:
                    return "DOUBLE";
                case TypeCode.Decimal:
                    return "DECIMAL";
                case TypeCode.Char:
                    return "CHAR";
                case TypeCode.String:
                    return "VARCHAR";
                case TypeCode.DateTime:
                    return "DATETIME";
                default:
                    if (type == typeof(Guid))
                    {
                        return "CHAR(36)";
                    }
                    else if (type == typeof(byte[]))
                    {
                        return "BLOB";
                    }
                    else if (type == typeof(TimeSpan))
                    {
                        return "TIME";
                    }
                    else
                    {
                        throw new NotSupportedException("Tipo " + type.FullName + " não é suportado.");
                    }
            }
        }
    }
}

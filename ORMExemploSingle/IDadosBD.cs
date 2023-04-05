using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    internal interface IDadosBD
    {
        string ConverterParaDataType(Type type);
    }
}

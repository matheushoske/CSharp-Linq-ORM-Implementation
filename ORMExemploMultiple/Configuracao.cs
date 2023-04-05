using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    internal class Configuracao
    {
        internal DbConnection Connection { get; set; }
        internal IDadosBD DadosBD { get; set; }
        internal BDProvider DbProvider { get; set; }
        internal BDConnectionStyle ConnectionStyle { get; set; } = BDConnectionStyle.Direct;
        
    }
}

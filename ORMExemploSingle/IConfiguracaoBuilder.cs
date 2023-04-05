using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    public interface IConfiguracaoBuilder
    {
        void UseConnection(DbConnection connection, BDProvider provider, BDConnectionStyle connectionStyle = BDConnectionStyle.Direct);
    }
}

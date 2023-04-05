using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle.Factories
{
    internal class DadosBDFactory
    {
        public static IDadosBD Create(BDProvider provider) 
        {
            switch (provider)
            {
                case BDProvider.MySQL:
                    return new DadosBDMysql();
                case BDProvider.SQLServer:
                    throw new NotImplementedException();
                case BDProvider.Oracle:
                    throw new NotImplementedException();
                case BDProvider.Postgres:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

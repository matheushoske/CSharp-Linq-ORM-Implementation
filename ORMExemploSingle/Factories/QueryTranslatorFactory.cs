using ORMExemploSingle.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    internal class QueryTranslatorFactory
    {
        public static IQueryTranslator Create(MapeadorBD mapeador) 
        {
            switch (mapeador.Configuracao.DbProvider)
            {
                case BDProvider.MySQL:
                    return new MysqlQueryTranslator(mapeador.Model);
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

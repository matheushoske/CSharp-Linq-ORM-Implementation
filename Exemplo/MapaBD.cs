using GlobalDatabaseConnector.Database;
using ORMExemploSingle;
//using ORMExemploMultiple;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemplo
{
    internal class MapaBD : MapeadorBD
    {
        public Mapear<Produto> Produtos { get; set; }
        protected override void Configurar(IConfiguracaoBuilder configuracao)
        {
            var cn = DbFactory.Database(DataBase.MySQL)
                                 .CreateConnector("localhost", "hoske", "123", "testes", 3306)
                                 .Connect();
            configuracao.UseConnection(cn as DbConnection,BDProvider.MySQL);
        }
    }
}

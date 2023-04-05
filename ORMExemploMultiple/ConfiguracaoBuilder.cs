using ORMExemploMultiple.Factories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    public class ConfiguracaoBuilder : IConfiguracaoBuilder
    {
        private readonly Configuracao _configuracao;
        public ConfiguracaoBuilder()
        {
            _configuracao = new Configuracao();
        }
        public void UseConnection(DbConnection connection,BDProvider provider, BDConnectionStyle connectionStyle = BDConnectionStyle.Direct)
        {
            _configuracao.Connection = connection;
            _configuracao.DadosBD = DadosBDFactory.Create(provider);
            _configuracao.ConnectionStyle = connectionStyle;
            _configuracao.DbProvider = provider;
        }
        internal Configuracao BuildConfiguracao() 
        {
            return _configuracao;
        }
    }
}

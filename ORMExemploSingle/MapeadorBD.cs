using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ORMExemploSingle
{
    public abstract class MapeadorBD : IDisposable
    {
        internal readonly Configuracao Configuracao;
        private readonly MetaModelBD _metaModel;
        private readonly ProviderBD _provider;
        public MetaModelBD Model => _metaModel;

        private Dictionary<MetaTableBD, IMapearTabela> _tabelas => MetaModelBD.Tabelas;
        public MapeadorBD()
        {
            var configuracaoBuilder = new ConfiguracaoBuilder();
            Configurar(configuracaoBuilder);
            Configuracao = configuracaoBuilder.BuildConfiguracao();
            _metaModel = new MetaModelBD(this);
            _provider = new ProviderBD(this);

        }
        protected abstract void Configurar(IConfiguracaoBuilder configuracao);
        public void Dispose()
        {
            _disposed = true;
        }
        private bool _disposed = false;
        private void CheckDispose() 
        {
            if(_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
        internal IMapearTabela GetTable(MetaTableBD metaTable)
        {
            IMapearTabela table;
            if (!_tabelas.TryGetValue(metaTable, out table))
            {
                table = (IMapearTabela)Activator.CreateInstance(
                  typeof(Mapear<>).MakeGenericType(
                    new Type[] { metaTable.EntityType }),
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                    new object[] { this, metaTable }, null);
                _tabelas.Add(metaTable, table);
            }
            return table;
        }
    }
}

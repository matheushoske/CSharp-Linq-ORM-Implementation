using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ORMExemploMultiple
{
    public abstract class MapeadorBD : IDisposable
    {
        internal readonly Configuracao Configuracao;
        private readonly MetaModelBD _metaModel;
        private readonly ProviderBD _provider;
        private readonly ChangeTracker _changeTracker;
        public MetaModelBD Model => _metaModel;

        public ChangeTracker ChangeTracker => _changeTracker;

        private Dictionary<MetaTableBD, IMapearTabela> _tabelas => MetaModelBD.Tabelas;
        public MapeadorBD()
        {
            var configuracaoBuilder = new ConfiguracaoBuilder();
            Configurar(configuracaoBuilder);
            Configuracao = configuracaoBuilder.BuildConfiguracao();
            _metaModel = new MetaModelBD(this);
            _provider = new ProviderBD(this);
            _changeTracker = new ChangeTracker();

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
        public Mapear<TEntity> GetTable<TEntity>() where TEntity : class
        {
            CheckDispose();
            MetaTableBD metaTable = _metaModel.GetTable(typeof(TEntity));
            if (metaTable == null)
                throw new Exception(
                  string.Format("{0} is not decorated with the TableAttribute.",
                  typeof(TEntity).Name));
            IMapearTabela table = GetTable(metaTable);
            if (table.ElementType != typeof(TEntity))
                throw new Exception(
                  string.Format("It was not possible to find a table for type {0}",
                  typeof(TEntity).Name));
            return (Mapear<TEntity>)table;
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
        public void SubmitChanges()
        {
            this.CheckDispose();
            // Get an open database connection
            using (DbConnection connection = Configuracao.Connection)
                try
                {
                    // Create or use a transaction
                    using (TransactionScope ts = new TransactionScope())
                    {
                        // Enlist in this transaction
                        connection.EnlistTransaction(Transaction.Current);
                        // Process all changes and apply them to the database
                        ChangeProcessor processor =
                          new ChangeProcessor(_changeTracker, this);
                        processor.SubmitChanges();
                        // Vote to commit the transaction
                        ts.Complete();
                        // If all changes were committed successfuly
                        // to the DB then accept all the changes
                        _changeTracker.AcceptChanges();
                    }
                }
                finally
                {
                }
        }
    }
}

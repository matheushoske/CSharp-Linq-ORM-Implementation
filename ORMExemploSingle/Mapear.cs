using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ORMExemploSingle
{
    public sealed class Mapear<TModel> : IQueryable<TModel>, IEnumerable<TModel>,IOrderedQueryable<TModel>,  IMapearTabela where TModel : class
    {
        private readonly ProviderBD provider;
        private readonly MetaTableBD _metaTable;
        public Mapear(MapeadorBD mapeador, MetaTableBD metaTable)
        {
            provider = new ProviderBD(mapeador);
            _metaTable = metaTable;
        }
        
        public Expression Expression => Expression.Constant(this);

        public Type ElementType => typeof(TModel);

        public IQueryProvider Provider => this;

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<TModel>(expression);
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var result = (IEnumerable)provider.Execute(expression);
            IQueryable simpleQuery = result.AsQueryable();
            IQueryable<TElement> query = simpleQuery as IQueryable<TElement>;
            return query;
        }

        public object Execute(Expression expression)
        {
            return this.Execute<TModel>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)provider.Execute(expression);
        }


        public IEnumerator<TModel> GetEnumerator()
        {
            return ((IEnumerable<TModel>)provider.
      Execute(Expression.Constant(this))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

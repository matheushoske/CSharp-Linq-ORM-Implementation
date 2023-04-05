using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ORMExemploMultiple
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

        //single example
        //public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        //{
        //    var result = (IEnumerable)provider.Execute(expression);
        //    IQueryable simpleQuery = result.AsQueryable();
        //    IQueryable<TElement> query = simpleQuery as IQueryable<TElement>;
        //    return query;
        //}
        private List<Expression> _expressions = new List<Expression>();
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            _expressions.Add(expression);
            return this as IQueryable<TElement>;
        }

        public object Execute(Expression expression)
        {
            return provider.Execute(_expressions.ToArray());//alterado
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }


        public IEnumerator<TModel> GetEnumerator()
        {
            return ((IEnumerable<TModel>)provider.
      Execute(_expressions.ToArray())).GetEnumerator();//alterado
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

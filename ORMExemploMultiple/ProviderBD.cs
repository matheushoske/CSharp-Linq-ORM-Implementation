using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Linq;
using System.Reflection;
using ORMExemploMultiple.Interfaces;

namespace ORMExemploMultiple
{
    internal class ProviderBD : IProviderBD
    {
        private readonly MapeadorBD _mapeador;
        public ProviderBD(MapeadorBD mapeador) 
        {
            _mapeador = mapeador;
        }
       
        private bool _disposed = false;
        private void CheckDispose() 
        {
            if(_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
        public void Dispose()
        {
            _disposed = true;
        }

        public string GetQueryText()
        {
            throw new NotImplementedException();
        }

        internal object Execute(Expression[] expressions)//alterado
        {
            CheckDispose();
            IQueryTranslator translator = QueryTranslatorFactory.Create(_mapeador);
            QueryInfo info = translator.Translate(expressions);
            return Execute(info);
       
        }
        private object Execute(QueryInfo info)
        {
            // Get an open connection to the database
            DbConnection connection = _mapeador.Configuracao.Connection;
            try
            {
                // Build a SQL Command
                DbCommand command = connection.CreateCommand();
                command.CommandText = info.QueryText;
                AddParameters(info.QueryParameters, ref command);
                // Attempt to excute the query if no result was expected from the query
                if (info.ResultShape == ResultShape.None)
                    command.ExecuteNonQuery();
                else
                {
                    DbDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult);

                    //...

      // What is the CLR type of the returned result-set rows?
      Type resultEntityType =
        (info.LambdaExpression == null) ? info.SourceMetadata.EntityType : info.LambdaExpression.Body.Type;
                    // Build a pipeline so that we can       // read and map the results returned from the DB.
                    // Do this by dynamically creating an instance       // of DatabaseResultMapper<> class.
                    // The followig use of reflection is equivalent to:
                    //    IEnumerable mappedResults =       //         new DatabaseResultMapper<T>(      //           info, reader, _dataContext.ChangeTracker);
                    IEnumerable mappedResults = (IEnumerable)Activator.CreateInstance(typeof(ResultMapperBD<>)
                    .MakeGenericType(new Type[] { resultEntityType }),
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { info, reader, _mapeador.ChangeTracker }, null);
                    // Are we expecting a single entity or a sequence of entities?
                    if (info.ResultShape == ResultShape.Sequence)
                    {
                        //IEnumerator enumerator = mappedResults.GetEnumerator();
                        // Read the results by enumerating through all mappedResults
                        // This is not perfect but as I am sharing the database
                        // connection between all queries and even write operations,
                        // I need to load all entities into memory by fully reading
                        // them using the DbDataReader (mappedResults) – that is exactly         // what the constructor of List<> does here.
                        // The followig use of reflection is equivalent to:
                        //    return new List<T>(mappedResults);
                        return Activator.CreateInstance(typeof(List<>).MakeGenericType(
                          new Type[] { resultEntityType }), new object[] { mappedResults });
                    }
                    else if (info.ResultShape == ResultShape.Singleton)
                    {
                        IEnumerator enumerator = mappedResults.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            return enumerator.Current;
                        }
                        // If First or Single query operators are used        // throw an excpetion because no rows where returned.
                        // You should always expect at least one element
                        // when First or Single are used
                        else if (!info.UseDefault)
                        {
                            throw new Exception("No entity was found meeting the specified criteria.");
                        }
                    }
                }
                return null;
            }
            finally
            {
                //connection.Close();
            }
        }

        private void AddParameters(List<KeyValuePair<string, object>> parameters, ref DbCommand command) 
        {
            foreach (var item in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = item.Key;
                parameter.Value = item.Value;
                command.Parameters.Add(parameter);
            }
        }

    }
}

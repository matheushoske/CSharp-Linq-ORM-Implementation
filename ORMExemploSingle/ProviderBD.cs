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
using ORMExemploSingle.Interfaces;

namespace ORMExemploSingle
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

        internal object Execute(Expression query)
        {
            CheckDispose();
            IQueryTranslator translator = QueryTranslatorFactory.Create(_mapeador);
            QueryInfo info = translator.Translate(query);
            return Execute(info);
       
        }
        private object Execute(QueryInfo info)
        {
            // Aqui retornamos a nossa Connection construida na configuração
            // nesse caso temos uma conexão única e estática, o que não é muito interessante
            // e foi utilizado apenas para o exemplo.
            // Para cenários reais o ideal seria retornar uma nova conexão a partir de um método
            // abrindo e "disposing" a conexão para que não tenho risco de readers em execução
            // simultânea dentro da mesma conexão em diferentes Threads ou Tasks
            // exemplo de como realmente deverá ser implementado:
            //  using(var connection = BDHelper.NovaConexaoAberta())
            DbConnection connection = _mapeador.Configuracao.Connection;
            try
            {
                // Contrundo o DbCommand
                DbCommand command = connection.CreateCommand();
                command.CommandText = info.QueryText;
                //Aqui fazemos a inserção dos parâmetros da query para dentro do nosso command
                AddParameters(info.QueryParameters, ref command);
                // Tenta executar a query caso não esteja aguardando nenhum resultado
                if (info.ResultShape == ResultShape.None)
                    command.ExecuteNonQuery();
                else
                {
                    DbDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult);

                    // Encontra o tipo do objeto (Entidade) de retorno
                    Type resultEntityType =
                    (info.LambdaExpression == null) ? info.SourceMetadata.EntityType : info.LambdaExpression.Body.Type;
                    // Construa um pipeline para que possamos ler e mapear os resultados retornados do banco de dados
                    // Faça isso criando dinamicamente uma instância da classe ResultMapperBD<>.
                    // O uso do reflection abaixo é o equiavalente a:
                    //    IEnumerable mappedResults = new ResultMapperBD<T>(info, reader);
                    IEnumerable mappedResults = (IEnumerable)Activator.CreateInstance(typeof(ResultMapperBD<>)
                    .MakeGenericType(new Type[] { resultEntityType }),
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { info, reader }, null);
                    // Valida se esperando uma única entidade ou uma sequência de entidades
                    if (info.ResultShape == ResultShape.Sequence)
                    {
                        //IEnumerator enumerator = mappedResults.GetEnumerator();
                        // Lê os resultados enumerando todos os mappedResults
                        // This is not perfect but as I am sharing the database
                        // connection between all queries and even write operations,
                        // I need to load all entities into memory by fully reading
                        // them using the DbDataReader (mappedResults) – that is exactly
                        // what the constructor of List<> does here.
                        // The followig use of reflection is equivalent to:
                        // Isso aqui não ficou perfeito, mas como estou compartilhando a connection
                        // entre todas as consultas e até operações de gravação,
                        // foi preciso carregar todas as entidades na memória lendo elas totalmente
                        // usando o reader (mappedResults) – isso é exatamente
                        // o que o construtor da Lista (List<>) está fazendo.
                        // O uso do reflection abaixo é o equiavalente a:
                        // return new List<T>(mappedResults);
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
                        // Se os operadores de consulta 'First' ou 'Single' forem usados
                        // lança uma exception porque nenhuma linha foi retornada.
                        // Devemos sempre esperar por pelo menos uma linha
                        // quando os operadores 'First' ou 'Single' forem usados
                        else if (!info.UseDefault)
                        {
                            throw new Exception("Nenhuma entidade foi encontrada atendendo aos critérios especificados.");
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

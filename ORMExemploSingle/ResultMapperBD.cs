using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    internal enum ResultShape
    {
        None,       // Queries que não se esperam ter valores de retorno (insert, update, delete, etc...)
        Singleton,  // Retorna uma entidade única
        Sequence    // Retorna uma sequência de entidades
    }
    
    //Mapear o Reader em objetos
    internal class ResultMapperBD<TEntity> : IEnumerable<TEntity>
    {
        private readonly QueryInfo _info;
        private readonly IDataReader _reader;
        public ResultMapperBD(QueryInfo info, IDataReader reader)
        {
            _info = info;
            _reader = reader;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            // compila a expressão lambda em uma função
            Delegate projectionFunction = null;
            if (_info.LambdaExpression != null)
            {
                projectionFunction = _info.LambdaExpression.Compile();
            }
            bool primeiro = true;
            MemberInfo[] members = null;
            BindingFlags bindingFlags = BindingFlags.NonPublic |
            BindingFlags.Public |
                                        BindingFlags.Instance;
            while (_reader.Read())
            {
                if (primeiro)
                {
                    // encontra a ordem das colunas retornadas pelo banco de dados
                    members = new MemberInfo[_reader.FieldCount];
                    var persistentDataMembers = _info.SourceMetadata.PersistentDataMembers;
                    for (int i = 0; i < _reader.FieldCount; i++)
                    {
                        string colName = _reader.GetName(i);
                        MetaDataMemberBD mem =
                          persistentDataMembers.FirstOrDefault(
                            p => string.Compare(p.MappedName, colName, true, CultureInfo.InvariantCulture) == 0);
                        if (mem == null)
                            throw new Exception(string.Format(
                              "Não foi possível encontrar uma coluna de mapeada para {0}",
                              colName));
                        members[i] = mem.StorageMember ?? mem.Member;
                    }
                    primeiro = false;
                }
                // cria uma instância única e vazia da Entidade (objeto mapeado)
                object entity = Activator.CreateInstance(
                  _info.SourceMetadata.EntityType, bindingFlags, null, null, null);
                // preenche a entidade com valores recebidos do reader
                for (int i = 0; i < members.Length; i++)
                {
                    // OBSERVAÇÃO: estou usando uma técnica de conversão muito simples aqui. // Você pode querer usar uma mais complexa...
                    Type memberType = TypeHelper.GetMemberType(members[i]);
                    //é um tipo anulável? se sim, faz a extração do tipo genérico //similar ao Nullable.GetUnderlyingType
                    if (TypeHelper.IsNullableType(memberType))
                    {
                        memberType = memberType.GetGenericArguments()[0];
                    }
                    object value = Convert.ChangeType(_reader.GetValue(i), memberType);
                    // define o valor do membro na instância da entidade como 'value'
                    TypeHelper.SetMemberValue(entity, members[i], value);
                }
                // projeta a entidade chamando o 'projectionFunction'
                if (projectionFunction == null)
                {
                    yield return (TEntity)entity;
                }
                else
                {
                    yield return (TEntity)projectionFunction.DynamicInvoke(entity);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

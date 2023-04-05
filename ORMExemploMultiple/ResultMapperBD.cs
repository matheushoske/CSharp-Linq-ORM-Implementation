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

namespace ORMExemploMultiple
{
    internal enum ResultShape
    {
        None,       // The query is not expected to have a return value
        Singleton,  // It returns a single entity
        Sequence    // It returns a sequence of entities
    }
    
    //Mapear o Reader em objetos
    internal class ResultMapperBD<TEntity> : IEnumerable<TEntity>
    {
        private readonly QueryInfo _info;
        private readonly IDataReader _reader;
        public ResultMapperBD(QueryInfo info, IDataReader reader, ChangeTracker changeTracker)
        {
            _info = info;
            _reader = reader;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            // compile the lambda expression into a function
            Delegate projectionFunction = null;
            if (_info.LambdaExpression != null)
            {
                projectionFunction = _info.LambdaExpression.Compile();
            }
            bool isFirst = true;
            MemberInfo[] members = null;
            BindingFlags bindingFlags = BindingFlags.NonPublic |
            BindingFlags.Public |
                                        BindingFlags.Instance;
            while (_reader.Read())
            {
                if (isFirst)
                {
                    // find the order of the columns returned by the database
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
                              "It was not possible to find a mapping column for {0}",
                              colName));
                        members[i] = mem.StorageMember ?? mem.Member;
                    }
                    isFirst = false;
                }
                // create a single instanace of the Entity (the mapper object)
                object entity = Activator.CreateInstance(
                  _info.SourceMetadata.EntityType, bindingFlags, null, null, null);
                // populate its members with values from the result-set
                for (int i = 0; i < members.Length; i++)
                {
                    // Do magic conversion from SQL type to CLR type!
                    // NOTE: I am using a very simplied conversion technique here.       // You may want to use a more complex one...
                    Type memberType = TypeHelper.GetMemberType(members[i]);
                    // is this a Nullable type? if yes, then get       // its generic type argument for conversion
                    if (TypeHelper.IsNullableType(memberType))
                    {
                        memberType = memberType.GetGenericArguments()[0];
                    }
                    object value = Convert.ChangeType(_reader.GetValue(i), memberType);
                    // set the value of the member on the entity instance to 'value'
                    TypeHelper.SetMemberValue(entity, members[i], value);
                }
                // project on the entity by calling the     // compiled projection funcation
                if (projectionFunction == null)
                {
                    //// this entity needs to be tracked
                    //// some tracking code to go here ...
                    //yield return (TEntity)trackedEntity;
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

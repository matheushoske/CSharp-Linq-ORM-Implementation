using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    public class MetaDataMemberBD
    {
        public bool CanBeNull { get; set; }
        public string DbType { get; set; }
        public Type DeclaringType { get; set; }

        public bool IsAssociation { get; set; }

        //É realmente uma coluna do banco de dados
        public bool IsPersistent { get; set; }
        public bool IsPrimaryKey { get; set; }

        //Nome da coluna no banco
        public string MappedName { get; set; }
        public string Name { get; set; }

        //The MemberInfo object referencing the actual property or field of the entity class. This is used to gain access to the value of this member.
        public MemberInfo Member { get; set; }

        //Similar to the Member property, with the difference that it represents the underlying member that should be used by the mapper to set the value of a cell read from the database
        //– should the setter of a property be called when the value of the column is read from the database or should the mapper find the underlying field and set its value directly?
        public MemberInfo StorageMember { get; set; }
        public Type Type { get; set; }
    }
}

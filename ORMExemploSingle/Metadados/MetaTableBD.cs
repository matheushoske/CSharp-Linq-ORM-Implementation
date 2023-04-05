using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    public class MetaTableBD
    {
        public MetaTableBD()
        {

        }
        public string TableName { get; internal set; }

        public List<MetaDataMemberBD> DataMembers { get; internal set; }

        public Type EntityType { get; internal set; }
        public MetaModelBD Model { get; internal set; }
        
        //Chaves primarias
        public List<MetaDataMemberBD> IdentityMembers { get; internal set; }

        //Todas as colunas mapeadas no banco
        public List<MetaDataMemberBD> PersistentDataMembers { get; internal set; }
        
    }
}

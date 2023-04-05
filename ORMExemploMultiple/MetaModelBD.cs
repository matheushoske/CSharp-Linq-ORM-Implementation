using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    public class MetaModelBD
    {
        private readonly static List<MetaTableBD> _tabelas = new List<MetaTableBD>();
        internal static Dictionary<MetaTableBD, IMapearTabela> Tabelas = new Dictionary<MetaTableBD, IMapearTabela>();
        private readonly MapeadorBD _mapeador;
        public MetaModelBD(MapeadorBD mapeador)
        {
            _mapeador = mapeador;
            CarregarMetadados(_mapeador);
        }
        private void CarregarMetadados(MapeadorBD mapeador)
        {
            var props = mapeador.GetType().GetProperties();
            props = props.Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Mapear<>)).ToArray();
            foreach (var prop in props)
            {
                Type type = prop.PropertyType.GetGenericArguments()[0];
                var tabela = new MetaTableBD();
                tabela.EntityType = type;
                tabela.Model = this;
                tabela.TableName = BuscarNomeTabelaBD(type);
                CarregarMembersMetadata(type, ref tabela);
                _tabelas.Add(tabela);
                var classeMapeada = mapeador.GetTable(tabela);
                prop.SetValue(mapeador, classeMapeada);
                
            }
        }
        string BuscarNomeTabelaBD(Type type) 
        {
            Object atributo = type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            if (atributo != null)
            {
                return ((TableAttribute)atributo).Name;
            }
            else
                return type.Name;
        }
        void CarregarMembersMetadata(Type type, ref MetaTableBD tabela) 
        {
            var colunas = new List<MetaDataMemberBD>();
            foreach (var prop in type.GetProperties())
            {
                var proptype = prop.PropertyType;
                var coluna = new MetaDataMemberBD();
                coluna.Name = prop.Name;
                coluna.MappedName = GetColumnName(prop);
                coluna.Type = proptype;
                coluna.CanBeNull = TypeHelper.IsNullableType(proptype);
                coluna.Member = prop;
                coluna.StorageMember = prop; //
                coluna.DeclaringType = proptype;
                coluna.IsPrimaryKey = ChecarPrimary(prop);
                coluna.IsPersistent = ChecarPersistencia(prop);
                if (coluna.CanBeNull) proptype = Nullable.GetUnderlyingType(proptype) ?? proptype;
                if (proptype.IsPrimitive || proptype == typeof(Decimal) || proptype == typeof(String) || proptype == typeof(DateTime))
                {
                    coluna.IsDbGenerated = false; //
                    coluna.DbType = _mapeador.Configuracao.DadosBD.ConverterParaDataType(proptype);
                    coluna.IsAssociation = false;
                }
                else
                {

                    coluna.IsPersistent = false;
                    coluna.IsDbGenerated = true; //
                    coluna.DbType = "";
                    coluna.IsAssociation = true;

                }
                colunas.Add(coluna);
            }
            tabela.PersistentDataMembers = colunas.Where(x => x.IsPersistent).ToList();
            tabela.IdentityMembers = colunas.Where(x => x.IsPrimaryKey).ToList();
            tabela.DataMembers = colunas;
        }

        bool ChecarPrimary(PropertyInfo prop)
        {
            Object atributo = prop.BuscarAtributo(typeof(KeyAttribute));
            return atributo == null;
        }
        bool ChecarPersistencia(PropertyInfo prop) 
        {
            Object atributo = prop.BuscarAtributo(typeof(NotMappedAttribute));
            return atributo == null;
        }
        internal string GetColumnName(MemberInfo member)
        {
            Object atributo = member.BuscarAtributo(typeof(ColumnAttribute));
            if(atributo != null) 
            {
                return ((ColumnAttribute)atributo).Name;
            }
            else
                return member.Name;
        }

        internal MetaTableBD GetTable(Type type)
        {
           var tabela = _tabelas.Where(x => x.EntityType == type).FirstOrDefault();
            if(tabela != null)
                return tabela;
            throw new Exception("Tabela não encontrada nos metadados");
        }
        
    }
}

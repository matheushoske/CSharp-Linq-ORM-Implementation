using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    internal static class AttributeExtension
    {
        public static object BuscarAtributo(this MemberInfo member, Type attribute) 
        {
            return member.GetCustomAttributes(attribute, false).FirstOrDefault();
        }
        public static object[] BuscarAtributos(this MemberInfo member, Type attribute)
        {
            return member.GetCustomAttributes(attribute, false);
        }
    }
}

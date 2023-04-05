using System;
using System.Reflection;

namespace ORMExemploSingle
{
    internal class TypeHelper
    {
        internal static Type GetMemberType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        internal static object GetMemberValue(object entity, MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(entity);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(entity);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static bool IsNullableType(Type memberType)
        {
           return Nullable.GetUnderlyingType(memberType) != null;
        }

        internal static void SetMemberValue(object entity, MemberInfo memberInfo, object value)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                     ((FieldInfo)memberInfo).SetValue(entity,value);
                    break;
                case MemberTypes.Property:
                     ((PropertyInfo)memberInfo).SetValue(entity, value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
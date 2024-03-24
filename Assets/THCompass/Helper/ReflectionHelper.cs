using PugMod;
using System;
using System.Linq;

namespace Assets.THCompass.Helper
{
    public static class ReflectionHelper
    {
        public static MemberInfo Reflection(this object obj, string targetName)
        {
            return obj.GetType().GetMembersChecked().FirstOrDefault(info => info.GetNameChecked().Equals(targetName))
                ?? throw new Exception("Not found" + targetName);
        }
        public static T GetField<T>(this object obj, string fieldName)
        {
            return (T)API.Reflection.GetValue(obj.Reflection(fieldName), obj);
        }
        public static void InvokeMethod(this object obj, string methodName, params object[] parameters)
        {
            obj.Reflection(methodName).InvokeChecked(obj, parameters);
        }
    }
}

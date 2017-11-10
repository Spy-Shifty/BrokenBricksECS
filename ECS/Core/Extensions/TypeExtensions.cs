using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ECS.Extensions {
    static class TypeExtensions {
        public static IEnumerable<FieldInfo> GetFieldsRecursive(this Type type, BindingFlags bindingAttr) {
            if (type == null) {
                return new FieldInfo[0];
            }
            IEnumerable<FieldInfo> fields = type.GetFields(bindingAttr);
            var otherFields = GetFieldsRecursive(type.BaseType, bindingAttr);
            fields = fields.Concat(otherFields);
            return fields;
        }

        public static IEnumerable<Type> GetTypesRecursive(this Type type) {
            if (type == null) {
                return new Type[0];
            }            
            var otherTypes = GetTypesRecursive(type.BaseType);
            otherTypes = otherTypes.Concat(new Type[] { type });
            return otherTypes;
        }
    }
}

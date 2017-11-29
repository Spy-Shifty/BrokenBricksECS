using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ECS {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class InjectTupleAttribute : Attribute  {
        public int GroupId;

        public InjectTupleAttribute(string group) {
            GroupId = group.GetHashCode();
        }
        public InjectTupleAttribute(int groupId) {
            GroupId = groupId;
        }

        public InjectTupleAttribute() {
            GroupId = 0;
        }
    }
}

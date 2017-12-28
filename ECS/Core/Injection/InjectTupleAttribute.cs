using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ECS {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class InjectTupleAttribute : Attribute  {
        public int GroupId;
        public string GroupName;

        public InjectTupleAttribute(string group) {
            GroupId = group.GetHashCode();
            GroupName = group;
        }
        public InjectTupleAttribute(int groupId) {
            GroupId = groupId;
            GroupName = groupId.ToString();
        }

        public InjectTupleAttribute() {
            GroupId = 0;
        }

        public override bool Equals(object obj) {
            if(!(obj is InjectTupleAttribute)) {
                return false;
            }
            return GroupId.Equals((obj as InjectTupleAttribute).GroupId);
        }

        public override int GetHashCode() {
            return GroupId.GetHashCode();
        }
    }
}

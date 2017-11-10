using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ECS {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class InjectTupleAttribute : Attribute  {
    }
}

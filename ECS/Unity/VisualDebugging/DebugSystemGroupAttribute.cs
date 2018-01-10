using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DebugSystemGroupAttribute : Attribute {
    public readonly string Group;
    public DebugSystemGroupAttribute(string group) {
        Group = group;
    }
}

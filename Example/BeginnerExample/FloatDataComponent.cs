using ECS;
using System;

namespace ECSExample.Beginner {
    [Serializable]
    public struct FloatComponent : IComponent {
        public float value;

        public FloatComponent(float value) {
            this.value = value;
        }
    }

    public class FloatDataComponent : ComponentDataWrapper<FloatComponent> { }

}
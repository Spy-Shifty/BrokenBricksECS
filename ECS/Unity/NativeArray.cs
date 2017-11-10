using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {
    public class NativeArray<T> : IEnumerable<T> {

        private readonly T[] _data;
        public NativeArray(int length) {
            _data = new T[length];
        }

        public T this[int index] {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        public int Length { get { return _data.Length; } }

        public IEnumerator<T> GetEnumerator() {
            int index = 0;
            while(index < _data.Length) {
                yield return _data[index];
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            int index = 0;
            while (index < _data.Length) {
                yield return _data[index];
                index++;
            }
        }
    }
}

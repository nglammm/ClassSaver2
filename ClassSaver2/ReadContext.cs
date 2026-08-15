using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ClassSaver2
{
    public sealed class ReadContext
    {
        private readonly Dictionary<int, object> _dataMap = new Dictionary<int, object>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int code, object data)
        {
            _dataMap[code] = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(int code) where T : class
        {
            return (T)_dataMap[code];
        }
    }
}
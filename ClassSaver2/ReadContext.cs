using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ClassSaver2
{
    public class ReadContext
    {
        private readonly Dictionary<int, object> _dataMap = new Dictionary<int, object>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int code, object data)
        {
            _dataMap[code] = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Get(int code)
        {
            return _dataMap[code];
        }
    }
}
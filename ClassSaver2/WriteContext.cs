using System.Collections.Generic;

namespace ClassSaver2
{
    public sealed class WriteContext
    {
        private Dictionary<object, int> _objectCodeMap = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
        private int _nextIndex = 0;

        public bool TryGet(object data, out int index)
        {
            return _objectCodeMap.TryGetValue(data, out index);
        }

        public int Add(object data)
        {
            int myIndex = _nextIndex;
            
            _objectCodeMap.Add(data, myIndex);
            _nextIndex++;
            
            return myIndex;
        }
    }
}
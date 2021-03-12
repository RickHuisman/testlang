using System.Collections.Generic;

namespace testlang
{
    public class CallFrame
    {
        public ObjFunction Function;
        public int Ip;
        public SliceableArray<Value> Slots;
    }
}
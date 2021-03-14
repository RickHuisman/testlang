using System.Collections.Generic;

namespace testlang
{
    public class CallFrame
    {
        public ObjClosure Closure;
        public int Ip;
        public SliceableArray<Value> Slots;
    }
}
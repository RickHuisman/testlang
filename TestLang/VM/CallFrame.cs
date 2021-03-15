using System.Collections.Generic;

namespace testlang.Scanner
{
    public class CallFrame
    {
        public ObjClosure Closure;
        public int Ip;
        public SliceableArray<Value> Slots;
    }
}
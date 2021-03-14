using System;

namespace testlang
{
    public abstract class Obj
    {
        public Obj(ObjType type)
        {
            Type = type;
        }

        public virtual ObjType Type { get; private set; }
    }

    public class ObjString : Obj
    {
        public ObjString() : base(ObjType.String)
        {
        }

        public string Chars;
        public int Length => Chars.Length;

        public static ObjString CopyString(string src)
        {
            return new ObjString
            {
                Chars = new string(src),
            };
        }

        public override string ToString()
        {
            return Chars;
        }
    }

    public class ObjFunction : Obj
    {
        public ObjFunction() : base(ObjType.Function)
        {
            Arity = 0;
            UpValueCount = 0;
            Name = null;
            Chunk = new Chunk("test"); // TODO
        }

        public int Arity;
        public int UpValueCount;
        public Chunk Chunk;
        public ObjString Name;

        public override string ToString()
        {
            return $"<fn {(Name == null ? "<script>" : Name.Chars)}>";
        }
    }

    public class ObjNative : Obj
    {
        public ObjNative() : base(ObjType.Native)
        {
        }

        public Func<int, Value[], Value> Func { get; set; }

        public override string ToString()
        {
            return $"<native fn>";
        }
    }

    public class ObjClosure : Obj
    {
        public ObjFunction Function;
        
        public ObjClosure(ObjFunction function) : base(ObjType.Closure)
        {
            Function = function;
        }
    }

    public enum ObjType
    {
        Closure,
        Function,
        String,
        Native,
    }
}
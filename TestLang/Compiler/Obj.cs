using System;
using System.Collections.Generic;

namespace testlang.Scanner
{
    public abstract class Obj
    {
        public Obj(ObjType type)
        {
            Type = type;
        }

        public ObjType Type { get; }
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
        public int Arity;
        public int UpValueCount;
        public Chunk Chunk;
        public ObjString Name { get; private set; }

        public ObjFunction() : base(ObjType.Function)
        {
            Arity = 0;
            UpValueCount = 0;
            Name = null;
            Chunk = new Chunk();
        }

        public void SetName(string name)
        {
            Name = ObjString.CopyString(name);
            Chunk.Name = name;
        }

        public override string ToString()
        {
            return $"<fn {(Name == null ? "<script>" : Name.Chars)}>";
        }
    }

    public class ObjNative : Obj
    {
        public Func<int, Value[], Value> Func { get; set; }
        
        public ObjNative() : base(ObjType.Native)
        {
        }

        public override string ToString()
        {
            return "<native fn>";
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

    public class ObjStruct : Obj
    {
        public ObjString Name { get; }
        
        public ObjStruct(string name) : base(ObjType.Struct)
        {
            Name = ObjString.CopyString(name);
        }
    }

    public class ObjInstance : Obj
    {
        public ObjStruct Struct { get; }
        public Dictionary<string, Value> Fields { get; }
        
        public ObjInstance(ObjStruct objStruct) : base(ObjType.Instance)
        {
            Struct = objStruct;
            Fields = new Dictionary<string, Value>();
        }
    }

    public enum ObjType
    {
        Struct,
        Instance,
        Closure,
        Function,
        String,
        Native,
    }
}
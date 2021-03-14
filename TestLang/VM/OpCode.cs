namespace testlang
{
    public enum OpCode : byte
    {
        Constant,
        Add,
        Subtract,
        Multiply,
        Divide,
        Negate,
        Nil,
        Return,
        Print,
        Equal,
        Greater,
        Less,
        Not,
        Pop,
        DefineGlobal,
        GetGlobal,
        SetGlobal,
        GetLocal,
        SetLocal,
        JumpIfFalse,
        Jump,
        Loop,
        Call,
        Closure
    }
}

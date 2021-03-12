namespace testlang
{
    public enum Precedence
    {
        None,
        Assignment,
        Or,
        And,
        Equality,
        Comparison,
        Term,
        Factor,
        Unary,
        Call,
        Primary,
    }

    // public static class Precedence
    // {
    //     public static (int, int) PREC_NONE = (-1, -1);
    //     public static (int, int) PREC_ASSIGNMENT = (3, 4); // =
    //     public static (int, int) PREC_OR = (5, 6); // or
    //     public static (int, int) PREC_AND = (7, 8); // and
    //     public static (int, int) PREC_EQUALITY = (9, 10); // == !=
    //     public static (int, int) PREC_COMPARISON = (11, 12); // < > <= >=
    //     public static (int, int) PREC_TERM = (13, 14); // + -
    //     public static (int, int) PREC_FACTOR = (15, 16); // * /
    //     public static (int, int) PREC_UNARY = (-1, 18); // ! - TODO -1???
    //     public static (int, int) PREC_CALL = (19, 20); // . ()
    //     public static (int, int) PREC_PRIMARY = (21, 22); //
    // }
}
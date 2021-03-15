namespace testlang.Scanner
{
    public class UpValue
    {
        public int Index;
        public bool IsLocal;

        public UpValue(int index, bool isLocal)
        {
            Index = index;
            IsLocal = isLocal;
        }
    }
}
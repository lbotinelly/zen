namespace Zen.Base.Module
{
    public class DataOperationControl<T>
    {
        public T Current;
        public bool IsNew;
        public string Message;
        public T Original;
        public bool Success = true;
    }
}
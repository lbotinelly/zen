namespace Zen.Base.Common
{
    public interface IZenProvider
    {
        EOperationalStatus OperationalStatus { get; }
        void Initialize();
        string GetState();
    }
}
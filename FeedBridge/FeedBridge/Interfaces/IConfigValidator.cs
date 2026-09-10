namespace FeedBridge.Interfaces
{
    public interface IConfigValidator
    {
        void Validate(object config);
        bool CanValidate(object config);
    }
}
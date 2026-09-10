namespace FeedBridge.Services
{
    public class ReflectionHandler<T>
    {
        public bool IsFieldEmpty(string field, object instance)
        {
            var pInfo = typeof(T).GetProperty(field);

            if (pInfo == null || pInfo.GetValue(instance) == null || string.IsNullOrEmpty(pInfo.GetValue(instance).ToString()))
                return true;

            return false;
        }
    }
}
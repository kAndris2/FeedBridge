using FeedBridge.Interfaces;

namespace FeedBridge.Services.Validators
{
    public abstract class ConfigValidatorBase<T> : IConfigValidator
    {
        protected T? config;

        public void Validate(object config)
        {
            try
            {
                if (config is not T)
                    throw new ArgumentException("The configuration type is incorrect!");

                this.config = (T)config;

                ValidateListFields();
                ValidateSingleFields();
                ValidateObjectFields();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"An error occurred during the config ({typeof(T).Name}) validation! Ex.: {ex.Message}");
            }
        }

        public bool CanValidate(object config)
        {
            return config is T;
        }

        protected abstract void ValidateSingleFields();
        protected abstract void ValidateListFields();
        protected abstract void ValidateObjectFields();

        protected void CheckSingleFields<C>(List<string> fields, object instance)
        {
            if (instance == null)
                throw new ArgumentException($"{typeof(C).Name} cannot be empty!");

            var reflectionHandler = new ReflectionHandler<C>();

            foreach (var field in fields)
            {
                if (reflectionHandler.IsFieldEmpty(field, instance))
                    throw new ArgumentException($"The field '{field}' is required and must not be null or empty!");
            }
        }

        protected void CheckListCount(int? count, string listName)
        {
            if (count == null || count == 0)
                throw new ArgumentException($"The configuration of {listName} cannot be empty!");
        }
    }
}
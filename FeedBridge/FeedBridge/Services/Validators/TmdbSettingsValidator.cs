using FeedBridge.Models.Configuration;

namespace FeedBridge.Services.Validators
{
    public class TmdbSettingsValidator : ConfigValidatorBase<TmdbSettings>
    {
        protected override void ValidateSingleFields()
        {
            var requiredSingleFields = new List<string>()
            {
                nameof(config.ApiKey)
            };

            CheckSingleFields<TmdbSettings>(requiredSingleFields, config);
        }

        protected override void ValidateListFields()
        {
        }

        protected override void ValidateObjectFields()
        {
        }
    }
}
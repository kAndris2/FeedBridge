using FeedBridge.Models.Configuration;

namespace FeedBridge.Services.Validators
{
    public class NcoreSettingsValidator : ConfigValidatorBase<NcoreSettings>
    {
        protected override void ValidateSingleFields()
        {
            var requiredSingleFields = new List<string>()
            {
                nameof(config.RssUrl)
            };

            if (string.IsNullOrEmpty(config.RssUrl) && !string.IsNullOrEmpty(config.RssFilePath)) return;

            CheckSingleFields<NcoreSettings>(requiredSingleFields, config);
        }

        protected override void ValidateListFields()
        {
            CheckListCount(config.CategoryFilter?.Length, nameof(config.CategoryFilter));
        }

        protected override void ValidateObjectFields()
        {
        }
    }
}
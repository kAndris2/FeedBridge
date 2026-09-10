using FeedBridge.Models.Configuration;

namespace FeedBridge.Services.Validators
{
    public class GoogleDriveSettingsValidator : ConfigValidatorBase<GoogleDriveSettings>
    {
        protected override void ValidateSingleFields()
        {
            var requiredSingleFields = new List<string>()
            {
                nameof(config.TargetFolder),
                nameof(config.JsonFileName)
            };

            CheckSingleFields<GoogleDriveSettings>(requiredSingleFields, config);
        }

        protected override void ValidateListFields()
        {
        }

        protected override void ValidateObjectFields()
        {
        }
    }
}
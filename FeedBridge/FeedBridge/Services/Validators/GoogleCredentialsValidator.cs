using FeedBridge.Models.Configuration;

namespace FeedBridge.Services.Validators
{
    public class GoogleCredentialsValidator : ConfigValidatorBase<GoogleCredentials>
    {
        protected override void ValidateSingleFields()
        {
            var requiredSingleFields = new List<string>()
            {
                nameof(config.ClientId),
                nameof(config.ClientSecret)
            };

            CheckSingleFields<GoogleCredentials>(requiredSingleFields, config);
        }

        protected override void ValidateListFields()
        {
        }

        protected override void ValidateObjectFields()
        {
        }
    }
}
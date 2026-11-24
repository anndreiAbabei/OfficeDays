namespace OfficeDays.Features.Operations.GetConfiguration.Contracts;

public sealed record GetConfigurationResponse(PublishedPrivacyNotice? PrivacyNotice, double PersistentLoginSeconds);

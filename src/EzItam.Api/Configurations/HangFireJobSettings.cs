namespace EzItam.Api.Configurations
{
    public class HangFireJobSettings
    {
        public const string SectionName = "HangFireJobSetting";

        public HrSyncSettings HrSync { get; set; } = new();
    }

    public class HrSyncSettings
    {
        public string JobId { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string OrgCode { get; set; } = string.Empty;
    }
}

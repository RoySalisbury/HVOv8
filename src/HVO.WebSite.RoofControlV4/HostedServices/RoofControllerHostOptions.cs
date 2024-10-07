namespace HVO.WebSite.RoofControlV4.HostedServices
{
    public record RoofControllerHostOptions
    {
        public int RestartOnFailureWaitTime { get; set; } = 10;
    }

}
namespace LoggingExample.Web.Models.OptionModels;

internal class SeriLogConfigModel
{
    public required string ProjectName { get; set; }
    public required string ElasticUri { get; set; }
    public required string Environment { get; set; }
    public required string ElasticUser { get; set; }
    public required string ElasticPassword { get; set; }
}
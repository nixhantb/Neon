namespace Neon.Samples;
public class DataProcessingService
{
    private readonly ILogger<DataProcessingService> _logger;

    public DataProcessingService(ILogger<DataProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessUserDataAsync(int userId)
    {
        _logger.LogInformation("Processing data for user {UserId}", userId);
        await Task.Delay(3000); // Simulate heavy processing
        _logger.LogInformation("Data processing completed for user {UserId}", userId);
    }

    public async Task CleanupTempFilesAsync()
    {
        _logger.LogInformation("Starting cleanup of temporary files");
        await Task.Delay(1000);
        _logger.LogInformation("Temporary files cleanup completed");
    }

    public void GenerateReport(string reportType, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Generating {ReportType} report from {StartDate} to {EndDate}",
            reportType, startDate, endDate);

        Thread.Sleep(2000); // Simulate report generation

        _logger.LogInformation("{ReportType} report generated successfully", reportType);
    }
}

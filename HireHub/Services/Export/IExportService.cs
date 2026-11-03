namespace HireHub.Services.Export
{
    public interface IExportService
    {
        byte[] ExportPaymentsToCsv(List<Models.Payment> payments);
        byte[] ExportUsersToCsv(List<Models.User> users);
        byte[] ExportApplicationsToCsv(List<Models.Application> applications);
    }
}
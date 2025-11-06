namespace PDF_Server.Domain.Interfaces
{
    public interface ILocalStorageService
    {
        Task<bool> SendPdfToLocalStorageAsync(byte[] pdfBytes, string fileName, string correlationId);
    }
}

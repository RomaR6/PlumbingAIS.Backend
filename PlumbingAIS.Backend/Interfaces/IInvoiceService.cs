namespace PlumbingAIS.Backend.Interfaces
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(int transactionId);
    }
}
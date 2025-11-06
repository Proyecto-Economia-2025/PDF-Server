namespace PDF_Server.Domain.Models
{
    public class TopProductsRequest : BaseRequest
    {
        public TopProductsPayload Payload { get; set; }
    }

    public class TopProductsPayload
    {
        public List<ProductSale> TopProducts { get; set; }
        public string requestedAction { get; set; }
        public Metadata Metadata { get; set; }
    }
}
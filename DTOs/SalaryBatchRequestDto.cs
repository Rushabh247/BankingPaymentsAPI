namespace BankingPaymentsAPI.DTOs
{

    public class SalaryBatchRequestDto
    {
        public int ClientId { get; set; }
        public string BatchCode { get; set; }
        public List<SalaryItemDto> Items { get; set; }
    }
}

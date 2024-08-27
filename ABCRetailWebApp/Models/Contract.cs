namespace ABCRetailWebApp.Models
{
    public class Contract
    {
        public string ContractId { get; set; } // Unique identifier for the contract
        public string Title { get; set; }      // Title of the contract
        public string Content { get; set; }    // Content of the contract (could be a path to the file in Azure Files)
    }

}

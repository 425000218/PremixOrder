namespace PremixOrderAPI.Models;

public class PurchaseOrderRowDto
{
    public int? PODetailID { get; set; }
    public string PONumber { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public string SupplierShortName { get; set; } = "";
    public string RegionName { get; set; } = "SOUTH";
    public string DivisionName { get; set; } = "Livestock";
    public string ProductCode { get; set; } = "";
    public string FactoryCode { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string VATRate { get; set; } = "NonVAT";
    public string? DeliveryMonth { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Note { get; set; }
}
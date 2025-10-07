﻿namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public class UpdateSaleItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsCancelled { get; set; }
    }
}

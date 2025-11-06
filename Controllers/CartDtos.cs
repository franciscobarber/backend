using System;

namespace RetailDemo.Dtos
{
    public class AddToCartRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}


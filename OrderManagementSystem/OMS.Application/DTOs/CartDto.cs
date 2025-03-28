using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Application.DTOs
{
    public class CartDto
    {
        public int CustomerId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalAmount { get; set; }
        public int ItemCount => Items?.Count ?? 0;
    }
}

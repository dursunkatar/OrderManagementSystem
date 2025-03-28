using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Domain.Entities
{
    [Table("CartItems")]
    public class CartItem
    {
        public int Id { get; private set; }
        public int CartId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        private CartItem() { }

        public static CartItem Create(int productId, string productName, decimal price, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

            return new CartItem
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                Quantity = quantity
            };
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

            Quantity = quantity;
        }
    }
}

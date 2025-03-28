using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Domain.Entities
{
    public class Cart
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public ICollection<CartItem> Items { get; private set; } = new List<CartItem>();

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public decimal TotalAmount => Items.Sum(i => i.Price * i.Quantity);

        private Cart() { }

        public static Cart Create(int customerId)
        {
            return new Cart
            {
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void AddItem(int productId, string productName, decimal price, int quantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                var newItem = CartItem.Create(productId, productName, price, quantity);
                Items.Add(newItem);
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItemQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
                throw new InvalidOperationException($"Ürün sepette bulunamadı: {productId}");

            if (quantity <= 0)
            {
                RemoveItem(productId);
            }
            else
            {
                item.UpdateQuantity(quantity);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void RemoveItem(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                Items.Remove(item);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void Clear()
        {
            Items.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

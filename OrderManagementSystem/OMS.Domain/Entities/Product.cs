using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Domain.Entities
{
    [Table("Products")]
    public class Product
    {
        public int Id { get; private set; }
        public string Sku { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public int CategoryId { get; private set; }
        public string ImageUrl { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        private Product() { }
        public static Product Create(
            string sku,
            string name,
            string description,
            decimal price,
            int stockQuantity,
            int categoryId,
            string imageUrl = null)
        {
            if (string.IsNullOrEmpty(sku))
                throw new ArgumentNullException(nameof(sku), "Ürün SKU değeri boş olamaz.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "Ürün adı boş olamaz.");

            if (price < 0)
                throw new ArgumentException("Ürün fiyatı negatif olamaz.", nameof(price));

            if (stockQuantity < 0)
                throw new ArgumentException("Stok miktarı negatif olamaz.", nameof(stockQuantity));

            return new Product
            {
                Sku = sku,
                Name = name,
                Description = description,
                Price = price,
                StockQuantity = stockQuantity,
                CategoryId = categoryId,
                ImageUrl = imageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Ürün fiyatı negatif olamaz.", nameof(newPrice));

            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int quantity)
        {
            if (StockQuantity + quantity < 0)
                throw new InvalidOperationException("Stok miktarı yetersiz.");

            StockQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool ReserveStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Rezervasyon miktarı pozitif olmalıdır.", nameof(quantity));

            if (StockQuantity < quantity)
                return false;

            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public void Update(string name, string description, int? categoryId, string imageUrl)
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;

            Description = description;

            if (categoryId.HasValue)
                CategoryId = categoryId.Value;

            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}

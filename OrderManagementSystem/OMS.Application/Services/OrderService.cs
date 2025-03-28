using Microsoft.Extensions.Logging;
using OMS.Application.DTOs;
using OMS.Application.Events;
using OMS.Application.Interfaces;
using OMS.Domain;
using OMS.Domain.Entities;

namespace OMS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICacheService _cacheService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OrderService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        // Cache anahtarları için sabitler
        private const string ORDER_CACHE_KEY = "order_{0}"; // {0} yerine order id gelecek
        private const string CUSTOMER_ORDERS_CACHE_KEY = "customer_{0}_orders_page_{1}_size_{2}"; // {0} yerine customer id, {1} yerine page, {2} yerine pageSize gelecek

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            ICacheService cacheService,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("Sipariş oluşturma işlemi başlatıldı. Müşteri: {CustomerId}", request.CustomerId);

                // Müşteri kontrolü
                var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    _logger.LogWarning("Müşteri bulunamadı: {CustomerId}", request.CustomerId);
                    throw new InvalidOperationException($"Müşteri bulunamadı: {request.CustomerId}");
                }

                // Siparişteki ürünleri kontrol et ve OrderItem'ları oluştur
                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                foreach (var item in request.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Ürün bulunamadı: {ProductId}", item.ProductId);
                        throw new InvalidOperationException($"Ürün bulunamadı: {item.ProductId}");
                    }

                    // Stok kontrolü
                    if (!product.ReserveStock(item.Quantity))
                    {
                        _logger.LogWarning("Yetersiz stok. Ürün: {ProductId}, İstenen: {Quantity}, Mevcut: {Available}",
                            product.Id, item.Quantity, product.StockQuantity + item.Quantity);
                        throw new InvalidOperationException($"Yetersiz stok. Ürün: {product.Name}, İstenen: {item.Quantity}");
                    }

                    var orderItem = OrderItem.Create(
                        product.Id,
                        product.Name,
                        product.Price,
                        item.Quantity);

                    orderItems.Add(orderItem);
                    totalAmount += product.Price * item.Quantity;
                }

                // Sipariş oluştur
                var order = Order.Create(request.CustomerId, orderItems);

                // Adresi kaydet
                var shippingAddress = MapAddressFromRequest(request.ShippingAddress);
                // Burada sipariş ile adres ilişkisi kurulabilir

                // Ödeme bilgilerini kaydet (gerçek bir ödeme işlemi yapılabilir)
                // Burada ödeme işlemi gerçekleştirilebilir

                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Siparişi kaydet
                    await _orderRepository.AddAsync(order);

                    // Ürün stok güncellemelerini kaydet
                    foreach (var item in request.Items)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        await _productRepository.UpdateAsync(product);
                    }

                    // Değişiklikleri uygula
                    await _unitOfWork.SaveChangesAsync();

                    // Transaction tamamla
                    await _unitOfWork.CommitTransactionAsync();

                    // Event yayınla
                    var orderCreatedEvent = new OrderCreatedEvent
                    {
                        OrderId = order.Id,
                        CustomerId = order.CustomerId,
                        TotalAmount = order.TotalAmount,
                        CreatedAt = order.CreatedAt
                    };

                    await _eventPublisher.PublishAsync(orderCreatedEvent);

                    // Dönüş değerini hazırla
                    var orderDto = MapOrderToDto(order);

                    _logger.LogInformation("Sipariş başarıyla oluşturuldu. Sipariş ID: {OrderId}", order.Id);

                    return orderDto;
                }
                catch (Exception ex)
                {
                    // Hata durumunda transaction geri al
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Sipariş oluşturma işlemi sırasında hata: {Message}", ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturma işlemi başarısız: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> GetOrderAsync(int orderId)
        {
            try
            {
                // Önce cache'e bak
                var cacheKey = string.Format(ORDER_CACHE_KEY, orderId);
                var cachedOrder = await _cacheService.GetAsync<OrderDto>(cacheKey);

                if (cachedOrder != null)
                {
                    _logger.LogInformation("Sipariş cache'ten alındı. Sipariş ID: {OrderId}", orderId);
                    return cachedOrder;
                }

                // Cache'te yoksa veritabanından getir
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Sipariş bulunamadı: {OrderId}", orderId);
                    return null;
                }

                // DTO'ya dönüştür
                var orderDto = MapOrderToDto(order);

                // Cache'e ekle (1 saat geçerli)
                await _cacheService.SetAsync(cacheKey, orderDto, TimeSpan.FromHours(1));

                _logger.LogInformation("Sipariş veritabanından alındı ve cache'e eklendi. Sipariş ID: {OrderId}", orderId);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayları alınırken hata: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResult<OrderDto>> GetCustomerOrdersAsync(int customerId, int page, int pageSize)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                // Önce cache'e bak
                var cacheKey = string.Format(CUSTOMER_ORDERS_CACHE_KEY, customerId, page, pageSize);
                var cachedResult = await _cacheService.GetAsync<PagedResult<OrderDto>>(cacheKey);

                if (cachedResult != null)
                {
                    _logger.LogInformation("Müşteri siparişleri cache'ten alındı. Müşteri ID: {CustomerId}, Page: {Page}", customerId, page);
                    return cachedResult;
                }

                // Cache'te yoksa veritabanından getir
                var orders = await _orderRepository.GetByCustomerIdAsync(customerId, page, pageSize);
                var totalCount = await _orderRepository.GetCustomerOrderCountAsync(customerId);

                // DTO'ya dönüştür
                var orderDtos = orders.Select(MapOrderToDto).ToList();

                var result = new PagedResult<OrderDto>
                {
                    Items = orderDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                // Cache'e ekle (15 dakika geçerli)
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

                _logger.LogInformation("Müşteri siparişleri veritabanından alındı ve cache'e eklendi. Müşteri ID: {CustomerId}, Page: {Page}", customerId, page);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri siparişleri alınırken hata: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> CancelOrderAsync(int orderId)
        {
            // CancelOrderAsync(Guid orderId) metodu yerine bu metodu kullanacağız
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("İptal edilecek sipariş bulunamadı: {OrderId}", orderId);
                    throw new InvalidOperationException($"Sipariş bulunamadı: {orderId}");
                }

                if (order.StatusId != Const.OrderStatus.PENDING)
                {
                    _logger.LogWarning("Sipariş iptal edilemez çünkü durumu Pending değil. Sipariş ID: {OrderId}, Mevcut Durum: {Status}",
                        orderId, order.Status.Name);
                    throw new InvalidOperationException($"Sadece Pending durumundaki siparişler iptal edilebilir. Mevcut durum: {order.Status.Name}");
                }

                var oldStatus = order.Status.Name;

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    order.Cancel();
                    await _orderRepository.UpdateAsync(order);

                    int[] productIds = order.Items.Select(o => o.ProductId).ToArray();

                    var products = await _productRepository.GetListByProductIdsAsync(productIds);

                    foreach (var product in products)
                    {
                        var item = order.Items.Single(p => p.ProductId == product.Id);
                        // Stok miktarını geri ekle
                        product.UpdateStock(item.Quantity);
                        await _productRepository.UpdateAsync(product);
                    }

                    // Değişiklikleri uygula
                    await _unitOfWork.SaveChangesAsync();

                    // Transaction tamamla
                    await _unitOfWork.CommitTransactionAsync();

                    // Event yayınla
                    var statusChangedEvent = new OrderStatusChangedEvent
                    {
                        OrderId = order.Id,
                        OldStatus = oldStatus,
                        NewStatus = order.Status.Name,
                        ChangedAt = DateTime.UtcNow
                    };

                    await _eventPublisher.PublishAsync(statusChangedEvent);

                    // Cache'ten sil (artık geçersiz)
                    await _cacheService.RemoveAsync(string.Format(ORDER_CACHE_KEY, orderId));

                    // DTO'ya dönüştür
                    var orderDto = MapOrderToDto(order);

                    _logger.LogInformation("Sipariş başarıyla iptal edildi. Sipariş ID: {OrderId}", orderId);
                    return orderDto;
                }
                catch (Exception ex)
                {
                    // Hata durumunda transaction geri al
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Sipariş iptal edilirken hata: {Message}", ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş iptal edilirken hata: {Message}", ex.Message);
                throw;
            }
        }

        // OrderService.cs içindeki eksik metotlar
        public async Task<OrderDto> CompleteOrderAsync(int orderId)
        {
            // CompleteOrderAsync(Guid orderId) metodu yerine bu metodu kullanacağız
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Tamamlanacak sipariş bulunamadı: {OrderId}", orderId);
                    throw new InvalidOperationException($"Sipariş bulunamadı: {orderId}");
                }

                // Sipariş durumunu kontrol et
                if (order.StatusId != Const.OrderStatus.PENDING)
                {
                    _logger.LogWarning("Sipariş tamamlanamaz çünkü durumu Pending değil. Sipariş ID: {OrderId}, Mevcut Durum: {Status}",
                        orderId, order.Status.Name);
                    throw new InvalidOperationException($"Sadece Pending durumundaki siparişler tamamlanabilir. Mevcut durum: {order.Status.Name}");
                }

                // Eski durumu kaydet (event için)
                var oldStatus = order.Status.Name;

                // Siparişi tamamla
                order.Complete();

                // Güncelle
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Event yayınla
                var statusChangedEvent = new OrderStatusChangedEvent
                {
                    OrderId = order.Id,
                    OldStatus = oldStatus,
                    NewStatus = order.Status.Name,
                    ChangedAt = DateTime.UtcNow
                };

                await _eventPublisher.PublishAsync(statusChangedEvent);

                // Cache'ten sil (artık geçersiz)
                await _cacheService.RemoveAsync(string.Format(ORDER_CACHE_KEY, orderId));

                // DTO'ya dönüştür
                var orderDto = MapOrderToDto(order);

                _logger.LogInformation("Sipariş başarıyla tamamlandı. Sipariş ID: {OrderId}", orderId);
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş tamamlanırken hata: {Message}", ex.Message);
                throw;
            }
        }

        // OrderService.cs içine MapOrderToDto yardımcı metodu ekleyin
        private OrderDto MapOrderToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName,
                Status = order.Status?.Name,
                CreatedAt = order.CreatedAt,
                CompletedAt = order.CompletedAt,
                CancelledAt = order.CancelledAt,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}

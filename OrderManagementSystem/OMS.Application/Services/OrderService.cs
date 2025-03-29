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
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICacheService _cacheService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OrderService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        
        private const string ORDER_CACHE_KEY = "order_{0}"; 
        private const string CUSTOMER_ORDERS_CACHE_KEY = "customer_{0}_orders_page_{1}_size_{2}"; 

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            ICacheService cacheService,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            ILogger<OrderService> logger,
            ICartRepository cartRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cartRepository = cartRepository;
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(CreateOrderFromCartRequest request, int customerId)
        {
            try
            {
                _logger.LogInformation("Sepetten sipariş oluşturma işlemi başlatıldı. Müşteri: {CustomerId}", customerId);

                
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning("Müşteri bulunamadı: {CustomerId}", customerId);
                    throw new InvalidOperationException($"Müşteri bulunamadı: {customerId}");
                }

                
                var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Sepet bulunamadı veya boş: {CustomerId}", customerId);
                    throw new InvalidOperationException("Sepette ürün bulunmamaktadır.");
                }

                
                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                foreach (var cartItem in cart.Items)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Ürün bulunamadı: {ProductId}", cartItem.ProductId);
                        throw new InvalidOperationException($"Ürün bulunamadı: {cartItem.ProductId}");
                    }

                    
                    if (!product.ReserveStock(cartItem.Quantity))
                    {
                        _logger.LogWarning("Yetersiz stok. Ürün: {ProductId}, İstenen: {Quantity}, Mevcut: {Available}",
                            product.Id, cartItem.Quantity, product.StockQuantity + cartItem.Quantity);
                        throw new InvalidOperationException($"Yetersiz stok. Ürün: {product.Name}, İstenen: {cartItem.Quantity}");
                    }

                    var orderItem = OrderItem.Create(
                        product.Id,
                        product.Name,
                        product.Price,
                        cartItem.Quantity);

                    orderItems.Add(orderItem);
                    totalAmount += product.Price * cartItem.Quantity;
                }

              
                var order = Order.Create(customerId, request.ShippingAddress, orderItems);

              
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    
                    await _orderRepository.AddAsync(order);

                    
                    foreach (var item in cart.Items)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        await _productRepository.UpdateAsync(product);
                    }

                    
                    await _unitOfWork.SaveChangesAsync();

                    
                    await _unitOfWork.CommitTransactionAsync();

                    
                    var orderCreatedEvent = new OrderCreatedEvent
                    {
                        OrderId = order.Id,
                        CustomerId = order.CustomerId,
                        TotalAmount = order.TotalAmount,
                        CreatedAt = order.CreatedAt
                    };

                    await _eventPublisher.PublishAsync(orderCreatedEvent);

                    
                    await _cartRepository.ClearCartAsync(customerId);
                    await _unitOfWork.SaveChangesAsync();

                    
                    var orderDto = MapOrderToDto(order);

                    _logger.LogInformation("Sepetten sipariş başarıyla oluşturuldu. Sipariş ID: {OrderId}", order.Id);

                    return orderDto;
                }
                catch (Exception ex)
                {
                    
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Sepetten sipariş oluşturma işlemi sırasında hata: {Message}", ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepetten sipariş oluşturma işlemi başarısız: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> GetOrderAsync(int orderId)
        {
            try
            {
                
                var cacheKey = string.Format(ORDER_CACHE_KEY, orderId);
                var cachedOrder = await _cacheService.GetAsync<OrderDto>(cacheKey);

                if (cachedOrder != null)
                {
                    _logger.LogInformation("Sipariş cache'ten alındı. Sipariş ID: {OrderId}", orderId);
                    return cachedOrder;
                }

                
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Sipariş bulunamadı: {OrderId}", orderId);
                    return null;
                }

                
                var orderDto = MapOrderToDto(order);

                
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

                
                var cacheKey = string.Format(CUSTOMER_ORDERS_CACHE_KEY, customerId, page, pageSize);
                var cachedResult = await _cacheService.GetAsync<PagedResult<OrderDto>>(cacheKey);

                if (cachedResult != null)
                {
                    _logger.LogInformation("Müşteri siparişleri cache'ten alındı. Müşteri ID: {CustomerId}, Page: {Page}", customerId, page);
                    return cachedResult;
                }

                
                var orders = await _orderRepository.GetByCustomerIdAsync(customerId, page, pageSize);
                var totalCount = await _orderRepository.GetCustomerOrderCountAsync(customerId);

                
                var orderDtos = orders.Select(MapOrderToDto).ToList();

                var result = new PagedResult<OrderDto>
                {
                    Items = orderDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                
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
                        
                        product.UpdateStock(item.Quantity);
                        await _productRepository.UpdateAsync(product);
                    }

                    
                    await _unitOfWork.SaveChangesAsync();

                    
                    await _unitOfWork.CommitTransactionAsync();

                    
                    var statusChangedEvent = new OrderStatusChangedEvent
                    {
                        OrderId = order.Id,
                        OldStatus = oldStatus,
                        NewStatus = order.Status.Name,
                        ChangedAt = DateTime.UtcNow
                    };

                    await _eventPublisher.PublishAsync(statusChangedEvent);

                    
                    await _cacheService.RemoveAsync(string.Format(ORDER_CACHE_KEY, orderId));

                    
                    var orderDto = MapOrderToDto(order);

                    _logger.LogInformation("Sipariş başarıyla iptal edildi. Sipariş ID: {OrderId}", orderId);
                    return orderDto;
                }
                catch (Exception ex)
                {
                    
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

        
        public async Task<OrderDto> CompleteOrderAsync(int orderId)
        {
            
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Tamamlanacak sipariş bulunamadı: {OrderId}", orderId);
                    throw new InvalidOperationException($"Sipariş bulunamadı: {orderId}");
                }

                
                if (order.StatusId != Const.OrderStatus.PENDING)
                {
                    _logger.LogWarning("Sipariş tamamlanamaz çünkü durumu Pending değil. Sipariş ID: {OrderId}, Mevcut Durum: {Status}",
                        orderId, order.Status.Name);
                    throw new InvalidOperationException($"Sadece Pending durumundaki siparişler tamamlanabilir. Mevcut durum: {order.Status.Name}");
                }

                
                var oldStatus = order.Status.Name;

                
                order.Complete();

                
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                
                var statusChangedEvent = new OrderStatusChangedEvent
                {
                    OrderId = order.Id,
                    OldStatus = oldStatus,
                    NewStatus = "COMPLETED",
                    ChangedAt = DateTime.UtcNow
                };

                await _eventPublisher.PublishAsync(statusChangedEvent);

                
                await _cacheService.RemoveAsync(string.Format(ORDER_CACHE_KEY, orderId));

                
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

        
        private OrderDto MapOrderToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName,
                Status = "COMPLETED",
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

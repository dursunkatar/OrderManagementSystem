using Microsoft.Extensions.Logging;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;
using OMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CartService> _logger;

        private const string CART_CACHE_KEY = "cart_customer_{0}"; // {0} yerine customer id gelecek

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<CartDto> GetCartAsync(int customerId)
        {
            try
            {
                // Önce cache'e bak
                var cacheKey = string.Format(CART_CACHE_KEY, customerId);
                var cachedCart = await _cacheService.GetAsync<CartDto>(cacheKey);

                if (cachedCart != null)
                {
                    _logger.LogInformation("Sepet cache'ten alındı. Müşteri ID: {CustomerId}", customerId);
                    return cachedCart;
                }

                // Cache'te yoksa veritabanından getir
                var cart = await _cartRepository.GetByCustomerIdAsync(customerId);

                if (cart == null)
                {
                    _logger.LogInformation("Müşteri için sepet bulunamadı, yeni oluşturuluyor. Müşteri ID: {CustomerId}", customerId);
                    return new CartDto { CustomerId = customerId };
                }

                // DTO'ya dönüştür
                var cartDto = MapCartToDto(cart);

                // Cache'e ekle (30 dakika geçerli)
                await _cacheService.SetAsync(cacheKey, cartDto, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Sepet veritabanından alındı. Müşteri ID: {CustomerId}", customerId);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet getirilirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<CartDto> AddToCartAsync(int customerId, AddToCartRequest request)
        {
            try
            {
                _logger.LogInformation("Sepete ürün ekleniyor. Müşteri ID: {CustomerId}, Ürün ID: {ProductId}, Miktar: {Quantity}",
                    customerId, request.ProductId, request.Quantity);

                // Ürün bilgilerini getir
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Ürün bulunamadı: {ProductId}", request.ProductId);
                    throw new InvalidOperationException($"Ürün bulunamadı: {request.ProductId}");
                }

                // Stok kontrolü
                if (product.StockQuantity < request.Quantity)
                {
                    _logger.LogWarning("Yetersiz stok. Ürün: {ProductId}, İstenen: {Quantity}, Mevcut: {Available}",
                        product.Id, request.Quantity, product.StockQuantity);
                    throw new InvalidOperationException($"Yetersiz stok. Ürün: {product.Name}, İstenen: {request.Quantity}, Mevcut: {product.StockQuantity}");
                }

                // Müşterinin sepetini getir veya oluştur
                var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    cart = Cart.Create(customerId);
                    await _cartRepository.AddAsync(cart);
                }

                // Sepete ürün ekle
                cart.AddItem(product.Id, product.Name, product.Price, request.Quantity);

                await _unitOfWork.SaveChangesAsync();


                // Sepeti güncelle
                await _cartRepository.UpdateAsync(cart);
                await _unitOfWork.SaveChangesAsync();

                // Cache'i temizle
                var cacheKey = string.Format(CART_CACHE_KEY, customerId);
                await _cacheService.RemoveAsync(cacheKey);

                var cartDto = MapCartToDto(cart);

                _logger.LogInformation("Ürün sepete eklendi. Müşteri ID: {CustomerId}, Ürün ID: {ProductId}", customerId, request.ProductId);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepete ürün eklenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<CartDto> UpdateCartItemAsync(int customerId, UpdateCartItemRequest request)
        {
            try
            {
                _logger.LogInformation("Sepetteki ürün güncelleniyor. Müşteri ID: {CustomerId}, Ürün ID: {ProductId}, Miktar: {Quantity}",
                    customerId, request.ProductId, request.Quantity);

                // Müşterinin sepetini getir
                var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    _logger.LogWarning("Sepet bulunamadı. Müşteri ID: {CustomerId}", customerId);
                    throw new InvalidOperationException($"Sepet bulunamadı. Müşteri ID: {customerId}");
                }

                if (request.Quantity <= 0)
                {
                    // Ürünü sepetten kaldır
                    cart.RemoveItem(request.ProductId);
                }
                else
                {
                    // Ürün stok kontrolü
                    var product = await _productRepository.GetByIdAsync(request.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Ürün bulunamadı: {ProductId}", request.ProductId);
                        throw new InvalidOperationException($"Ürün bulunamadı: {request.ProductId}");
                    }

                    if (product.StockQuantity < request.Quantity)
                    {
                        _logger.LogWarning("Yetersiz stok. Ürün: {ProductId}, İstenen: {Quantity}, Mevcut: {Available}",
                            product.Id, request.Quantity, product.StockQuantity);
                        throw new InvalidOperationException($"Yetersiz stok. Ürün: {product.Name}, İstenen: {request.Quantity}, Mevcut: {product.StockQuantity}");
                    }

                    // Sepetteki ürün miktarını güncelle
                    cart.UpdateItemQuantity(request.ProductId, request.Quantity);
                }

                // Sepeti güncelle
                await _cartRepository.UpdateAsync(cart);
                await _unitOfWork.SaveChangesAsync();

                // Cache'i temizle
                var cacheKey = string.Format(CART_CACHE_KEY, customerId);
                await _cacheService.RemoveAsync(cacheKey);

                var cartDto = MapCartToDto(cart);

                _logger.LogInformation("Sepetteki ürün güncellendi. Müşteri ID: {CustomerId}, Ürün ID: {ProductId}", customerId, request.ProductId);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepetteki ürün güncellenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int customerId, int productId)
        {
            try
            {
                _logger.LogInformation("Ürün sepetten kaldırılıyor. Müşteri ID: {CustomerId}, Ürün ID: {ProductId}", customerId, productId);

                // Müşterinin sepetini getir
                var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    _logger.LogWarning("Sepet bulunamadı. Müşteri ID: {CustomerId}", customerId);
                    return false;
                }

                // Ürünü sepetten kaldır
                cart.RemoveItem(productId);

                // Sepeti güncelle
                await _cartRepository.UpdateAsync(cart);
                await _unitOfWork.SaveChangesAsync();

                // Cache'i temizle
                var cacheKey = string.Format(CART_CACHE_KEY, customerId);
                await _cacheService.RemoveAsync(cacheKey);

                _logger.LogInformation("Ürün sepetten kaldırıldı. Müşteri ID: {CustomerId}, Ürün ID: {ProductId}", customerId, productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepetten kaldırılırken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> ClearCartAsync(int customerId)
        {
            try
            {
                _logger.LogInformation("Sepet temizleniyor. Müşteri ID: {CustomerId}", customerId);

                // Müşterinin sepetini getir
                var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    _logger.LogWarning("Sepet bulunamadı. Müşteri ID: {CustomerId}", customerId);
                    return false;
                }

                // Sepeti temizle
                cart.Clear();
                await _cartRepository.UpdateAsync(cart);
                await _unitOfWork.SaveChangesAsync();

                // Cache'i temizle
                var cacheKey = string.Format(CART_CACHE_KEY, customerId);
                await _cacheService.RemoveAsync(cacheKey);

                _logger.LogInformation("Sepet temizlendi. Müşteri ID: {CustomerId}", customerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet temizlenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        private CartDto MapCartToDto(Cart cart)
        {
            var cartDto = new CartDto
            {
                CustomerId = cart.CustomerId,
                TotalAmount = cart.TotalAmount,
                Items = cart.Items.Select(item => new CartItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            return cartDto;
        }
    }
}

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Infrastructure.Persistence
{
    public class UnitOfWork
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(OrderDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Veritabanı güncellenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                _logger.LogWarning("Zaten bir transaction aktif.");
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
            _logger.LogDebug("Transaction başlatıldı");
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();

                if (_currentTransaction == null)
                {
                    _logger.LogWarning("Commit edilecek aktif transaction bulunamadı.");
                    return;
                }

                await _currentTransaction.CommitAsync();
                _logger.LogDebug("Transaction commit edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction commit edilirken hata: {Message}", ex.Message);
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_currentTransaction == null)
                {
                    _logger.LogWarning("Geri alınacak aktif transaction bulunamadı.");
                    return;
                }

                await _currentTransaction.RollbackAsync();
                _logger.LogDebug("Transaction geri alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction geri alınırken hata: {Message}", ex.Message);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}

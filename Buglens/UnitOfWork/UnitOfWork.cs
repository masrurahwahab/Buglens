using Buglens.Contract.IRepository;
using BugLens.Data;
using Buglens.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace Buglens.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly BugLensContext _context;
    private IDbContextTransaction? _transaction;
    private IAnalysisRepository? _analysisRepository;

    public UnitOfWork(BugLensContext context)
    {
        _context = context;
    }

    public IAnalysisRepository Analyses =>
        _analysisRepository ??= new AnalysisRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started.");

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

  
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

  
    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
            await _transaction.DisposeAsync();

        await _context.DisposeAsync();
    }
}
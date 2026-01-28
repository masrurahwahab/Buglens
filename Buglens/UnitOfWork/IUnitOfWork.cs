
using Buglens.Contract.IRepository;

namespace Buglens.UnitOfWork;
public interface IUnitOfWork : IDisposable
{
    IAnalysisRepository Analyses { get; }
    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.TestHelpers;

public static class DbSetMockHelper
{
    public static DbSet<T> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        var provider = new AsyncQueryProvider<T>(data.Provider);
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => AsyncEnumeratorHelper<T>.Create(data.GetEnumerator()));
        
        return mockSet.Object;
    }

    public static DbSet<T> CreateMockDbSetFromList<T>(IEnumerable<T> data) where T : class
    {
        return CreateMockDbSet(data.AsQueryable());
    }

    public static Mock<DbSet<T>> CreateMockDbSetWithAddAsync<T>(IEnumerable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new AsyncQueryProvider<T>(data.AsQueryable().Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        // Setup AddAsync to complete without throwing
        mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>>(
                Task.FromResult(default(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>)!)));
        
        return mockSet;
    }
}

internal static class AsyncEnumeratorHelper<T> where T : class
{
    public static IAsyncEnumerator<T> Create(IEnumerator<T> enumerator)
    {
        return new AsyncEnumerator(enumerator);
    }

    private class AsyncEnumerator : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public AsyncEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public T Current => _enumerator.Current;

        public ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return default;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_enumerator.MoveNext());
        }
    }
}

internal class AsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _innerProvider;

    public AsyncQueryProvider(IQueryProvider innerProvider)
    {
        _innerProvider = innerProvider;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new AsyncEnumerable<T>(_innerProvider.CreateQuery<T>(expression));
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new AsyncEnumerable<TElement>(_innerProvider.CreateQuery<TElement>(expression));
    }

    public object Execute(System.Linq.Expressions.Expression expression)
    {
        return _innerProvider.Execute(expression)!;
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _innerProvider.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression)
    {
        return new AsyncEnumerable<TResult>(_innerProvider.CreateQuery<TResult>(expression));
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }
}

internal class AsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _queryable;

    public AsyncEnumerable(IQueryable<T> queryable)
    {
        _queryable = queryable;
    }

    public Type ElementType => _queryable.ElementType;
    public System.Linq.Expressions.Expression Expression => _queryable.Expression;
    public IQueryProvider Provider => new AsyncQueryProvider<T>(_queryable.Provider);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // Execute the query and return results asynchronously
        IEnumerable<T> results;
        try
        {
            results = _queryable.AsEnumerable();
        }
        catch
        {
            // If query execution fails, return empty enumerable
            results = Enumerable.Empty<T>();
        }

        return AsyncEnumeratorHelper<T>.Create(results.GetEnumerator());
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _queryable.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _queryable.GetEnumerator();
    }
}

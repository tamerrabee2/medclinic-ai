using System.Collections.Concurrent;

namespace MedClinic.Infrastructure.Services;

public interface IKeyedLockManager
{
    Task<IDisposable> AcquireLockAsync(string key, CancellationToken ct = default);
}

public sealed class KeyedLockManager : IKeyedLockManager
{
    private sealed class RefCountedSemaphore
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public int RefCount = 1;
    }

    private readonly ConcurrentDictionary<string, RefCountedSemaphore> _semaphores = new();

    public async Task<IDisposable> AcquireLockAsync(string key, CancellationToken ct = default)
    {
        RefCountedSemaphore item;
        lock (_semaphores)
        {
            if (_semaphores.TryGetValue(key, out var existing))
            {
                existing.RefCount++;
                item = existing;
            }
            else
            {
                item = new RefCountedSemaphore();
                _semaphores[key] = item;
            }
        }

        await item.Semaphore.WaitAsync(ct).ConfigureAwait(false);

        return new Releaser(this, key, item);
    }

    private void Release(string key, RefCountedSemaphore item)
    {
        item.Semaphore.Release();
        lock (_semaphores)
        {
            item.RefCount--;
            if (item.RefCount <= 0)
            {
                _semaphores.TryRemove(key, out _);
            }
        }
    }

    private sealed class Releaser : IDisposable
    {
        private KeyedLockManager? _manager;
        private readonly string _key;
        private readonly RefCountedSemaphore _item;

        public Releaser(KeyedLockManager manager, string key, RefCountedSemaphore item)
        {
            _manager = manager;
            _key = key;
            _item = item;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _manager, null)?.Release(_key, _item);
        }
    }
}

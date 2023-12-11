using System.Threading;
using System.Threading.Tasks;

public class AsyncLock
{
    private readonly SemaphoreSlim _semaphore;

    public AsyncLock()
    {
        _semaphore = new SemaphoreSlim(1);
    }

    public Task WaitAsync()
    {
        return _semaphore.WaitAsync();
    }

    public void Release()
    {
        _semaphore.Release();
    }
}
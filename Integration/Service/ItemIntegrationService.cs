using Integration.Backend;
using Integration.Common;
using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

    // This is called externally and can be called multithreaded, in parallel.
    // More than one item with the same content should not be saved. However,
    // calling this with different contents at the same time is OK, and should
    // be allowed for performance reasons.
    public Result SaveItem(string itemContent)
    {
        string connectionString = "localhost";

        var connection = ConnectionMultiplexer.Connect(connectionString);

        string lockKey = $"ItemIntegrationService_Item_{itemContent}";

        var @lock = new RedisDistributedLock(lockKey, connection.GetDatabase());

        using (var handle = @lock.TryAcquire())
        {
            if (handle != null)
            {
                // Check the backend to see if the content is already saved.
                if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
                {
                    return new Result(false, $"Duplicate item received with content {itemContent}.");
                }

                var item = ItemIntegrationBackend.SaveItem(itemContent);

                return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
            }
            else
            {
                Console.WriteLine("Lock is taken");

                return new Result(false, $"Lock is taken");
            }
        }
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}
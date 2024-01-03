using Integration.Service;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var service = new ItemIntegrationService();

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(500);

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(5000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItems().ForEach(Console.WriteLine);

        SaveItemsForParallelTasks(service);

        Console.WriteLine("Everything recorded for parellel tasks");

        service.GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();
    }

    private static void SaveItemsForParallelTasks(ItemIntegrationService service)
    {
        Task taskA = Task.Run(() => service.SaveItem("taskAItem"));

        Task taskB = Task.Run(() => service.SaveItem("taskBItem"));

        Task taskA1 = Task.Run(() => service.SaveItem("taskAItem"));

        Task taskB1 = Task.Run(() => service.SaveItem("taskBItem"));

        Task.WaitAll(taskA, taskB, taskA1, taskB1);
    }
}
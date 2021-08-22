using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

class Program2
{
    public static async Task Main2(string[] args)
    {
        Console.Title = "Test task demo";

        var stopwatch = Stopwatch.StartNew();

        var task = Enumerable.Range(0, 100).Select(i => dummy(i)).ToList();
        await Task.WhenAll(task);

        Console.WriteLine($"Total: {stopwatch.Elapsed}");
        Console.Read();
    }

    private async static Task<int> dummy(int index)
    {
        Console.WriteLine("Outer:" + index);
        int result = await DoAsync(index);
        return result;
    }

    private static async Task<int> DoAsync(int index)
    {
        Console.WriteLine("Inner:" + index);
        return await Task.Run(() => 1);
    }

    public static async Task Main3(string[] args)
    {
        Console.Title = "Test task demo";

        var stopwatch = Stopwatch.StartNew();

        var task = Enumerable.Range(0, 20).Select(i => Task.Run(() => { Console.WriteLine("Outer:" + i); return DoAsync(i).Result; })).ToList();
        await Task.WhenAll(task);

        Console.WriteLine($"Total: {stopwatch.Elapsed}");
        Console.Read();
    }


}

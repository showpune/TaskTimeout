using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static Random random = new Random();

    static Boolean scatter = false;

    static int totalService = 100;

    static int deploymentUnderService = 5;

    static int httpTimeOut = 1500;

    static int inspectTimeout = 20000;

    static int batchNumber = 4;

    static int timeOutNumber = 0;
    static async Task Main(string[] args)
    {
        int i = 1;
        if (i == 1)
        {
            await Program.Main1(args);
        }
        else if (i == 2)
        {
            await Program2.Main2(args);
        }
        else{
            await Program2.Main3(args);
        }
    }
    static async Task Main1(string[] args)
    {

        Console.WriteLine("Start without batch,totalService="+totalService);
        scatter = false;
        timeOutNumber = 0;
        var stopwatch1 = Stopwatch.StartNew();
        var task = ProduceAppAvailabilityService_exec();
        await Task.WhenAll(task);
        Console.WriteLine($"No Scatter total cost: {stopwatch1.Elapsed}"+",timeOutNumber:"+timeOutNumber);

        Console.WriteLine("Start with batch,totalService=" + totalService +",batchNumber="+ batchNumber);
        scatter = true;
        timeOutNumber = 0;
        stopwatch1 = Stopwatch.StartNew();
        task = ProduceAppAvailabilityService_exec();
        await Task.WhenAll(task);

        Console.WriteLine($"After Scatter total cost: {stopwatch1.Elapsed}" + ",timeOutNumber:" + timeOutNumber);


        Console.Read();
    }

    private async static Task ProduceAppAvailabilityService_exec()
    {
        List<Task> tasks = await CheckEndpointForServiceInstances();
        await Task.WhenAll(tasks);
    }

    private async static Task<List<Task>> CheckEndpointForServiceInstances()
    {
        List<Task> taskList = new List<Task>();
        await dummy("sql Query");
        if (scatter)
        {
            for (int j = 0; j < batchNumber; j++)
            {
                int numberUnderBatch = totalService / batchNumber;
                for (int i = j * numberUnderBatch; i < (j + 1) * numberUnderBatch; i++)
                {
                    taskList.Add(CheckSingleInstanceEndpoints(i));
                }
                await waitTime();
            }
            await Task.WhenAll(taskList);

            
        }
        else
        {
            for (int i = 0; i < 99; i++)
            {
                taskList.Add(CheckSingleInstanceEndpoints(i));
            }
            await Task.WhenAll(taskList);
        }
        return taskList;
    }

    private async static Task waitTime()
    {
        await Task.Run(() =>
        { 
            Thread.Sleep(60*1000/batchNumber);
        });
    }

    private async static Task dummy(String s)
    {
       // await Task.Run(() =>
       // Console.WriteLine(s));
        await Task.Run(() =>
        { 
            Thread.Sleep(1000);
        });
    }

    private async static Task<List<Task>> CheckSingleInstanceEndpoints(int i)
    {
        await Task.Delay(random.Next(5000));
        //Console.WriteLine("Start CheckSingleInstanceEndpoints:" + i);
       List <Task> taskList = new List<Task>();
        await dummy("init check "+i);
        for (int j = 0; j < deploymentUnderService; j++)
        {
            taskList.Add(CheckDeployment("Service:"+i+" Deployment: "+j));
        }
        //  Console.WriteLine("End CheckSingleInstanceEndpoints:" + i);
        await Task.WhenAll(taskList);
       // Console.WriteLine("Service:" + i);
        return taskList;
    }

    private async static Task CheckDeployment(String 
        i)
    {
        await Task.Delay(random.Next(5000));
        //Console.WriteLine("Start CheckDeployment:" + i);
        var stopwatch = Stopwatch.StartNew();
        await httpRequest("http1:"+i, httpConnection());
        await httpRequest("http2:"+i, httpConnection());
        //Console.WriteLine(i+$" deloyment cost: {stopwatch.Elapsed}");
        // Console.WriteLine("End CheckDeployment:" + i);
       // Console.WriteLine(i);
    }

    private async static Task httpRequest(string v,Task connection)
    {
        // Console.WriteLine(v);
        var stopwatch = Stopwatch.StartNew();
        var task = await Task.WhenAny(connection, Task.Delay(inspectTimeout));
      //  Console.WriteLine(v + $" http cost: {stopwatch.Elapsed}");
         if (task != connection)
         {
             Console.WriteLine(v+" is timeout, cost="+stopwatch.Elapsed);
            timeOutNumber++;
         }
        //Console.WriteLine(task.ToString());
        
    }

    private async static Task httpConnection()
    {
        await Task.Run(() => Thread.Sleep(httpTimeOut));
        
    }

}

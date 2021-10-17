
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ChoETL;
using ChoCSVReader = ChoETL.ChoCSVReader;
using Timer = System.Timers.Timer;

namespace ConsoleApp2
{
    class Program
    {

        public static BlockingCollection<ObjectToRead> Queue;
        public const string PayloadConsole = "Console";
        public const string PayloadFile = "File";

        private static async Task Run()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(10000); //cancelation Token cancel Task after 10 Seconds 
            Console.WriteLine("Press a key to start the Execution");
            Console.ReadKey();
            Timer timer = new Timer(10000);
            timer.Elapsed += TimerTick;
            timer.Start();
            //blocking collection to hold items 
            Queue = new BlockingCollection<ObjectToRead>();
            //task list 
            var t1 = Task.Run(() => NonBlockingProducer(cts.Token), cts.Token);
            var t2 = Task.Run(() =>  FileConsumer(cts), cts.Token);
            var t3 = Task.Run(() =>  ConsoleConsumer(cts), cts.Token);
            //start the task running 
            await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);
            timer.Stop();
            Console.WriteLine("Stopped");
            Console.Read();
        }

        static async Task Main(string[] args)
        {
            await Run();
        }
        
        private static void FileConsumer(CancellationTokenSource cts)
        {
            GenericConsumer(cts, PayloadFile);
        }
        
        private static void ConsoleConsumer(CancellationTokenSource cts)
        {
            GenericConsumer(cts, PayloadConsole);
        }
        
        private static void GenericConsumer(CancellationTokenSource cts, string typePayload)
        {
            while (!Queue.IsCompleted)
            {
                if (cts.IsCancellationRequested)
                {
                    "Cancelled".Dump();
                    return;
                }
                if (!Queue.TryTake(out var obj, Timeout.InfiniteTimeSpan))
                    Console.WriteLine("This is blocked");
                else if (IsItemApplies(typePayload, obj))
                {
                    switch (typePayload)
                    {
                        case PayloadConsole:
                            Console.WriteLine(obj.Payload);
                            break;
                        case PayloadFile:
                            File.AppendAllLinesAsync("temp.txt", new[] { obj.Payload });
                            break;
                    }
                  Thread.Sleep(10);// just for visuals 
                }
                else
                    Queue.Add(obj);
            }
        }
        
        // original non refactored code
        // private static void FileConsumer(CancellationTokenSource cts)
        // {
        //    
        //         
        //         while (!Queue.IsCompleted)
        //         {
        //             if (cts.IsCancellationRequested)
        //             {
        //                 "Cancelled".Dump();
        //                 return;
        //             }
        //             if (!Queue.TryTake(out var obj, Timeout.InfiniteTimeSpan))
        //             {
        //                 Console.WriteLine("This is blocked");
        //             }
        //
        //             else
        //             {
        //                 if (IsItemApplies("File", obj))
        //                 {
        //                     File.AppendAllLinesAsync("temp.txt", new[] { obj.Payload });
        //                 }
        //                 else
        //                 {
        //                      Queue.Add(obj);
        //                 }
        //             }
        //         }
        // }
        //
        // private static void ConsoleConsumer(CancellationTokenSource cts)
        // {
        //    
        //         while (!Queue.IsCompleted)
        //         {
        //             if (cts.IsCancellationRequested)
        //             {
        //                 "Cancelled".Dump();
        //                 return;
        //             }
        //             if (!Queue.TryTake(out var obj, Timeout.InfiniteTimeSpan))
        //             {
        //                 Console.WriteLine("This is blocked");
        //
        //             }
        //             else
        //             {
        //                 if (IsItemApplies("Console", obj))
        //                 {
        //                     Console.WriteLine(obj.Payload);
        //                 }
        //                 else
        //                 {
        //                      Queue.Add(obj);
        //                 }
        //             }
        //         }
        // }

        private static bool IsItemApplies(string type, ObjectToRead item)
        {
            return item != null && item.Type.Equals(type);
        }
        static void NonBlockingProducer(CancellationToken cts)
        {
            using var reader = new ChoCSVReader("CodingTest2020InputStimulus1.csv").WithFirstLineHeader();
            foreach (var item in reader)
            {
                
                try
                {
                    Queue.TryAdd(new ObjectToRead
                    {
                        Type = item[0],
                        Payload = item[1]

                    }, 20 ,cts);

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Add loop canceled.");
                    Queue.CompleteAdding();
                    break;
                }
            }
        }
        static void TimerTick(Object obj, ElapsedEventArgs e)
      {
         Console.WriteLine("Exiting");
         Environment.Exit(0);
      }
        
        
    }


}
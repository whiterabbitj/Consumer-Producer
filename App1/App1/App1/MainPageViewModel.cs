using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Prism.Mvvm;
using Prism.Navigation;
using Xamarin.Forms;
using static System.Text.ASCIIEncoding;

namespace App1
{
    public class MainPageViewModel : BindableBase, IInitialize , INotifyPropertyChanged
    {
        private const string PayloadConsole = "Console";
        private const string PayloadFile = "File";
        private BlockingCollection<ObjectDataModel> _queue;
        private ObservableCollection<ObjectDataModel> _listViewData;
        private List<ObjectDataModel> _list;  
        private string _stringVal;
        public ObservableCollection<ObjectDataModel> ListViewData
        {
            get => _listViewData;
            set => SetProperty(ref _listViewData, value);
        }
        public string StringVal
        {
            get => _stringVal;
            set => SetProperty(ref _stringVal, value);
        }
        private async void Run()
        {
        _queue = new BlockingCollection<ObjectDataModel>();
        var cts = new CancellationTokenSource();
        // cts.CancelAfter(10000); // comment this if you wish to see all of the dat 
        var t1 = Task.Run(() => NonBlockingProducer(cts.Token), cts.Token);
        var t2 = Task.Run(() =>FileConsumer(cts), cts.Token);
        var t3 = Task.Run(UiConsumer, cts.Token);
        var t4 = Task.Run(() =>ConsoleConsumer(cts), cts.Token);
        await Task.WhenAll(t1, t2, t3, t4).ConfigureAwait(false);
        }
        private void FileConsumer(CancellationTokenSource cts)
        {
            GenericConsumer(cts, PayloadFile);
        }
        private  void ConsoleConsumer(CancellationTokenSource cts)
        {
            GenericConsumer(cts, PayloadConsole);
        }
        private  void GenericConsumer(CancellationTokenSource cts, string typePayload)
        {
            while (!_queue.IsCompleted)
            {
                if (cts.IsCancellationRequested) return;
                    if (!_queue.TryTake(out var obj, Timeout.InfiniteTimeSpan))
                    Console.WriteLine("This is blocked");
                else if (IsItemApplies(typePayload, obj))
                {
                    switch (typePayload)
                    {
                        case PayloadConsole:
                            Console.WriteLine(obj.Payload);
                            break;
                        case PayloadFile:
                            InsertData(obj); // insert into SQL lite DB 
                            //todo figure out hot to write to file without blocking the thread. 
                            // StreamWriter outputFile = new(temp());
                            // outputFile.WriteAsync(obj.Payload);
                            // System.IO.File.WriteAllText(temp(), obj.Payload.ToString());
                            // var assembly = typeof(MainPageViewModel).GetTypeInfo().Assembly; 
                            // var stream = assembly.GetManifestResourceStream("App1.Output.txt");
                            // stream.Write(Encoding.Default.GetBytes(obj.Payload), Int32.MaxValue, Int32.MaxValue);
                            // stream.Flush();
                            // File.AppendAllLinesAsync("temp.txt", new[] { obj.Payload });
                            break;
                    }
                     Thread.Sleep(100); // just for visuals non blovking UI interaction 
                    _list.Add(obj);
                    OnPropertyChanged(nameof(_list));
                   
                }
                else
                    _queue.Add(obj);
            }
        }
        private  void UiConsumer()
         { 
           try
           {
              while (!_queue.IsCompleted)
              {
                  ListViewData = new ObservableCollection<ObjectDataModel>(_list);
                  OnPropertyChanged(nameof(ListViewData));
                  StringVal = "Total Element Count - " + ListViewData.Count;
                  OnPropertyChanged(nameof(StringVal));
                  Thread.Sleep(100);  // just for visuals non blocking UI interaction 

              }
           }
           catch(Exception exception)
           {
               Console.WriteLine(exception);
           }
         }
         private  bool IsItemApplies(string type, ObjectDataModel item)
         {
             return item != null && item.Type.Equals(type);
         }
         private void NonBlockingProducer(CancellationToken cts)
         {
             var assembly =  typeof(MainPageViewModel).GetTypeInfo().Assembly;
             using var stream = assembly.GetManifestResourceStream("App1.CodingTest2020InputStimulus1.csv");
             using var reader = new StreamReader(stream);
             using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);
             csv.Read();
             csv.ReadHeader();
             while (csv.Read())
             {
                 try
                 {
                     _queue.TryAdd(new ObjectDataModel
                     {
                         Type = csv.GetField<string>(0),
                         Payload = csv.GetField<string>(1),
                     },100, cts);
                 }
                 catch (OperationCanceledException)
                 {
                     Console.WriteLine("Add loop canceled.");
                     _queue.CompleteAdding();
                     break;
                 }
             }
         }

         // private string temp()
         // {
         //     string filePath; 
         //     if(Device.RuntimePlatform == Device.Android)
         //        filePath = DependencyService.Get<IFileWriter>().getPath();
         //     else
         //        filePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
         //    return Path.Combine(filePath,"temp.txt");
         // }
         
         public event PropertyChangedEventHandler PropertyChanged;
         void OnPropertyChanged([CallerMemberName] string propertyName = null)
         {
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
         }
         public void Initialize(INavigationParameters parameters)
         {
             _list = new List<ObjectDataModel>();
             Run();
         }

         private static void InsertData(ObjectDataModel obj)
          {
            try
            {
                using var conn = DependencyService.Get<ISQliteInterface>().GetConnection();
                conn.CreateTable<ObjectDataModel>();
                conn.Insert(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}
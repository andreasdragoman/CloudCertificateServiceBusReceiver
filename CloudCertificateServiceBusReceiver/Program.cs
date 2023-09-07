using System.Text.Json;
using System.Text;
using Azure.Messaging.ServiceBus;
using Core.Models;
using System;
using System.Threading.Tasks;

string connectionString = "Endpoint=sb://andreas-service-bus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=tdOlUCsSgMXwKLT5Oifxh8eKlSl/7qqvF+ASbBoHQgc=";
string queueName = "personqueue";
ServiceBusClient client;
ServiceBusProcessor processor;


var clientOptions = new ServiceBusClientOptions()
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};

client = new ServiceBusClient(connectionString, clientOptions);
processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

try
{
    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    await processor.DisposeAsync();
    await client.DisposeAsync();
}


async Task MessageHandler(ProcessMessageEventArgs args)
{
    var body = args.Message.Body.ToArray();
    var jsonString = Encoding.UTF8.GetString(body);
    PersonModel person = JsonSerializer.Deserialize<PersonModel>(jsonString);

    Console.WriteLine($"Person Received: {person.FirstName} {person.LastName}");
    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine($"Exception: {args.Exception.ToString()}");
    return Task.CompletedTask;
}
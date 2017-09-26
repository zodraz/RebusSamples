using System;
using Consumer.Messages;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
using Rebus.SqlServer.Transport;
using Rebus;

namespace Producer
{
    class Program
    {
        static void Main()
        {
            using (var adapter = new BuiltinHandlerActivator())
            {
                //adapter.Handle<Reply>(async reply =>
                //{
                //    await Console.Out.WriteLineAsync($"Got reply '{reply.KeyChar}' (from OS process {reply.OsProcessId})");
                //});

                Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                    .Options(o => o.EnableSynchronousRequestReply(replyMaxAgeSeconds: 7))
                    .Transport(t => t.UseSqlServer("Data Source=VS2017-W2016;Initial Catalog=ActorMessages;Integrated Security=True;MultipleActiveResultSets=True", "Messages", "producer.input"))
                    .Routing(r => r.TypeBased().Map<Job>("consumer.input"))
                    .Start();

                Console.WriteLine("Press Q to quit , r for a request/response or any other key to produce a job");
                while (true)
                {
                    var keyChar = char.ToLower(Console.ReadKey(true).KeyChar);

                    switch (keyChar)
                    {
                        case 'q':
                            goto quit;

                        case 'r':
                            var reply = adapter.Bus.SendRequest<Reply>(new Job(keyChar)).Result;
                            break;

                        default:
                            adapter.Bus.Send(new Job(keyChar)).Wait();
                            break;
                    }
                }

            quit:
                Console.WriteLine("Quitting...");
            }
        }
    }
}

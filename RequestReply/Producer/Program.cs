using System;
using Consumer.Messages;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
using Rebus.SqlServer.Transport;
using Rebus;
using Rebus.Transport.InMem;

namespace Producer
{
    class Program
    {
        static void Main()
        {
            const string ConnectionString = "Data Source=VS2017-W2016;Initial Catalog=ActorMessages;Integrated Security=True;MultipleActiveResultSets=True";

            using (var adapter = new BuiltinHandlerActivator())
            {

                Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
                    .Options(o => o.EnableSynchronousRequestReply(replyMaxAgeSeconds: 7))
                     .Options(o => o.LogPipeline(verbose: true))
                    .Transport(t => t.UseSqlServer(ConnectionString, "Messages", "producer.input"))
                    .Routing(r => r.TypeBased().Map<Job>("consumer.input").Map<Job2>("consumer2.input"))
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
                            Console.WriteLine($"Got reply: {reply.KeyChar} from PID {reply.OsProcessId}");
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

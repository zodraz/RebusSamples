using System;
using System.Diagnostics;
using Consumer.Messages;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.SqlServer.Transport;
using System.Threading.Tasks;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

namespace Consumer
{
    public class Consumer
    {
        const string ConnectionString = "Data Source=VS2017-W2016;Initial Catalog=ActorMessages;Integrated Security=True;MultipleActiveResultSets=True";

        static void Main()
        {
            using (var adapter = new BuiltinHandlerActivator())
            {
                adapter.Handle<Job2>(async (bus, job) =>
                {
                    var keyChar = job.KeyChar;
                    var processId = Process.GetCurrentProcess().Id;
                    var reply = new Reply2(keyChar, processId);

                    await bus.Reply(reply);
                });

                Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
                    .Transport(t => t.UseSqlServer(ConnectionString, "Messages", "consumer2.input"))
                    .Routing(r => r.TypeBased().Map<Job>("consumer.input").Map<Job2>("consumer2.input"))
                    .Start();

                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
            }
        }
    }
}

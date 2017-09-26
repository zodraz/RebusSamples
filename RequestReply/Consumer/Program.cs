using System;
using System.Diagnostics;
using Consumer.Messages;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.SqlServer.Transport;
using System.Threading.Tasks;
using Rebus;
using Rebus.Routing.TypeBased;

namespace Consumer
{
    class Program
    {
        const string ConnectionString = "Data Source=VS2017-W2016;Initial Catalog=ActorMessages;Integrated Security=True;MultipleActiveResultSets=True";

        static void Main()
        {
            using (var adapter = new BuiltinHandlerActivator())
            {
                adapter.Handle<Job>(async (bus, job) =>
                {
                    var keyChar = job.KeyChar;
                    var processId = Process.GetCurrentProcess().Id;
                    var reply = new Reply(keyChar, processId);

                    var reply1 = adapter.Bus.SendRequest<Reply2>(new Job2(keyChar)).Result;
                    //Don't care for reply1...
                    await bus.Reply(reply);
                });

                Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                    .Transport(t => t.UseSqlServer(ConnectionString, "Messages", "consumer.input"))
                    .Routing(r => r.TypeBased().Map<Job2>("consumer2.input"))
                    .Start();

                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
            }
        }
    }
}

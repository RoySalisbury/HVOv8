using HVO.Hardware.PowerSystems.Voltronic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;


namespace HVO.Test.InverterQuery
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args);
            await host.RunConsoleAsync(o => o.SuppressStatusMessages = true);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
             .ConfigureServices(context => 
             {
                context.AddOptions();
                context.AddOptions<InverterClientOptions>();

                //.context.AddHostedService<InverterServiceHost>();




             });
    }
}





        //public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args);
    

            //using var client = new InverterCommunicationsClient();

            //var request1 = new InverterGetSerialNumberRequest();
            //var request2 = new InverterGetDeviceProtocolIDRequest();

            //client.Open();

            //var result1 = await client.SendRequest<InverterGetSerialNumberResponse>(request1);
            //var result2 = await client.SendRequest<InverterGetDeviceProtocolIDResponse>(request2);

            //using var client = new InverterClient(null, 
            //client.Open();

            //var r1 = await client.QPI();
            // var r2 = await client.QID();
            // var r3 = await client.QSID();
            // var r4 = await client.QVFW();
            // var r5 = await client.QVFW2(); //NAK
            // var r6 = await client.QVFW3();
            // var r7 = await client.VERFW(); // NAK
            // var r8 = await client.QPIRI();
            // var r9 = await client.QFLAG();
            // var r10 = await client.QPIGS();
            // var r11 = await client.QMOD();
            // var r12 = await client.QPIWS();
            // var r13 = await client.QDI();

            // var r14 = await client.QMCHGCR();
            // var r15 = await client.QMUCHGCR();
            // var r16 = await client.QOPPT();
            // var r17 = await client.QCHPT();
            // var r19 = await client.QMN();
            // var r20 = await client.QGMN();
            // var r21 = await client.QBEQI();


            // var r22 = await client.QET();
            // var r23 = await client.QEY();
            // var r24 = await client.QEM();
            // var r25 = await client.QED();
            // var r26 = await client.QLT();
            // var r27 = await client.QLY();
            // var r28 = await client.QLM();
            // var r29 = await client.QLD();
            // var r30 = await client.QLED();
            // var r31 = await client.Q1();
            // var r32 = await client.QBOOT();
            // var r33 = await client.QOPM();
            // var r34 = await client.QPGS();
            // var r35 = await client.QBV();


            //var r18 = await client.QT();
            //var r36 = await client.DAT(DateTime.Now);

//            var r0 = await client.Test();




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OPC UA Packages.
// using Microsoft.Extensions.Logging.Abstractions;
using Opc.Ua;
// using Opc.Ua.Server;
using Opc.Ua.Configuration;

namespace ConsoleApp1
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Create a telemetry context without logging output.
            ITelemetryContext telemetry = DefaultTelemetry.Create(builder => { });

            // Create the OPC UA application.
            var application = new ApplicationInstance(telemetry)
            {
                ApplicationName = "SimpleOpcUaServer",
                ApplicationType = ApplicationType.Server
            };

            await application
                .Build("urn:localhost:SimpleOpcUaServer", "urn:example:SimpleOpcUaServer")
                .AsServer(new[] { "opc.tcp://localhost:4840/SimpleOpcUaServer" })
                // This allows anonymous connection without a certificate.
                .AddUnsecurePolicyNone()
                .AddSignAndEncryptPolicies()
                .AddSecurityConfiguration("CN=SimpleOpcUaServer")
                .CreateAsync();

            bool isCertificateOK = await application.CheckApplicationInstanceCertificatesAsync(true);
            if (!isCertificateOK)
            {
                throw new Exception("Application certificate validation failed.");
            }

            // This starts the OPC UA server aysnchronously.
            var server = new MyOpcUaServer(telemetry);
            await application.StartAsync(server);

            // Write a message to the console.
            Console.WriteLine("OPC UA server started.");
            Console.WriteLine("opc.tcp://localhost:4840/SimpleOpcUaServer");
            Console.WriteLine("Press Enter to stop.");

            // Wait for user input on the console.
            Console.ReadLine();

            // Stop the aysnchronous OPC UA server.
            await application.StopAsync();
        }
    }
}

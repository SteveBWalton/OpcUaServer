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
            // ITelemetryContext telemetry = DefaultTelemetry.Create(NullLoggerFactory.Instance);
            ITelemetryContext telemetry = DefaultTelemetry.Create(builder => { });

            // Create the OPC UA application.
            var application = new ApplicationInstance(telemetry)
            {
                ApplicationName = "SimpleOpcUaServer",
                ApplicationType = ApplicationType.Server
            };

            /*
            await application
                .Build(
                    "urn:localhost:SimpleOpcUaServer",
                    "urn:example:SimpleOpcUaServer")
                .AsServer(new[]
                {
                    "opc.tcp://localhost:4840/SimpleOpcUaServer"
                })
                .Create();
            */

            /*
            await application
                .Build(
                    "urn:localhost:SimpleOpcUaServer",
                    "urn:example:SimpleOpcUaServer")
                .AsServer(new[]
                {
                    "opc.tcp://localhost:4840/SimpleOpcUaServer"
                })
                .AddSignAndEncryptPolicies()
                .SetSecurityConfiguration(
                    "CN=SimpleOpcUaServer",
                    "pki/own",
                    "pki/trusted",
                    "pki/issuer",
                    "pki/rejected")
                .Create();
            */

            await application
                .Build("urn:localhost:SimpleOpcUaServer", "urn:example:SimpleOpcUaServer")
                .AsServer(new[] { "opc.tcp://localhost:4840/SimpleOpcUaServer" })
                .AddSignAndEncryptPolicies()
                .AddSecurityConfiguration("CN=SimpleOpcUaServer")
                .CreateAsync();

            bool isCertificateOK = await application.CheckApplicationInstanceCertificatesAsync(true);

            if (!isCertificateOK)
            {
                throw new Exception("Application certificate validation failed.");
            }

            var server = new MyOpcUaServer(telemetry);

            await application.StartAsync(server);

            Console.WriteLine("OPC UA server started.");
            Console.WriteLine("opc.tcp://localhost:4840/SimpleOpcUaServer");
            Console.WriteLine("Press Enter to stop.");

            Console.ReadLine();

            await application.StopAsync();
        }
    }


























}

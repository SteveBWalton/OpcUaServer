// OPC UA Packages.
// using Microsoft.Extensions.Logging.Abstractions;
using Opc.Ua;
// using Opc.Ua.Server;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1
{
    internal class Program
    {
        #region Member Varaibles

        /// <summary>The parent OPC UA application instance for the OPC UA server.</summary>
        /// <remarks>The application instance is repsonable for security certificates, application configuration and starting and stopping the OPC UA server.
        /// The application instance can support a server or a client.</remarks>
        private static Opc.Ua.Configuration.ApplicationInstance _applicationInstance;

        #endregion

        /// <summary>Run the console program.</summary>
        /// <param name="args">Specifies the command line arguments.</param>
        /// <returns>Zero for success, an error code otherwise.</returns>
        private static int Main(string[] args)
        {
            // No OPC UA application instance.
            _applicationInstance = null;

            // Start the server asynchronous on a separate thread.
            Task.Run(async () => { await startServer(); });

            // Write a message to the console.
            Console.WriteLine("OPC UA server started.");
            Console.WriteLine("opc.tcp://localhost:4840/SimpleOpcUaServer");
            Console.WriteLine("Press Enter to stop.");

            // Wait for user input on the console.
            Console.ReadLine();

            // Stop the aysnchronous OPC UA server.
            if (_applicationInstance != null)
            {
                _applicationInstance.StopAsync();
                _applicationInstance= null;
            }

            // Write a message to the console.
            Console.WriteLine("OPC UA server stopped.");
            Console.WriteLine("Press Enter to finish.");

            // Wait for user input on the console.
            Console.ReadLine();

            // Return success.
            return 0;
        }



        /// <summary>Creates and starts the OPC UA server asynchronously after configuring its application instance.</summary>
        /// <returns>A task that represents the asynchronous server startup operation.</returns>
        private static async Task startServer()
        {
            // Create a telemetry context without logging output.
            ITelemetryContext telemetry = DefaultTelemetry.Create(builder => { });

            // Create the OPC UA application instance.
            _applicationInstance = new Opc.Ua.Configuration.ApplicationInstance(telemetry)
            {
                ApplicationName = "SimpleOpcUaServer",
                ApplicationType = ApplicationType.Server
            };

            await _applicationInstance
                .Build("urn:localhost:SimpleOpcUaServer", "urn:example:SimpleOpcUaServer")
                .AsServer(new[] { "opc.tcp://localhost:4840/SimpleOpcUaServer" })
                // This allows anonymous connection without a certificate.
                .AddUnsecurePolicyNone()
                .AddSignAndEncryptPolicies()
                .AddSecurityConfiguration("CN=SimpleOpcUaServer")
                .CreateAsync();

            bool isCertificateOK = await _applicationInstance.CheckApplicationInstanceCertificatesAsync(true);
            if (!isCertificateOK)
            {
                throw new Exception("Application certificate validation failed.");
            }

            // This starts the OPC UA server aysnchronously.
            MyOpcUaServer server = new MyOpcUaServer(telemetry);
            await _applicationInstance.StartAsync(server);
        }
    }
}

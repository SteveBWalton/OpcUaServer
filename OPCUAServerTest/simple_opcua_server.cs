using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using Opc.Ua;
using Opc.Ua.Server;
using OPCUAServerTest.Properties;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Utilities.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using static Opc.Ua.Server.UserDatabase.LinqUserDatabase;
using static System.Net.Mime.MediaTypeNames;

namespace OPCUAServerTest
{




































        // Custom Node Manager
        public class SimpleNodeManager : INodeManager
        {
            private readonly IServerInternal _server;
            private readonly ApplicationConfiguration _configuration;
            private readonly ushort _namespaceIndex;
            private BaseDataVariableState _temperatureNode;

            public SimpleNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            {
                _server = server;
                _configuration = configuration;
                _namespaceIndex = _server.NamespaceUris.GetIndexOrAppend("urn:localhost:SimpleOpcUaServer");
                CreateAddressSpace();
            }

            private void CreateAddressSpace()
            {
                // Create folder node
                var folder = new FolderState(null)
                {
                    NodeId = new NodeId("MyDevices", _namespaceIndex),
                    BrowseName = new QualifiedName("MyDevices", _namespaceIndex),
                    DisplayName = "MyDevices",
                    TypeDefinitionId = ObjectTypeIds.FolderType
                };
                folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);

                // Create variable node
                _temperatureNode = new BaseDataVariableState(folder)
                {
                    NodeId = new NodeId("Temperature", _namespaceIndex),
                    BrowseName = new QualifiedName("Temperature", _namespaceIndex),
                    DisplayName = "Temperature",
                    TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                    DataType = DataTypeIds.Double,
                    ValueRank = ValueRanks.Scalar,
                    Value = 25.0,
                    StatusCode = StatusCodes.Good,
                    Timestamp = DateTime.UtcNow
                };

                folder.AddChild(_temperatureNode);

                // Add nodes to the server
                _server.NodeManager.AddNode(folder);
                _server.NodeManager.AddNode(_temperatureNode);

                Console.WriteLine("Address space created: MyDevices → Temperature");
            }
        }

        // Custom Server
        public class SimpleServer : StandardServer
        {
            protected override NodeManagerCollection CreateNodeManagers(IServerInternal server, ApplicationConfiguration configuration)
            {
                var nodeManagers = new NodeManagerCollection
            {
                new CustomNodeManagerWrapper(server, configuration)
            };
                return nodeManagers;
            }
        }

        // Wrapper to adapt our SimpleNodeManager to NodeManagerCollection
        public class CustomNodeManagerWrapper : CustomNodeManager2
        {
            public CustomNodeManagerWrapper(IServerInternal server, ApplicationConfiguration configuration)
                : base(server, configuration, "urn:localhost:SimpleOpcUaServer")
            {
                SystemContext.NodeIdFactory = this;

                // Create folder node
                var folder = new FolderState(null)
                {
                    NodeId = new NodeId("MyDevices", NamespaceIndex),
                    BrowseName = new QualifiedName("MyDevices", NamespaceIndex),
                    DisplayName = "MyDevices",
                    TypeDefinitionId = ObjectTypeIds.FolderType
                };
                folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);

                // Create variable node
                var temperatureNode = new BaseDataVariableState(folder)
                {
                    NodeId = new NodeId("Temperature", NamespaceIndex),
                    BrowseName = new QualifiedName("Temperature", NamespaceIndex),
                    DisplayName = "Temperature",
                    TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                    DataType = DataTypeIds.Double,
                    ValueRank = ValueRanks.Scalar,
                    Value = 25.0,
                    StatusCode = StatusCodes.Good,
                    Timestamp = DateTime.UtcNow
                };

                folder.AddChild(temperatureNode);

                // Register nodes
                AddPredefinedNode(SystemContext, folder);
                AddPredefinedNode(SystemContext, temperatureNode);
            }
        }

        class Program
        {
            static void Main(string[] args)
            {
                try
                {
                    // Correct BaseAddresses initialization for ArrayOf<string>
                    var baseAddresses = new Opc.Ua.ArrayOf<string>
                    {
                        Items = new string[] { "opc.tcp://localhost:4840" }
                    };

                    var config = new ApplicationConfiguration
                    {
                        ApplicationName = "SimpleOpcUaServer",
                        ApplicationUri = "urn:localhost:SimpleOpcUaServer",
                        ApplicationType = ApplicationType.Server,
                        ServerConfiguration = new ServerConfiguration
                        {
                            BaseAddresses = baseAddresses
                        },
                        SecurityConfiguration = new SecurityConfiguration
                        {
                            ApplicationCertificate = new CertificateIdentifier(),
                            AutoAcceptUntrustedCertificates = true
                        }
                    };

                    config.Validate(ApplicationType.Server).Wait();

                    var server = new SimpleServer();
                    server.Start(config);

                    Console.WriteLine("OPC UA Server started at opc.tcp://localhost:4840");
                    Console.WriteLine("Press Ctrl+C to exit...");

                    Thread.Sleep(Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error starting server: " + ex.Message);
                }
            }
        }





















}

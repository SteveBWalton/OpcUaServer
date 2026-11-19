using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// OPC UA Libraries.
using Opc.Ua;
using Opc.Ua.Server;

namespace ConsoleApp1
{
    /// <summary>Class to represent the OPC UA Server.</summary>
    /// <remarks>Really the only difference to a StandardServer is our custom node manager to add our folders and variables.</remarks>
    public class MyOpcUaServer : StandardServer
    {
        /// <summary>Constructor.</summary>
        /// <param name="telemetry"></param>
        public MyOpcUaServer(ITelemetryContext telemetry)
            : base(telemetry)
        {
        }

        /// <summary>Create a node manager for the server.</summary>
        /// <param name="server"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        protected override IMasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            var nodeManagers = new INodeManager[]
            {
                new MyNodeManager(server, configuration)
            };

            return new MasterNodeManager(server, configuration, null, nodeManagers);
        }
    }
}

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
    public class MyOpcUaServer : StandardServer
    {
        public MyOpcUaServer(ITelemetryContext telemetry)
            : base(telemetry)
        {
        }

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

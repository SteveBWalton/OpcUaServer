using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// OPC UA Libraries.
using Opc.Ua;
using Opc.Ua.Server;


namespace ConsoleApp1
{
    /// <summary>Class to represent a node manager for the OPC UA server.</summary>
    /// <remarks>This adds folders and variable to the server.</remarks>
    public class MyNodeManager : CustomNodeManager2
    {
        #region Member Variables

        /// <summary>A temperature variable to export via OPC UA.</summary>
        private BaseDataVariableState _temperature;
        /// <summary>A pressure variable to export via OPC UA.</summary>
        private BaseDataVariableState _pressure;
        /// <summary>A running variable to export via OPC UA.</summary>
        private BaseDataVariableState _running;

        /// <summary>Timer to change the value of the temperature in time.</summary>
        private Timer _timer;

        #endregion

        #region Constructors etc

        /// <summary>Constructor.</summary>
        /// <param name="server">Specifes the OPC UA server that this will manage nodes for.</param>
        /// <param name="configuration"></param>
        public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration, "urn:example:SimpleOpcUaServer:Data")
        {
            SystemContext.NodeIdFactory = this;
        }

        /// <summary>Release the resources used at the end of life for the object.</summary>
        /// <param name="disposing">Specifies true when the object is really end of life.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
                _timer = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                ushort ns = NamespaceIndexes[0];

                // Create a folder to hold the variables.
                FolderState folder = new FolderState(null)
                {
                    SymbolicName = "MyDevice",
                    NodeId = new NodeId("MyDevice", ns),
                    BrowseName = new QualifiedName("MyDevice", ns),
                    DisplayName = (Opc.Ua.LocalizedText)"MyDevice",
                    TypeDefinitionId = ObjectTypeIds.FolderType
                };

                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
                {
                    references = new List<IReference>();

                    externalReferences[ObjectIds.ObjectsFolder] = references;
                }

                references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, folder.NodeId));
                folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
                AddPredefinedNode(SystemContext, folder);
                _temperature = createVariable(folder, "Temperature", 20.0);
                _pressure = createVariable(folder, "Pressure", 1.0);
                _running = createVariable(folder, "Running", true);
                _timer = new Timer(updateValues, null, 1000, 1000);
            }
        }

        /// <summary>Create an OPC UA variable with the speciifed name in the specified folder with the specified initial value.</summary>
        /// <param name="parent">Specifies the folder that contains the new OPC UA variable.</param>
        /// <param name="name">Specifies the name for the new OPC UA variable.</param>
        /// <param name="initialValue">Specifies the initial value and the type of the new OPC UA variable.</param>
        /// <returns>The OPC UA variable created.</returns>
        private BaseDataVariableState createVariable(NodeState parent, string name, Variant initialValue)
        {
            ushort ns = NamespaceIndexes[0];

            var variable = new BaseDataVariableState(parent)
            {
                SymbolicName = name,
                NodeId = new NodeId("MyDevice." + name, ns),
                BrowseName = new QualifiedName(name, ns),
                DisplayName = (Opc.Ua.LocalizedText)name,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                DataType = TypeInfo.GetDataTypeId(initialValue),
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Value = initialValue,
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow
            };

            parent.AddChild(variable);

            AddPredefinedNode(SystemContext, variable);

            return variable;
        }

        /// <summary>Signal handler for the timer control to update the OPC UA values.</summary>
        /// <param name="state"></param>
        private void updateValues(object state)
        {
            lock (Lock)
            {
                if (_temperature == null)
                {
                    return;
                }

                double value = Convert.ToDouble(_temperature.Value.ToString());

                value += 0.1;

                if (value > 30.0)
                {
                    value = 20.0;
                }

                _temperature.Value = value;
                _temperature.Timestamp = DateTime.UtcNow;

                _temperature.ClearChangeMasks(SystemContext, false);
            }
        }

    }
}

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





    public class MyNodeManager : CustomNodeManager2
    {
        private BaseDataVariableState temperature;
        private BaseDataVariableState pressure;
        private BaseDataVariableState running;

        private Timer timer;

        public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration, "urn:example:SimpleOpcUaServer:Data")
        {
            SystemContext.NodeIdFactory = this;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                ushort ns = NamespaceIndexes[0];

                var folder = new FolderState(null)
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
                temperature = CreateVariable(folder, "Temperature", 20.0);
                pressure = CreateVariable(folder, "Pressure", 1.0);
                running = CreateVariable(folder, "Running", true);
                timer = new Timer(UpdateValues, null, 1000, 1000);
            }
        }

        // private BaseDataVariableState CreateVariable(            NodeState parent,            string name,            object initialValue)
        private BaseDataVariableState CreateVariable(NodeState parent, string name, Variant initialValue)
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

        private void UpdateValues(object state)
        {
            lock (Lock)
            {
                if (temperature == null)
                {
                    return;
                }

                double value = Convert.ToDouble(temperature.Value.ToString());

                value += 0.1;

                if (value > 30.0)
                {
                    value = 20.0;
                }

                temperature.Value = value;
                temperature.Timestamp = DateTime.UtcNow;

                temperature.ClearChangeMasks(SystemContext, false);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Dispose();
                timer = null;
            }

            base.Dispose(disposing);
        }
    }























}

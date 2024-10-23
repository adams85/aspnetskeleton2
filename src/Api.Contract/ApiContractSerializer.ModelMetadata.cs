using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf.Meta;

namespace WebApp.Api;

public partial class ApiContractSerializer
{
    public sealed class ModelMetadataProvider
    {
        public static bool IsCollectionType(Type type)
        {
            return Array.FindIndex(type.GetInterfaces(), intf =>
                intf == typeof(IEnumerable)
                || intf.IsGenericType && intf.FullName == "System.Collections.Generic.IAsyncEnumerable`1") >= 0;
        }

        private readonly RuntimeTypeModel _typeModel;

        internal ModelMetadataProvider(RuntimeTypeModel typeModel)
        {
            _typeModel = typeModel;
        }

        public bool ShouldSerializeAsCollection(Type collectionType) => !_typeModel[collectionType].IgnoreListHandling;

        public bool CanSerialize(Type type) => _typeModel.CanSerialize(type);

        public IEnumerable<Type> GetSubTypes(Type type)
        {
            if (type.IsValueType || type.IsArray || _typeModel.CanSerializeBasicType(type))
            {
                return Type.EmptyTypes;
            }

            var visited = new HashSet<Type>() { type };
            return Visit(type, _typeModel, visited);

            static IEnumerable<Type> Visit(Type type, RuntimeTypeModel typeModel, HashSet<Type> visited)
            {
                var subTypes = typeModel.Add(type, applyDefaultBehaviour: true).GetSubtypes();

                for (int i = 0; i < subTypes.Length; i++)
                {
                    if (visited.Add(type = subTypes[i].DerivedType.Type))
                    {
                        foreach (var subType in Visit(type, typeModel, visited))
                            yield return subType;

                        yield return type;
                    }
                }
            }
        }

        public IEnumerable<ValueMember> GetMembers(Type type, out MetaType? metaType)
        {
            if (_typeModel.CanSerializeBasicType(type))
            {
                metaType = null;
                return Array.Empty<ValueMember>();
            }

            metaType = _typeModel.Add(type, applyDefaultBehaviour: true);
            return Visit(metaType);

            static IEnumerable<ValueMember> Visit(MetaType metaType)
            {
                do
                {
                    // NOTE: GetFields return fields sorted by FieldNumber.
                    var fields = metaType.GetFields();
                    for (int i = 0, n = fields.Length; i < n; i++)
                        yield return fields[i];
                }
                while ((metaType = metaType.BaseType) != null);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf.Meta;
using static ProtoBuf.Meta.TypeModel;

namespace WebApp.Api;

public partial class ApiContractSerializer
{
    public sealed class ModelMetadataProvider
    {
        private readonly RuntimeTypeModel _typeModel;

        internal ModelMetadataProvider(RuntimeTypeModel typeModel)
        {
            _typeModel = typeModel;
        }

        public bool ShouldSerializeAsList(Type type) => typeof(IEnumerable).IsAssignableFrom(type) && !_typeModel[type].IgnoreListHandling;

        public bool CanSerialize(Type type) => _typeModel.CanSerialize(type);

        public IEnumerable<Type> GetSubTypes(Type type) => GetSubTypes(type, out var _);

        public IEnumerable<Type> GetSubTypes(Type type, out HashSet<Type>? visited)
        {
            if (type.IsValueType || type.IsArray || _typeModel.CanSerializeBasicType(type))
            {
                visited = null;
                return Type.EmptyTypes;
            }

            visited = new HashSet<Type>();
            return Visit(type, _typeModel, visited);

            static IEnumerable<Type> Visit(Type type, RuntimeTypeModel typeModel, HashSet<Type> visited)
            {
                var subTypes = typeModel.Add(type, applyDefaultBehaviour: true).GetSubtypes();

                for (int i = 0, n = subTypes.Length; i < n; i++)
                {
                    if (visited.Add(type = subTypes[i].DerivedType.Type))
                    {
                        using (var enumerator = Visit(type, typeModel, visited).GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                                yield return enumerator.Current;
                        }

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
                return Enumerable.Empty<ValueMember>();
            }

            metaType = _typeModel.Add(type, applyDefaultBehaviour: true);
            return Visit(metaType);

            static IEnumerable<ValueMember> Visit(MetaType metaType)
            {
                do
                {
                    var fields = metaType.GetFields();
                    for (int i = 0, n = fields.Length; i < n; i++)
                        yield return fields[i];
                }
                while ((metaType = metaType.BaseType) != null);
            }
        }
    }
}

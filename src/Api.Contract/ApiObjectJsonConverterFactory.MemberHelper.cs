using System;
using System.Reflection;
using System.Text.Json;
using ProtoBuf.Meta;

namespace WebApp.Api
{
    internal partial class ApiObjectJsonConverterFactory
    {
        private abstract class MemberHelper<TContainer>
            where TContainer : notnull
        {
            public static MemberHelper<TContainer> Create(ValueMember member) => (MemberHelper<TContainer>)(
                typeof(TContainer).IsValueType ?
                Activator.CreateInstance(typeof(StructMemberHelper<,>).MakeGenericType(typeof(TContainer), member.MemberType), member) :
                Activator.CreateInstance(typeof(ClassMemberHelper<,>).MakeGenericType(typeof(TContainer), member.MemberType), member));

            protected MemberHelper() { }

            public abstract void Read(ref TContainer container, ref Utf8JsonReader reader, JsonSerializerOptions options);
            public abstract void Write(ref TContainer container, string memberName, Utf8JsonWriter writer, JsonSerializerOptions options);
        }

        private abstract class MemberHelper<TContainer, TMember> : MemberHelper<TContainer>
            where TContainer : notnull
        {
            protected MemberHelper() { }

            protected abstract bool IsReadable { get; }
            protected abstract bool IsWritable { get; }

            protected abstract void Get(ref TContainer container, out TMember value);
            protected abstract void Set(ref TContainer container, in TMember value);

            public sealed override void Read(ref TContainer container, ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (IsWritable)
                {
                    var value = JsonSerializer.Deserialize<TMember>(ref reader, options);

                    if (!options.IgnoreNullValues || !(value is null))
                        Set(ref container, in value);
                }
                else
                    reader.Skip();
            }

            public sealed override void Write(ref TContainer container, string memberName, Utf8JsonWriter writer, JsonSerializerOptions options)
            {
                if (IsReadable)
                {
                    Get(ref container, out var value);

                    if (!options.IgnoreNullValues || !(value is null))
                    {
                        writer.WritePropertyName(memberName);
                        JsonSerializer.Serialize(writer, value, options);
                    }
                }
            }
        }

        private sealed class ClassMemberHelper<TContainer, TMember> : MemberHelper<TContainer, TMember>
            where TContainer : class
        {
            private static Func<TContainer, TMember>? BuildGetter(ValueMember member)
            {
                switch (member.Member)
                {
                    case PropertyInfo property:
                        var method = property.GetGetMethod(nonPublic: true);
                        return method != null ? (Func<TContainer, TMember>)Delegate.CreateDelegate(typeof(Func<TContainer, TMember>), method) : null;
                    case FieldInfo field:
                        // we want to avoid run-time code generation in contract assemblies but field getter delegates cannot be created without that,
                        // so we resort to reflection as public fields are uncommon in DTOs anyway
                        return container => (TMember)field.GetValueDirect(__makeref(container));
                    default:
                        throw new ArgumentException(null, nameof(member));
                }
            }

            private static Action<TContainer, TMember>? BuildSetter(ValueMember member)
            {
                switch (member.Member)
                {
                    case PropertyInfo property:
                        var method = property.GetSetMethod(nonPublic: true);
                        return method != null ? (Action<TContainer, TMember>)Delegate.CreateDelegate(typeof(Action<TContainer, TMember>), method) : null;
                    case FieldInfo field:
                        // we want to avoid run-time code generation in contract assemblies but field setter delegates cannot be created without that,
                        // so we resort to reflection as public fields are uncommon in DTOs anyway
                        return !field.IsInitOnly ? (container, value) => field.SetValueDirect(__makeref(container), value) : (Action<TContainer, TMember>?)null;
                    default:
                        throw new ArgumentException(null, nameof(member));
                }
            }

            private readonly Func<TContainer, TMember>? _getter;
            private readonly Action<TContainer, TMember>? _setter;

            public ClassMemberHelper(ValueMember member)
            {
                _getter = BuildGetter(member);
                _setter = BuildSetter(member);
            }

            protected override bool IsReadable => _getter != null;
            protected override bool IsWritable => _setter != null;

            protected override void Get(ref TContainer container, out TMember value) => value = _getter!(container);
            protected override void Set(ref TContainer container, in TMember value) => _setter!(container, value);
        }

        private sealed class StructMemberHelper<TContainer, TMember> : MemberHelper<TContainer, TMember>
            where TContainer : struct
        {
            private delegate TMember Getter(ref TContainer container);

            private delegate void Setter(ref TContainer container, TMember value);

            private static Getter? BuildGetter(ValueMember member)
            {
                switch (member.Member)
                {
                    case PropertyInfo property:
                        var method = property.GetGetMethod(nonPublic: true);
                        return method != null ? (Getter)Delegate.CreateDelegate(typeof(Getter), method) : null;
                    case FieldInfo field:
                        // we want to avoid run-time code generation in contract assemblies but field getter delegates cannot be created without that,
                        // so we resort to reflection as public fields are uncommon in DTOs anyway
                        return (ref TContainer container) => (TMember)field.GetValueDirect(__makeref(container));
                    default:
                        throw new ArgumentException(null, nameof(member));
                }
            }

            private static Setter? BuildSetter(ValueMember member)
            {
                switch (member.Member)
                {
                    case PropertyInfo property:
                        var method = property.GetSetMethod(nonPublic: true);
                        return method != null ? (Setter)Delegate.CreateDelegate(typeof(Setter), method) : null;
                    case FieldInfo field:
                        // we want to avoid run-time code generation in contract assemblies but field setter delegates cannot be created without that,
                        // so we resort to reflection as public fields are uncommon in DTOs anyway
                        return !field.IsInitOnly ? (ref TContainer container, TMember value) => field.SetValueDirect(__makeref(container), value) : (Setter?)null;
                    default:
                        throw new ArgumentException(null, nameof(member));
                }
            }

            private readonly Getter? _getter;
            private readonly Setter? _setter;

            public StructMemberHelper(ValueMember member)
            {
                _getter = BuildGetter(member);
                _setter = BuildSetter(member);
            }

            protected override bool IsReadable => _getter != null;
            protected override bool IsWritable => _setter != null;

            protected override void Get(ref TContainer container, out TMember value) => value = _getter!(ref container);
            protected override void Set(ref TContainer container, in TMember value) => _setter!(ref container, value);
        }
    }
}

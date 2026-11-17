using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Game.Core.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Game.Presentation.Network
{
    // Only data from the engine's assembly uses field contracts. No network-provided CLR type names.
    public sealed class OnlineCoreResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization serialization)
        {
            if (type.Assembly != typeof(PlayerView).Assembly) return base.CreateProperties(type, serialization);
            var properties = new List<JsonProperty>();
            for(var current=type;current!=null&&current!=typeof(object);current=current.BaseType)
                foreach(var field in current.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
                {
                    var property=base.CreateProperty(field,MemberSerialization.Fields);
                    property.PropertyName=current.Name+"."+field.Name;property.Readable=true;property.Writable=true;
                    properties.Add(property);
                }
            return properties;
        }
        protected override JsonObjectContract CreateObjectContract(Type type)
        {
            var contract=base.CreateObjectContract(type);
            if(type.Assembly==typeof(PlayerView).Assembly&&!type.IsAbstract)
                contract.DefaultCreator=()=>FormatterServices.GetUninitializedObject(type);
            return contract;
        }
    }
}

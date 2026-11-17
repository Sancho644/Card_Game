using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.Cards;
using Game.Core.Rules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Game.Presentation.Network
{
    public static class OnlineCodec
    {
        public const int Protocol=1;
        public const int MaxBytes=256*1024;
        private static readonly JsonSerializerSettings Settings=new JsonSerializerSettings {
            ContractResolver=new OnlineCoreResolver(),TypeNameHandling=TypeNameHandling.None,MaxDepth=48,
            MissingMemberHandling=MissingMemberHandling.Error,MetadataPropertyHandling=MetadataPropertyHandling.Ignore
        };
        private static readonly Dictionary<string,Type> Types=typeof(GameCommand).Assembly.GetTypes()
            .Where(t=>!t.IsAbstract&&(typeof(GameCommand).IsAssignableFrom(t)||typeof(GameEvent).IsAssignableFrom(t)))
            .ToDictionary(t=>t.Name);
        public static string Encode(object value)=>JsonConvert.SerializeObject(value,Settings);
        public static T Decode<T>(string json)
        {
            if(json==null||Encoding.UTF8.GetByteCount(json)>MaxBytes)throw new ArgumentException("Packet too large");
            return JsonConvert.DeserializeObject<T>(json,Settings);
        }
        public static OnlineValue Pack(object value)=>new OnlineValue{Kind=value.GetType().Name,Data=JToken.FromObject(value,JsonSerializer.Create(Settings))};
        public static T Unpack<T>(OnlineValue value)
        {
            if(value==null||!Types.TryGetValue(value.Kind,out var type)||!typeof(T).IsAssignableFrom(type))throw new ArgumentException("Unknown wire type");
            return (T)value.Data.ToObject(type,JsonSerializer.Create(Settings));
        }
        public static string Fingerprint(CardCatalog catalog,GameRules rules,string build)
        {
            // Definitions contain effects of different concrete types, so include each concrete serialized definition.
            var data=Protocol+"/"+build+"/"+Encode(rules)+"/"+string.Join("/",catalog.All.OrderBy(c=>c.Id).Select(c=>c.GetType().Name+Encode(c)));
            using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-","");
        }
    }
}

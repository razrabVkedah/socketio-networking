using System;
using System.Collections.Generic;
using ClientSocketIO.NetworkData.NetworkRpc;
using ClientSocketIO.NetworkData.NetworkVariables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClientSocketIO.Types.NetworkUpdate.Json
{
    public class BaseDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(BaseNetworkData).IsAssignableFrom(objectType) || objectType == typeof(List<BaseNetworkData>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var jsonArray = JArray.Load(reader);
                var baseDataList = new List<BaseNetworkData>();

                foreach (var item in jsonArray)
                {
                    var baseData = ConvertTokenToBaseData(item, serializer);
                    baseDataList.Add(baseData);
                }

                return baseDataList;
            }

            var jsonObject = JObject.Load(reader);
            return ConvertTokenToBaseData(jsonObject, serializer);
        }

        private static BaseNetworkData ConvertTokenToBaseData(JToken token, JsonSerializer serializer)
        {
            var type = (NetworkDataType)(token["dataType"].Value<int>());

            BaseNetworkData baseData = type switch
            {
                NetworkDataType.Rpc => new NetworkRpcData(),
                NetworkDataType.Variable => new NetworkVariableData(),
                NetworkDataType.Spawn => new SpawnData(),
                NetworkDataType.Destroy => new DestroyData(),
                NetworkDataType.Transform => new TransformData(),
                NetworkDataType.Rigidbody => new RigidbodyData(),
                NetworkDataType.Rigidbody2D => new Rigidbody2dData(),
                NetworkDataType.Animator => new AnimatorData(),
                NetworkDataType.AudioSource => new AudioSourceData(),
                _ => throw new Exception("Unknown type")
            };

            serializer.Populate(token.CreateReader(), baseData);
            return baseData;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
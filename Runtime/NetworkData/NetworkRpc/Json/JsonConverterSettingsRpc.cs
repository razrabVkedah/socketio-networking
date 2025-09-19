using Newtonsoft.Json;

namespace ClientSocketIO.NetworkData.NetworkRpc.Json
{
    public static class JsonConverterSettingsRpc
    {
        static JsonConverterSettingsRpc()
        {
            serializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            serializerSettings.Converters.Add(new Vector2Converter());
            serializerSettings.Converters.Add(new Vector3Converter());
            serializerSettings.Converters.Add(new ColorConverter());
            serializerSettings.Converters.Add(new QuaternionConverter());
        }

        public static JsonSerializerSettings serializerSettings { get; }
    }
}
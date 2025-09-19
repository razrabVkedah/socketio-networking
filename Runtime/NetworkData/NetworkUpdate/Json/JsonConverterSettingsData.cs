using Newtonsoft.Json;

namespace ClientSocketIO.Types.NetworkUpdate.Json
{
    public static class JsonConverterSettingsData
    {
        static JsonConverterSettingsData()
        {
            serializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            serializerSettings.Converters.Add(new BaseDataConverter());
        }

        public static JsonSerializerSettings serializerSettings { get; }
    }
}
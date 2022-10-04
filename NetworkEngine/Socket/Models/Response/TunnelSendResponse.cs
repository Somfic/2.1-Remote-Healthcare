using Newtonsoft.Json;

namespace RemoteHealthcare.NetworkEngine.Socket.Models.Response;

public class TunnelSendResponse : IDataResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("data")] public TunnelData Data { get; set; }


    public class TunnelData
    {
        [JsonProperty("data")] public ChildrenData Data { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("serial")] public string Serial { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        public class ChildrenData
        {
            [JsonProperty("data")] public Child[] Data { get; set; }
            [JsonProperty("children")] public Child[] Children { get; set; }

            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("uuid")] public string Uuid { get; set; }
        }

        public class Child
        {
            [JsonProperty("components")] public Component[] Components { get; set; }

            [JsonProperty("children")] public Child[] Children { get; set; }
            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("uuid")] public string Uuid { get; set; }

            public class Component
            {
                [JsonProperty("position")] public double[] Position { get; set; }

                [JsonProperty("rotation")] public double[] Rotation { get; set; }

                [JsonProperty("scale")] public long[] Scale { get; set; }

                [JsonProperty("type")] public string Type { get; set; }

                [JsonProperty("baking")] public string Baking { get; set; }

                [JsonProperty("color")] public double[] Color { get; set; }

                [JsonProperty("cutoff")] public long? Cutoff { get; set; }

                [JsonProperty("directionalAmbient")] public double? DirectionalAmbient { get; set; }

                [JsonProperty("intensity")] public long? Intensity { get; set; }

                [JsonProperty("lighttype")] public string Lighttype { get; set; }

                [JsonProperty("range")] public long? Range { get; set; }

                [JsonProperty("shadow")] public string Shadow { get; set; }

                [JsonProperty("spotlightAngle")] public long? SpotlightAngle { get; set; }

                [JsonProperty("useFbo")] public bool? UseFbo { get; set; }

                [JsonProperty("light")] public string? Light { get; set; }

                [JsonProperty("timeOfDay")] public long? TimeOfDay { get; set; }

                [JsonProperty("castShadow")] public bool? CastShadow { get; set; }

                [JsonProperty("cullBackFaces")] public bool? CullBackFaces { get; set; }

                [JsonProperty("file")] public string File { get; set; }

                [JsonProperty("materialoverrides")] public object[] MaterialOverrides { get; set; }

                [JsonProperty("attach")] public string Attach { get; set; }

                [JsonProperty("mesh")] public string? Mesh { get; set; }
            }
        }
    }
}
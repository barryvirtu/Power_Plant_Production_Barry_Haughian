
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Power_Plant_Production_Classes
{



    public class PowerPlantAPIRequest
    {
        [JsonProperty("load")]
        [JsonPropertyName("load")]
        public int Load { get; set; }

        [JsonPropertyName("fuels")]
        [JsonProperty("fuels")]
        public Fuel? Fuels { get; set; }

        [JsonPropertyName("powerPlants")]
        [JsonProperty("powerPlants")]
        public required IEnumerable<PowerPlant> PowerPlants { get; set; }
    }
    public class PowerPlantResponse
    {
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonPropertyName("power")]
        [JsonProperty("power")]
        public double Power { get; set; } 
    }


    public class Fuel
    {
        [JsonPropertyName("gas")]
        [JsonProperty("gas")]
        public float Gas { get; set; }

        [JsonPropertyName("kerosine")]
        [JsonProperty("kerosine")]
        public float Kerosine { get; set; }
      
        [JsonPropertyName("Co2")]
        [JsonProperty("co2")]
        public float Co2 { get; set; }

        [JsonPropertyName("wind")]
        [JsonProperty("wind")]
        public float Wind { get; set; }
     }

    public class PowerPlant
    {
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        [JsonProperty("type")]
        public PlantType Type { get; set; }

        [JsonPropertyName("efficiency")]
        [JsonProperty("efficiency")]
        public float Efficiency { get; set; }

        [JsonPropertyName("pmin")]
        [JsonProperty("pMin")]
        public int PMin { get; set; }

        [JsonPropertyName("pmax")]
        [JsonProperty("pMax")]
        public int PMax { get; set; }
    }

    public class PowerPlantSelector
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonProperty("pMin")]
        [JsonPropertyName("pMin")]
        public int PMin { get; set; }
        [JsonProperty("pMax")]
        [JsonPropertyName("pMax")]
        public int PMax { get; set; }
        [JsonProperty("priceMWh")]
        [JsonPropertyName("priceMWh")]
        public float PriceMWh { get; set; }
        [JsonProperty("pMaxToDeliver")]
        [JsonPropertyName("pMaxToDeliver")]
        public float PMaxToDeliver { get; set; }
    }

    public static class Load
    {
        public static IEnumerable<PowerPlantResponse> Calculate(PowerPlantAPIRequest request)
        {
            List<PowerPlantSelector> plants = request.PowerPlants.Select(plant =>
            {

                return new PowerPlantSelector()
                {
                    Name = plant.Name,
                    PriceMWh = plant.Type.Equals(PlantType.Gasfired) ? (request.Fuels.Gas / plant.Efficiency) :
                                plant.Type.Equals(PlantType.Turbojet) ? (request.Fuels.Kerosine / plant.Efficiency) : 0,
                    PMaxToDeliver = plant.Type.Equals(PlantType.Windturbine) ? ((request.Fuels.Wind / 100) * plant.PMax) : plant.PMax,
                    PMax = plant.PMax,
                    PMin = plant.PMin
                };
            }).ToList();

            float TotalPower = 0;

            return plants
                .OrderBy(x => x.PMaxToDeliver == 0)
                .ThenBy(x => x.PriceMWh)
                .ThenByDescending(y => y.PMax)
                .Select(z => {
                    z.PMaxToDeliver = (TotalPower <= request.Load ?
                    (z.PMaxToDeliver + TotalPower) >= request.Load ?
                     (request.Load - TotalPower) : z.PMaxToDeliver : 0);
                    TotalPower += z.PMaxToDeliver;
                    return new PowerPlantResponse()
                    {
                        Name = z.Name,
                        Power = (Math.Truncate(z.PMaxToDeliver * 10) / 10)
                    };
                });
        }
    }




    public enum PlantType
    {
        [EnumMember(Value = "gasfired")]
        Gasfired,
        [EnumMember(Value = "turbojet")]
        Turbojet,
        [EnumMember(Value = "windturbine")]
        Windturbine
    }
}
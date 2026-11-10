
namespace WeatherApp.Models;

public class FavoriteCity
{
    public int Id { get; set; }
    public string CityName { get; set; } = "";
    public string Country { get; set; } = "";
    public double? LastTemperature { get; set; }
    public double? PreviousTemperature { get; set; }
    public string LastCheckedAt { get; set; } = "";
    public string PreviousCheckedAt { get; set; } = "";

    public string LastTemperatureText
    {
        get
        {
            if (LastTemperature.HasValue)
                return $"{LastTemperature.Value:0.#} °C";
            return "Fara date";
        }
    }

    public string PreviousTemperatureText
    {
        get
        {
            if (PreviousTemperature.HasValue)
                return $"{PreviousTemperature.Value:0.#} °C";
            return "Fara date";
        }
    }

    public string LastCheckedText
    {
        get
        {
            if (LastCheckedAt == "")
                return "Nu exista verificari.";
            return "Ultima verificare: " + LastCheckedAt;
        }
    }

    public string PreviousCheckedText
    {
        get
        {
            if (PreviousCheckedAt == "")
                return "";
            return "Verificare anterioara: " + PreviousCheckedAt;
        }
    }
    public string TrendText
    {
        get
        {
            if (!LastTemperature.HasValue)
                return "Apasa Actualizeaza pentru prima verificare.";
            if (!PreviousTemperature.HasValue)
                return "Prima verificare. Nu exista date anterioare.";
            double difference = LastTemperature.Value - PreviousTemperature.Value;
            if (difference > 0.5)
                return $"Temperatura a crescut cu {difference:0.#} °C";
            if (difference < -0.5)
                return $"Temperatura a scazut cu {Math.Abs(difference):0.#} °C";
            return "Temperatura a ramas aproape constanta.";


        }
    }
}

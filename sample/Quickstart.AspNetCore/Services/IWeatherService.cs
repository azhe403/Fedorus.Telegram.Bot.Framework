using System.Threading.Tasks;

namespace Quickstart.AspNetCore.Services
{
    interface IWeatherService
    {
        Task<CurrentWeather> GetWeatherAsync(double lat, double lon);
    }
}
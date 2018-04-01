using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnchantedMirror.Extensions;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace EnchantedMirror.Modules
{
    public sealed partial class CurrentWeather : UserControl
    {
        private string _apiKey;
        private Uri _currentUri;
        private string _units;
        private string _location;

        private DispatcherTimer _weatherTimer = new DispatcherTimer();

        public CurrentWeather()
        {
            this.InitializeComponent();
            App.ConfigurationLoaded += App_ConfigurationLoaded;

            LoadConfig().GetAwaiter().GetResult();

            SetUnitGlyph();

            LoadCurrentWeather();

            _weatherTimer.Interval = TimeSpan.FromMinutes(30);
            _weatherTimer.Tick += WeatherFeedTimer_Tick;
            _weatherTimer.Start();
        }

        private void SetUnitGlyph()
        {
            if (_units.Equals("metric", StringComparison.OrdinalIgnoreCase))
            {
                celcius.Glyph = char.ConvertFromUtf32((int)WeatherIcon.Celsius);
            }
            else
            {
                celcius.Glyph = char.ConvertFromUtf32((int)WeatherIcon.Fahrenheit);
            }
        }

        private void App_ConfigurationLoaded(object sender, EventArgs e)
        {
            LoadConfig().GetAwaiter().GetResult();
        }

        private async Task LoadConfig()
        {
            if (App.Configurations == null)
            {
                return;
            }

            dynamic config = App.Configurations
                .FirstOrDefault(c => c.module == "CurrentWeather");

            if (config != null)
            {
                SetAlignments(config);
                SetMargin(config);

                _apiKey = (string)config.attributes.openWeatherMap.key;
                if (_apiKey.Equals("yourkeyhere", StringComparison.OrdinalIgnoreCase))
                {
                    _apiKey = await LoadApiKeyAsync();
                }

                _location = (string)config.attributes.openWeatherMap.locationId;
                _units = (string)config.attributes.openWeatherMap.units;

                _currentUri = new Uri((string)config.attributes.openWeatherMap.uri
                    + $"/?id={_location}&units={_units}&APPID={_apiKey}");
            }
        }

        private void SetAlignments(dynamic config)
        {
            string vert = Convert.ToString(config.position.verticalAlignment);
            string hori = Convert.ToString(config.position.horizontalAlignment);
            VerticalAlignment = vert.ToVerticalAlignment();
            HorizontalAlignment = hori.ToHorizontalAlignment();
        }

        private void SetMargin(dynamic config)
        {
            Thickness margin = Margin;
            margin.Left = (double)config.position.margin.left;
            margin.Top = (double)config.position.margin.top;
            margin.Right = (double)config.position.margin.right;
            margin.Bottom = (double)config.position.margin.bottom;
            Margin = margin;
        }

        private async Task<string> LoadApiKeyAsync()
        {
            var files = Directory.GetFiles("./Modules/CurrentWeather/", "*.key");
            if (files.Count() == 1)
            {
                return await File.ReadAllTextAsync(files[0]);
            }

            return string.Empty;
        }

        public enum WeatherIcon
        {
            Celsius = 0xf03c,
            Fahrenheit = 0xf045,
            Sunrise = 0xf051,
            Sunset = 0xf052,
            SunnyDay = 0xf00d,
            Humidity = 0xf07a,
            MaxTemp = 0xf055,
            MinTemp = 0xf053,
            Barometer = 0xf079,
            CloudyDay = 0xf00d,
            Cloudy = 0xf013,
            CloudyWindy = 0xf012,
            Showers = 0xf01a,
            Rain = 0xf019,
            Thunderstorm = 0xf01e,
            Snow = 0xf01b,
            Fog = 0xf014,
            ClearNight = 0xf02e,
            CloudyNight = 0xf031,
            ShowersNight = 0xf037,
            RainNight = 0xf036,
            ThunderstormNight = 0xf02d,
            SnowNight = 0xf038,
            CloudyWindyNight = 0xf023,
            Alien = 0xf075,
        }

        public async void LoadCurrentWeather()
        {
            string weather;

            var httpClient = new HttpClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync(_currentUri);
            weather = await httpResponse.Content.ReadAsStringAsync();

            CurrentWeatherFeed result = JsonConvert.DeserializeObject<CurrentWeatherFeed>(weather);
            temp.Text = Math.Round(result.main.temp).ToString();
            weatherIcon.Glyph = GetWeatherIcon(result.weather[0].icon);

            // TODO: min and max temps
            var minTemp = Math.Round(result.main.temp_min).ToString();
            var maxTemp = Math.Round(result.main.temp_max).ToString();

            // TODO: humidity %
            var humidity = result.main.humidity.ToString();

            // TODO: pressure
            var pressure = result.main.pressure.ToString();

            // Speed is in meter/second
            // Direction is where the wind is coming from
            windNotes.Text = GetWindDescription(result.wind.speed, result.wind.deg);
            windRotation.Angle = Math.Round(result.wind.deg + 180);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var info = textInfo.ToLower(result.weather[0].description);
            info = textInfo.ToUpper(info[0]) + info.Substring(1);
            summary.Text = info
                + " and "
                + textInfo.ToLower(MpsToBeaufotDescription(result.wind.speed));

            // Use speed and direction to convert to words,
            // for example light winds from the north east
            // or          moderate winds form the south
            var windOutput = string.Empty;

            // Sunrise and Sunset times
            sunrise.Text = FromTimeStamp(result.sys.sunrise)
                .ToString("HH:mm", CultureInfo.InvariantCulture);
            sunset.Text = FromTimeStamp(result.sys.sunset)
                .ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        private string GetWindDescription(float speed, float deg)
        {
            string direction = DegreeToCardinal(deg);
            string windSpeed = MpsToBeaufotDescription(speed);

            switch (windSpeed)
            {
                case "Calm":
                case "Light air":
                    return windSpeed;

                default:
                    return $"{windSpeed} from the {direction}";
            }
        }

        private DateTime FromTimeStamp(int timespan)
            => new DateTime(1970, 1, 1).AddSeconds(timespan).ToLocalTime();

        private string GetWeatherIcon(string iconRequired)
        {
            WeatherIcon icon = WeatherIcon.SunnyDay;

            switch (iconRequired)
            {
                case "01d":
                    icon = WeatherIcon.SunnyDay;
                    break;

                case "02d":
                    icon = WeatherIcon.CloudyDay;
                    break;

                case "03d":
                    icon = WeatherIcon.Cloudy;
                    break;

                case "04d":
                    icon = WeatherIcon.CloudyWindy;
                    break;

                case "09d":
                    icon = WeatherIcon.Showers;
                    break;

                case "10d":
                    icon = WeatherIcon.Rain;
                    break;

                case "11d":
                    icon = WeatherIcon.Thunderstorm;
                    break;

                case "13d":
                    icon = WeatherIcon.Snow;
                    break;

                case "50d":
                    icon = WeatherIcon.Fog;
                    break;

                case "01n":
                    icon = WeatherIcon.CloudyNight;
                    break;

                case "02n":
                    icon = WeatherIcon.CloudyNight;
                    break;

                case "03n":
                    icon = WeatherIcon.CloudyNight;
                    break;

                case "04n":
                    icon = WeatherIcon.CloudyNight;
                    break;

                case "09n":
                    icon = WeatherIcon.ShowersNight;
                    break;

                case "10n":
                    icon = WeatherIcon.RainNight;
                    break;

                case "11n":
                    icon = WeatherIcon.ThunderstormNight;
                    break;

                case "13n":
                    icon = WeatherIcon.SnowNight;
                    break;

                case "50n":
                    icon = WeatherIcon.CloudyWindyNight;
                    break;

                default:
                    icon = WeatherIcon.Alien;
                    break;
            }

            return char.ConvertFromUtf32((int)icon);
        }

        private string MpsToBeaufotDescription(float speed)
        {
            if (speed < 0.3)
            {
                return "Calm";
            }

            if (speed <= 1.5)
            {
                return "Light air";
            }

            if (speed <= 3.3)
            {
                return "Light breeze";
            }

            if (speed <= 5.5)
            {
                return "Gentle breeze";
            }

            if (speed <= 7.9)
            {
                return "Moderate breeze";
            }

            if (speed <= 10.7)
            {
                return "Fresh breeze";
            }

            if (speed <= 13.8)
            {
                return "Strong breeze";
            }

            if (speed <= 17.1)
            {
                return "Near gale";
            }

            if (speed <= 20.7)
            {
                return "Gale";
            }

            if (speed <= 24.4)
            {
                return "Severe gale";
            }

            if (speed <= 28.4)
            {
                return "Storm";
            }

            if (speed <= 32.6)
            {
                return "Violent storm";
            }

            return "Hurricane";
        }

        private string DegreeToCardinal(float deg)
        {
            if (deg > 11.25 && deg <= 33.75)
            {
                return "north north east";
            }

            if (deg <= 56.25)
            {
                return "north east";
            }

            if (deg <= 78.75)
            {
                return "east north east";
            }

            if (deg <= 101.25)
            {
                return "east";
            }

            if (deg <= 123.75)
            {
                return "east south east";
            }

            if (deg <= 146.25)
            {
                return "south east";
            }

            if (deg <= 168.75)
            {
                return "south south east";
            }

            if (deg <= 191.25)
            {
                return "south";
            }

            if (deg <= 213.75)
            {
                return "south south west";
            }

            if (deg <= 236.25)
            {
                return "south west";
            }

            if (deg <= 258.75)
            {
                return "west south west";
            }

            if (deg <= 281.25)
            {
                return "west";
            }

            if (deg <= 303.75)
            {
                return "west north west";
            }

            if (deg <= 326.25)
            {
                return "north west";
            }

            if (deg <= 348.75)
            {
                return "north north west";
            }

            return "north";
        }

        private void WeatherFeedTimer_Tick(object sender, object e)
        {
            _weatherTimer.Stop();
            LoadCurrentWeather();
            _weatherTimer.Start();
        }

        public class Clouds
        {
            public int all { get; set; }
        }

        public class Coord
        {
            public float lat { get; set; }

            public float lon { get; set; }
        }

        public class CurrentWeatherFeed
        {
            public string _base { get; set; }

            public Clouds clouds { get; set; }

            public int cod { get; set; }

            public Coord coord { get; set; }

            public int dt { get; set; }

            public int id { get; set; }

            public Main main { get; set; }

            public string name { get; set; }

            public Sys sys { get; set; }

            public int visibility { get; set; }

            public Weather[] weather { get; set; }

            public Wind wind { get; set; }
        }

        public class Main
        {
            public int humidity { get; set; }

            public int pressure { get; set; }

            public float temp { get; set; }

            public float temp_max { get; set; }

            public float temp_min { get; set; }
        }

        public class Sys
        {
            public string country { get; set; }

            public int id { get; set; }

            public float message { get; set; }

            public int sunrise { get; set; }

            public int sunset { get; set; }

            public int type { get; set; }
        }

        public class Weather
        {
            public string description { get; set; }

            public string icon { get; set; }

            public int id { get; set; }

            public string main { get; set; }
        }

        public class Wind
        {
            public float deg { get; set; }

            public float speed { get; set; }
        }
    }
}

using Newtonsoft.Json.Linq;
using System.Globalization;

string locServUrl = "https://ipinfo.io/json";

Console.Title = "Today's Info";
Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.Green;

while(true){

    Console.Clear();

    DateTime now = DateTime.Now;

    var location = await FetchAndPrintLocationInfo(locServUrl);

    if( location != null ){

        double longitude = location.Longitude;
        double latitude = location.Latitude;
        string city = location.City;
        string country = location.Country;

        var weatherView = await FetchAndPrintWeatherInfo(latitude,longitude);

        if( weatherView != null ){

            List<string> lines = new List<string>
            {
                $"Sıcaklık: {weatherView.Temperature} *C",
                $"Rüzgar: {weatherView.Wind} m/s",
                $"Şehir: {city}",
                $"Ülke: {country}",
                "Tarih: " + now.ToShortDateString(),
                "Saat: " + now.ToShortTimeString()
            };

            int maxLength = lines.Max( line => line.Length );

            int maxWith = lines.Max( line => line.Length );

            Console.WindowWidth = Math.Min( maxWith + 20, Console.LargestWindowWidth );

            Console.WindowHeight = Math.Min( lines.Count +10, Console.LargestWindowHeight );

            string seperator = new string('-',maxLength);

            Console.WriteLine(seperator);

            foreach( var line in lines ){

                Console.WriteLine(line);
            }
 
            Console.WriteLine(seperator);

        }else{

            Console.WriteLine("Bir Sorun var, cevap null geldi: "+ weatherView);
        }

    }else{

        Console.Clear();

        Console.WriteLine("Bilgi Alınamadı Bağlantı Hatası!");

    }

    Thread.Sleep(60000);

}

static async Task<WeatherView> FetchAndPrintWeatherInfo( double latitude,double longitude )
{
    string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid=bda191da6dc316eff1c2544881ebca63&units=metric";

    using ( HttpClient client = new HttpClient() ){

        try{

            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            JObject weatherData = JObject.Parse(json);

            double temp = (double)weatherData["main"]["temp"];
            double windSpeed = (double)weatherData["wind"]["speed"];

            return new WeatherView{

                Temperature = temp,
                Wind = windSpeed

            };

        }catch( Exception ex ){

            Console.WriteLine("Bağlantı Hatası!",ex);

            return null;
        }

    }
}

static async Task<LocationInfo> FetchAndPrintLocationInfo(string url)
{

    using ( HttpClient client = new HttpClient() ){

        try{

            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            JObject ipInfo = JObject.Parse(json);

            string city = (string)ipInfo["city"];
            string country = (string)ipInfo["country"];
            string coords = (string)ipInfo["loc"];
            string[] coordsArr = coords.Split(',');
            double latitude = double.Parse(coordsArr[0], CultureInfo.InvariantCulture);
            double longitude = double.Parse(coordsArr[1], CultureInfo.InvariantCulture);

            return new LocationInfo{
                City = city,
                Country = country,
                Latitude = latitude,
                Longitude = longitude
            };

        }catch( Exception ex ){

            Console.WriteLine("Konum: ", ex.Message);
            return null;
        }

    }

}

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using MarsRoverPhotos_API.Manifest;
using MarsRoverPhotos_API.Photos;

namespace MarsRoverPhotos_API
{
    public class Support
    {
        static string Endpoint = "https://api.nasa.gov/mars-photos/api/v1/";
        static string APIKey = "PDeo30MFStY8ywaVRvaeC7RAanbh7GyXjbOv1Oqt";
        static string[] Rovers = new string[] { "curiosity", "opportunity", "spirit" };
        //Path could be tweaked as desired, modified default, user input, etc
        public static string StoragePath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;

        public static void GetMarsRoverPhotos(DateTime date)
        {
            Console.WriteLine($"Checking for valid rover for date {date.ToString("yyyy-M-d")}");
            string rover = getRoverForDate(date);
            Console.WriteLine($"Rover {rover} found");
            using (var client = new HttpClient())
            {
                Console.WriteLine($"Getting image list for {rover} on {date.ToString("yyyy-M-d")}");
                client.BaseAddress = new Uri(Endpoint);
                var responseTask = client.GetAsync($"rovers/{rover}/photos?api_key={APIKey}&earth_date={date.ToString("yyyy-M-d")}");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();                    

                    MarsRoverPhotosResponse responseData = JsonConvert.DeserializeObject<MarsRoverPhotosResponse>(readTask.Result);

                    if (!Directory.Exists($@"{StoragePath}\StoredMarsRoverPhotos\{date.ToString("yyyy-M-d")}"))
                        Directory.CreateDirectory($@"{StoragePath}\StoredMarsRoverPhotos\{date.ToString("yyyy-M-d")}");

                    Console.WriteLine("Image list pulled, pulling images to save locally");

                    foreach (var photoObj in responseData.photos)
                    {                        
                        saveFileFromURL(new Uri(photoObj.img_src), date);
                    }
                }
                else
                    LogError($"Failed to recieve http success during MarsRoverPhotos call. Code: {result.StatusCode}");
            }
        }

        public static void saveFileFromURL(Uri url, DateTime date)
        {
            string filePath = $@"{StoragePath}\StoredMarsRoverPhotos\{date.ToString("yyyy-M-d")}\{Path.GetFileName(url.LocalPath)}";

            using (HttpClient client = new HttpClient())
            {
                var res = client.GetAsync(url);
                res.Wait();

                var result = res.Result;
                if (result.IsSuccessStatusCode)
                    using (HttpContent content = result.Content)
                    {
                        var bytesTask = content.ReadAsByteArrayAsync();
                        bytesTask.Wait();
                        File.WriteAllBytes(filePath, bytesTask.Result);
                    }
                else
                    LogError($"Failed to recieve http success during file pull. Code: {result.StatusCode}");
            }
        }

        public static string getRoverForDate(DateTime date)
        {
            foreach (var rover in Rovers)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Endpoint);
                    var responseTask = client.GetAsync($"manifests/{rover}?api_key={APIKey}");
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        MarsRoverManifestResponse responseData = JsonConvert.DeserializeObject<MarsRoverManifestResponse>(readTask.Result);

                        DateTime landingDate;
                        DateTime maxDate;

                        if (DateTime.TryParse(responseData.photo_manifest.landing_date, out landingDate) && DateTime.TryParse(responseData.photo_manifest.max_date, out maxDate))
                            if (landingDate < date && maxDate >= date)
                                return rover;

                        //if (!Directory.Exists($@"{StoragePath}\StoredMarsRoverManifest\{date.ToString("yyyy-M-d")}"))
                        //    Directory.CreateDirectory($@"{StoragePath}\StoredMarsRoverManifest\{date.ToString("yyyy-M-d")}");
                    }
                    else
                        LogError($"Failed to recieve http success during MarsRoverManifest call. Code: {result.StatusCode}");
                }
            }
            //default
            return "curiosity";
        }

        public static List<DateTime> getDatesFromFile(string datesFilePath)
        {
            //return new List<DateTime> { new DateTime(2016, 7, 13) }; 
            string[] dateStrings = File.ReadAllLines(datesFilePath);
            List<DateTime> dates = new List<DateTime>();
            foreach (var dateString in dateStrings)
            {
                DateTime date = new DateTime();
                if (DateTime.TryParse(dateString, out date))
                    dates.Add(date);
                else
                    LogError($"Invalid date in dates.txt, {dateString}");
            }
            return dates;
        }

        public static void LogError(string message, Exception ex = null)
        {
            var filePath = StoragePath + @$"\Errors";

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            using (StreamWriter sw = File.AppendText(filePath + $@"\{DateTime.Now.ToString("yyyy-M-d")}.txt"))
            {
                sw.WriteLine(DateTime.Now.ToString() + ": " + message);
            }

            Console.WriteLine(message);
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
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
        public static List<ManifestData> manifests = new List<ManifestData>();
        static ReaderWriterLock locker = new ReaderWriterLock();
        public static int imageCount = 0;

        public static async Task GetMarsRoverPhotos(DateTime date)
        {
            LogEvent($"Checking for valid rover for date {date.ToString("yyyy-M-d")}");
            string rover = getRoverForDate(date);
            LogEvent($"Rover {rover} found for {date.ToString("yyyy-M-d")}");

            try
            {
                using (var client = new HttpClient())
                {
                    LogEvent($"Getting image list for {rover} on {date.ToString("yyyy-M-d")}");
                    client.BaseAddress = new Uri(Endpoint);
                    var response = await client.GetAsync($"rovers/{rover}/photos?api_key={APIKey}&earth_date={date.ToString("yyyy-M-d")}");

                    if (response.IsSuccessStatusCode)
                    {
                        var read = await response.Content.ReadAsStringAsync();

                        MarsRoverPhotosResponse responseData = JsonConvert.DeserializeObject<MarsRoverPhotosResponse>(read);

                        if (!Directory.Exists($@"{StoragePath}\StoredMarsRoverPhotos\{date.ToString("yyyy-M-d")}"))
                            Directory.CreateDirectory($@"{StoragePath}\StoredMarsRoverPhotos\{date.ToString("yyyy-M-d")}");

                        LogEvent($"Image list pulled for {rover} on {date.ToString("yyyy-M-d")}, pulling images to store locally");

                        List<Task> tasks = new List<Task>();

                        imageCount += responseData.photos.Count;

                        foreach (var photoObj in responseData.photos)
                        {
                            tasks.Add(saveFileFromURL(new Uri(photoObj.img_src), date));
                        }

                        Task.WaitAll(tasks.ToArray());
                    }
                    else
                        LogEvent($"Failed to recieve http success during MarsRoverPhotos call. Code: {response.StatusCode}", LogType.Error);
                }
            }
            catch (Exception ex)
            {
                LogEvent(ex.Message, LogType.Error, ex);
            }
        }

        public static async Task saveFileFromURL(Uri url, DateTime date)
        {
            string filePath = $@"{StoragePath}\StoredMarsRoverPhotos\{date.ToString("yyyy-M-d")}\{Path.GetFileName(url.LocalPath)}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var result = await client.GetAsync(url);

                    if (result.IsSuccessStatusCode)
                        using (HttpContent content = result.Content)
                        {
                            var bytesTask = await content.ReadAsByteArrayAsync();
                            File.WriteAllBytes(filePath, bytesTask);
                        }
                    else
                        LogEvent($"Failed to recieve http success during file pull. Code: {result.StatusCode}", LogType.Error);
                }
            }
            catch (Exception ex)
            {
                LogEvent(ex.Message, LogType.Error, ex);
            }
        }

        public static string getRoverForDate(DateTime date)
        {
            //I don't really like all the manifests/manifest variable names here, but we'll leave it as is for now
            var filePath = StoragePath + @"\manifest.txt";
            if (!File.Exists(filePath))
            {
                foreach (var rover in Rovers)
                {
                    try
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
                                    manifests.Add(new ManifestData { roverName = rover, LandingDate = landingDate, MaxDate = maxDate });
                            }
                            else
                                LogEvent($"Failed to recieve http success during MarsRoverManifest call. Code: {result.StatusCode}", LogType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogEvent(ex.Message, LogType.Error, ex);
                    }
                }                

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    foreach(var manifest in manifests)
                        sw.WriteLine($"{manifest.roverName}|{manifest.LandingDate}|{manifest.MaxDate}");                    
                }
            }
            else if (manifests.Count == 0)
            {
                string[] ManifestData = File.ReadAllLines(Support.StoragePath + @"\manifest.txt");
                foreach (var manifest in ManifestData)
                {
                    DateTime landingDate;
                    DateTime maxDate;
                    var split = manifest.Split('|');
                    if (split.Length < 3)
                        LogEvent($"Invalid record in manifest.txt, {manifest}", LogType.Error);
                    if (DateTime.TryParse(split[1], out landingDate) && DateTime.TryParse(split[2], out maxDate))
                        manifests.Add(new ManifestData { roverName = split[0], LandingDate = landingDate, MaxDate = maxDate });
                }
            }

            foreach (var manifest in manifests)
                if (manifest.LandingDate < date && manifest.MaxDate >= date)
                    return manifest.roverName;
            //default
            return "curiosity";
        }

        public static List<DateTime> getDatesFromFile(string datesFilePath)
        {
            string[] dateStrings = File.ReadAllLines(datesFilePath);
            List<DateTime> dates = new List<DateTime>();
            foreach (var dateString in dateStrings)
            {
                DateTime date = new DateTime();
                if (DateTime.TryParse(dateString, out date))
                    dates.Add(date);
                else
                    LogEvent($"Invalid date in dates.txt, {dateString}", LogType.Error);
            }
            return dates;
        }

        public static void LogEvent(string message, LogType type = LogType.Log, Exception ex = null)
        {
            var filePath = StoragePath + @$"\Logs";

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            locker.AcquireWriterLock(5000);

            try
            {
                using (StreamWriter sw = File.AppendText(filePath + $@"\{type}_{DateTime.Now.ToString("yyyy-M-d")}.txt"))
                {
                    sw.WriteLine($"{DateTime.Now.ToString()} : {message} {ex?.InnerException} {ex?.StackTrace}");
                }
            }
            finally
            {
                locker.ReleaseWriterLock();
            }

            Console.WriteLine(message);
        }

        public enum LogType
        {
            Log,
            Error
        }
    }
}

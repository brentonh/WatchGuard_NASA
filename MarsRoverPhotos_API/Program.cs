using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using MarsRoverPhotos_API;

namespace MarsRoverPhotos_API
{
    //Example https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date=2015-6-3&api_key=DEMO_KEY
    class Program
    {
        static void Main(string[] args)
        {
            //Pulls all images for all valid dates specified in dates.txt sln root directory
            //Images stored in a folder per date within StoredMarsRoverPhotos folder in sln root directory
            //Error logging stored in Errors folder in sln root

            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<Task> tasks = new List<Task>();

            foreach (var date in Support.getDatesFromFile(Support.StoragePath + @"\dates.txt"))
            {
                tasks.Add(Support.GetMarsRoverPhotos(date));
            }
            Task.WaitAll(tasks.ToArray());

            sw.Stop();
            Support.LogEvent($"Completed storage of {Support.imageCount} images in {sw.Elapsed.ToString()}");
        }
    }
}

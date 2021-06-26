using System;
using System.Collections.Generic;

namespace MarsRoverPhotos_API.Manifest
{
    public class MarsRoverManifestResponse
    {
        public PhotoManifest photo_manifest { get; set; }
    }
    public class Photo
    {
        public int sol { get; set; }
        public string earth_date { get; set; }
        public int total_photos { get; set; }
        public List<string> cameras { get; set; }
    }

    public class PhotoManifest
    {
        public string name { get; set; }
        public string landing_date { get; set; }
        public string launch_date { get; set; }
        public string status { get; set; }
        public int max_sol { get; set; }
        public string max_date { get; set; }
        public int total_photos { get; set; }
        public List<Photo> photos { get; set; }
    }

    public class ManifestData
    {
        public string roverName { get; set; }
        public DateTime LandingDate { get; set; }
        public DateTime MaxDate { get; set; }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MarsRoverPhotos_API.Tests
{
    public class MarsRoverPhotosTests
    {
        DateTime testDate = new DateTime(2015, 6, 3); //date from nasa api doc example query

        [Fact]
        public void getDatesFromFileTest() //fails if no dates, no valid dates, or no file
        {
            List<DateTime> dates = Support.getDatesFromFile(Support.StoragePath + @"\dates.txt");
            Assert.True(dates.Count > 0);
        }

        [Fact]
        public void getRoverForDateTest()
        {            
            Assert.False(string.IsNullOrWhiteSpace(Support.getRoverForDate(testDate)));
        }

        //Considered negative case testing, but did not seem benificial for the test cases
        //[Fact] 
        //public void getRoverForDate_Failure()
        //{
        //    Assert.Throws<Exception>(() => Support.getRoverForDate(new DateTime(2115, 6, 3)));
        //}

        [Fact]
        public async Task getPhotosListTest()
        {
            List<Photos.Photo> photos = await Support.getPhotosList(testDate, "curiosity");
            Assert.True(photos.Count > 0);
        }

        [Fact]
        public void saveFileFromUrlTest()
        {
            string path = @$"{Support.StoragePath}\MarsRoverPhotos_API\StoredMarsRoverPhotos\{testDate.ToString("yyyy-M-d")}";            

            if (Directory.Exists(path)) //delete 
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                dir.Delete(true);
            }
            if (!Directory.Exists(path)) //called prior to save method to prevent needing to check for directory for each individual file
                Directory.CreateDirectory(path); 
            Support.saveFileFromURL(new Uri("https://api.nasa.gov/assets/img/favicons/favicon-192.png"), testDate).Wait(); //Icon from nasa api
            DirectoryInfo di = new DirectoryInfo(path);
            Assert.True(di.GetFiles("*").Any());
        }
    }
}

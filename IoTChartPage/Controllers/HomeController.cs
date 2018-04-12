using IoTChartPage.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Helpers;
using System.Web.Mvc;

namespace IoTChartPage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Opis laboratoriów.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Strona kontaktowa.";

            return View();
        }

        public ActionResult GetChart(DateTime? from, DateTime? to, int? screenHeight, int? screenWidth)
        {
            CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageCredentials"]);
            CloudBlobClient cbc = csa.CreateCloudBlobClient();
            CloudBlobContainer container = cbc.GetContainerReference("iot");
            var blobs = container.ListBlobs();

            IList<IoTData> chartData = new List<IoTData>();
            bool validDateRange = from != null && to != null && from < to;
            foreach (var blob in blobs)
            {
                CloudBlob cBlob = new CloudBlob(blob.Uri, cbc);
                var stream = cBlob.OpenRead();
                StreamReader reader = new StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    var data = JsonConvert.DeserializeObject<IoTData>(reader.ReadLine());
                    if (!validDateRange || data.time >= from && data.time <= to)
                    {
                        chartData.Add(data);
                    }
                }
            }
            //Opcja 1: Zwróć wyrenderowany wykres
            int width = 800;
            int height = 500;
            if (screenWidth != null && screenHeight != null)
            {
                width = (int)screenWidth - 500;
                height = (int)screenHeight - 300;
            }
            var myChart = new Chart(width: width, height: height, themePath: "~/IoTChartTheme.xml")
                .AddSeries("Temperature", legend: "legend", xValue: chartData, xField: "time", yValues: chartData, yFields: "temperature", chartType: "Line")
                .AddSeries("Humidity", legend: "legend", xValue: chartData, xField: "time", yValues: chartData, yFields: "humidity", chartType: "Line")
                .AddLegend("IoT Chart", "legend").GetBytes();
            return Json(new { base64image = Convert.ToBase64String(myChart) }
                , JsonRequestBehavior.AllowGet);
            //Opcja 2: Zwróć dane w postaci JSONa i obrób w javascript w metodzie updateImage
            //return Content(JsonConvert.SerializeObject(chartData, new IsoDateTimeConverter()), "application/json");
        }
    }
}
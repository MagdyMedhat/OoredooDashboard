using MVCBootstrapDashboard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Services;
using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System.Text.RegularExpressions;

namespace MVCBootstrapDashboard.Controllers
{
    public class ChartsController : ApiController
    {
        // GET: /ChartsController/

        /// <summary>
        /// input: POST data as JSON array of SVG files that each represent a Chart
        /// and creates a powerpoint presentation file then import the chart images in the presentation file
        /// then returns the link to the presentation file to download
        /// </summary>
        /// <returns>string: url to the powerpoint file</returns>
        [WebMethod]
        [System.Web.Mvc.HttpPost]
        public string DownloadReport()
        {
            //read the POST data (JSON array) in a string
            HttpContext.Current.Request.InputStream.Position = 0;
            string message = new System.IO.StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();

            string appPhysicalPath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + @"downloads\";
            if (!Directory.Exists(appPhysicalPath))
                Directory.CreateDirectory(appPhysicalPath);

            DirectoryInfo directory = Directory.CreateDirectory(appPhysicalPath + UnixTimeNow().ToString());
            Dictionary<string, string> chartsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

            for (int i = 0; i < chartsList.Count; i++)
            {
                byte[] imageBuffer;
                string imageFileName;

                if (chartsList[i.ToString()].Contains("data:image/png;base64"))
                {
                    imageBuffer = ExportChartURLToImage(chartsList[i.ToString()]);
                    imageFileName = string.Format("chart{0}.png", i.ToString("D3") + "Table");
                }
                else
                {
                    imageBuffer = ExportChartSVGToImage(chartsList[i.ToString()]);
                    imageFileName = string.Format("chart{0}.png", i.ToString("D3"));
                }
                imageFileName = string.Format("chart{0}.png", i.ToString("D3"));
                File.WriteAllBytes(directory.FullName + @"\" + imageFileName, imageBuffer);
            }

            if (!ExportToPPT(directory.FullName))
                return "Error exporting PPT file";

            //string host = "10.123.105.196:80";
            string host = "10.123.105.196";
            String url = string.Format(@"http://{0}/{1}/{2}/{3}", host, "downloads", directory.Name, "Report.pptx");
            return url;
        }

        private static bool CopyPresentation(string copyToPath)
        {
            string templatePath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\downloads\ZainPPTtemplate.pptx";

            try
            {
                File.Copy(templatePath, copyToPath);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private bool ExportToPPT(string picturesFolderPath)
        {
            string pptFilePath = picturesFolderPath + @"\Report.pptx";
            CopyPresentation(pptFilePath);
            using (PresentationDocument presentationDocument = PresentationDocument.Open(pptFilePath, true))
            {
                DirectoryInfo imagesDirectory = new DirectoryInfo(picturesFolderPath);
                var presentationPart = presentationDocument.PresentationPart;
                //var templatePart = GetSlidePartsInOrder(presentationPart).Last();
                var templatePart = GetSlidePartsInOrder(presentationPart).ElementAt(4);

                foreach (FileInfo file in imagesDirectory.GetFiles("*.png").OrderByDescending(U => U.Name))
                {
                    var newSlidePart = CloneSlide(templatePart);
                    AppendSlide(presentationPart, newSlidePart);
                    Slide slide = newSlidePart.Slide;
                    Picture pic = newSlidePart.Slide.Descendants<Picture>().First();
                    AddImagePart(slide, pic.BlipFill.Blip.Embed.Value, file.FullName);

                    //Shape shape = slide.CommonSlideData.ShapeTree.Elements<Shape>().FirstOrDefault(
                    //    sh => sh.NonVisualShapeProperties.NonVisualDrawingProperties.Name.Value.ToLower().Equals("Content Placeholder 2".ToLower()));
                    //Picture pic = AddPicture(slide, shape, file.FullName);
                    //slide.CommonSlideData.ShapeTree.RemoveChild<Shape>(shape);

                    slide.Save();

                }

                presentationDocument.PresentationPart.Presentation.Save();
                presentationDocument.Close();
            }
            return true;
        }
        private long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        private byte[] ExportChartSVGToImage(string svgFile)
        {

            // Create a request using a URL that can receive a post. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://export.highcharts.com/");

            // Set the Method property of the request to POST.
            request.Method = "POST";

            // Create POST data and convert it to a byte array.
            string postData = string.Format("filename={0}&type={1}&width={2}&svg={3}", "chart", "image/png", 1270, HttpUtility.UrlEncode(svgFile));

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded; multipart/form-data";

            //User agent is based in a normal export.js request
            request.UserAgent = @"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();

            HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
            using (var memoryStream = new MemoryStream())
            {
                webResponse.GetResponseStream().CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private byte[] ExportChartURLToImage(string imageURL)
        {
            var base64Data = Regex.Match(imageURL, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            return Convert.FromBase64String(base64Data);
        }

        private static IEnumerable<SlidePart> GetSlidePartsInOrder(PresentationPart presentationPart)
        {
            SlideIdList slideIdList = presentationPart.Presentation.SlideIdList;

            return slideIdList.ChildElements
                .Cast<SlideId>()
                .Select(x => presentationPart.GetPartById(x.RelationshipId))
                .Cast<SlidePart>();
        }
        private static SlidePart CloneSlide(SlidePart templatePart)
        {
            // find the presentationPart: makes the API more fluent
            var presentationPart = templatePart.GetParentParts()
                .OfType<PresentationPart>()
                .Single();

            // clone slide contents
            Slide currentSlide = (Slide)templatePart.Slide.CloneNode(true);
            var slidePartClone = presentationPart.AddNewPart<SlidePart>();
            currentSlide.Save(slidePartClone);

            // copy layout part
            slidePartClone.AddPart(templatePart.SlideLayoutPart);

            return slidePartClone;
        }
        private static string AppendSlide(PresentationPart presentationPart, SlidePart newSlidePart)
        {
            //get slides id list
            SlideIdList slideIdList = presentationPart.Presentation.SlideIdList;

            // find the highest id
            uint maxSlideId = slideIdList.ChildElements
                .Cast<SlideId>()
                .Max(x => x.Id.Value);

            //create new slide id based on max id
            uint newId = maxSlideId + 1;

            //add new slide id item at the second place in the list
            SlideId newSlideId = new SlideId();
            slideIdList.InsertAt(newSlideId, 1);
            newSlideId.Id = newId;
            newSlideId.RelationshipId = presentationPart.GetIdOfPart(newSlidePart);
            //newSlideId.RelationshipId.Value = "rId2";

            return newSlideId.RelationshipId;
        }
        private static Picture AddPicture(Slide slide, Shape referingShape, string imageFile)
        {
            Picture picture = new Picture();

            string embedId = string.Empty;
            DocumentFormat.OpenXml.UInt32Value picId = 10001U;
            string name = string.Empty;

            if (slide.Elements<Picture>().Count() > 0)
            {
                picId = ++slide.Elements<Picture>().ToList().Last().NonVisualPictureProperties.NonVisualDrawingProperties.Id;
            }
            name = "image" + picId.ToString();
            embedId = "rId" + (slide.Elements<Picture>().Count() + 915).ToString(); // some value

            NonVisualPictureProperties nonVisualPictureProperties = new NonVisualPictureProperties()
            {
                NonVisualDrawingProperties = new NonVisualDrawingProperties() { Name = name, Id = picId, Title = name },
                NonVisualPictureDrawingProperties = new NonVisualPictureDrawingProperties() { PictureLocks = new DocumentFormat.OpenXml.Drawing.PictureLocks() { NoChangeAspect = true } },
                ApplicationNonVisualDrawingProperties = new ApplicationNonVisualDrawingProperties() { UserDrawn = true }
            };

            BlipFill blipFill = new BlipFill() { Blip = new DocumentFormat.OpenXml.Drawing.Blip() { Embed = embedId } };
            DocumentFormat.OpenXml.Drawing.Stretch stretch = new DocumentFormat.OpenXml.Drawing.Stretch() { FillRectangle = new DocumentFormat.OpenXml.Drawing.FillRectangle() };
            blipFill.Append(stretch);

            ShapeProperties shapeProperties = new ShapeProperties()
            {
                Transform2D = new DocumentFormat.OpenXml.Drawing.Transform2D()
                {
                    Offset = new DocumentFormat.OpenXml.Drawing.Offset() { X = 457200L, Y = 1124000L },// { X = 1554691, Y = 1600200 },
                    Extents = new DocumentFormat.OpenXml.Drawing.Extents() { Cx = 8229600L, Cy = 5029200L }//{ Cx = 6034617, Cy = 4525963 }
                }
            };
            DocumentFormat.OpenXml.Drawing.PresetGeometry presetGeometry = new DocumentFormat.OpenXml.Drawing.PresetGeometry() { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle };
            DocumentFormat.OpenXml.Drawing.AdjustValueList adjustValueList = new DocumentFormat.OpenXml.Drawing.AdjustValueList();

            presetGeometry.Append(adjustValueList);
            shapeProperties.Append(presetGeometry);
            picture.Append(nonVisualPictureProperties);
            picture.Append(blipFill);
            picture.Append(shapeProperties);

            slide.CommonSlideData.ShapeTree.Append(picture);

            // Add Image part
            AddImagePart(slide, embedId, imageFile);

            slide.Save();
            return picture;
        }
        private static void AddImagePart(Slide slide, string relationshipId, string imageFile)
        {
            ImagePart imgPart = slide.SlidePart.AddImagePart(GetImagePartType(imageFile), relationshipId);
            using (FileStream imgStream = File.Open(imageFile, FileMode.Open))
            {
                imgPart.FeedData(imgStream);
            }
        }
        private static ImagePartType GetImagePartType(string imageFile)
        {
            string[] imgFileSplit = imageFile.Split('.');
            string imgExtension = imgFileSplit.ElementAt(imgFileSplit.Count() - 1).ToString().ToLower();
            if (imgExtension.Equals("jpg"))
                imgExtension = "jpeg";
            return (ImagePartType)Enum.Parse(typeof(ImagePartType), imgExtension, true);
        }
    }
}

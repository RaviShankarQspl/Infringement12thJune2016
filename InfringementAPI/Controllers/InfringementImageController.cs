using InfringementAPI.Providers;
using InfringementAPI.Request;
using InfringementAPI.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace InfringementAPI.Controllers
{
    [RoutePrefix("api/infringement/{infringementNumber}")]
    public class InfringementImageController : ApiController
    {
        [Route("images")]
        [HttpPost]
        public Task<IEnumerable<FileDesc>> Post(string infringementNumber)
        {
            try
            {
                var folderName = String.Format("uploads/{0}", infringementNumber);
                var path = HttpContext.Current.Server.MapPath("~/uploads/" + infringementNumber);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var entity = new infringementEntities();

                var infringement = entity.infringements.FirstOrDefault(x => x.Number == infringementNumber);
                var longitude = (System.Web.HttpContext.Current.Request.Form["longitude"] ?? String.Empty).ToString();
                var latitude = (System.Web.HttpContext.Current.Request.Form["latitude"] ?? String.Empty).ToString();
                var description = (System.Web.HttpContext.Current.Request.Form["description"] ?? String.Empty).ToString();

                var rootUrl = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.AbsolutePath, String.Empty);
                if (Request.Content.IsMimeMultipartContent())
                {
                    var streamProvider = new CustomMultipartFormDataStreamProvider(path);

                    var task = Request.Content.ReadAsMultipartAsync(streamProvider).ContinueWith<IEnumerable<FileDesc>>(t =>
                    {

                        if (t.IsFaulted || t.IsCanceled)
                        {
                            throw new HttpResponseException(HttpStatusCode.InternalServerError);
                        }

                        var fileInfo = streamProvider.FileData.Select(i =>
                        {
                            var info = new FileInfo(i.LocalFileName);
                            return new FileDesc(info.Name, rootUrl + "/" + folderName + "/" + info.Name, info.Length / 1024);
                            //return new FileDesc(info.Name,  folderName + "/" + info.Name, info.Length / 1024);
                        });

                        foreach (var info in fileInfo)
                        {
                            entity.infringementpictures.Add(new infringementpicture()
                            {
                                Latitude = latitude,
                                Longitude = longitude,
                                Description = description,
                                InfringementId = infringement.Id,
                                Location = info.Path
                            });
                        }

                        entity.SaveChanges();

                        return fileInfo;
                    });

                    return task;
                }
                else
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable,
                        "This request is not properly formatted"));
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("images/delete")]
        public IHttpActionResult DeleteImage(Uri imagePath)
        {
            string filename = System.IO.Path.GetFileName(imagePath.AbsolutePath);
            if (File.Exists(filename))
                File.Delete(filename);
            return Ok();
        }
    }
}

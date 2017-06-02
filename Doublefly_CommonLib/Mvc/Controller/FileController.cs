using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Utility.Helper;
using Utility.Extension;

namespace Mvc.Controller
{
    public class FileController : BaseController
    {
        private static readonly bool IsEnableOss = ConfigHelper.GetValue("IsEnableOss").ToBoolean();

        /// <summary>
        /// 上传图片文件
        /// </summary>
        /// <param name="imgDirectory"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public ActionResult UpLoad(string imgDirectory, string imgName)
        {
            var filesize = ConfigHelper.GetValue("ImageSize");

            var fileSize = 1024 * 1024 * (filesize.ToInt32(10));

            if (Request.Files.Count <= 0)
            {
                return Fail("上传文件不能为空");
            }

            var postedFileBase = Request.Files[0];

            if (postedFileBase == null || postedFileBase.ContentLength < 1)
            {
                return Fail("上传文件不能为空");
            }

            if (postedFileBase.ContentLength >= fileSize)
            {
                return Fail("上传凭证大小不能超过" + filesize + "M");
            }

            imgName = imgName ?? Guid.NewGuid().ToString().Replace("-", "");
            var exn = Path.GetExtension(postedFileBase.FileName);
            var fileName = imgName + exn;

            var imageType = ConfigHelper.GetValue("ImageType");

            if (exn != null && imageType.IndexOf(exn, StringComparison.Ordinal) > -1)
            {
                return Fail("文件格式不正确");
            }

            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(postedFileBase.InputStream))
            {
                fileData = binaryReader.ReadBytes(postedFileBase.ContentLength);
            }

            var folder = string.Format("{0}", imgDirectory);

            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);

            var rvalue = FileUploadHelper.Instance.UploadFile(fileData, fileName, savePath);
            if (!rvalue)
            {
                return Fail("文件上传失败");
            }

            var filePath = string.Format("{0}/{1}", folder, fileName);

            if (!IsEnableOss)
                return Success(new
                {
                    filename = fileName,
                    oldfilename = postedFileBase.FileName,
                    filepath = filePath
                });
            var uploadPath = ConfigHelper.GetValue("UpLoadPath");
            if (uploadPath.IsNotNullOrEmpty())
            {
                filePath = filePath.Replace(uploadPath + "/", "");
            }

            return Success(new
            {
                filename = fileName,
                oldfilename = postedFileBase.FileName,
                filepath = filePath
            });
        }

        /// <summary>
        /// 上传临时文件
        /// </summary>
        /// <returns></returns>
        public ActionResult UpLoadTempFile()
        {
            const int fileSize = 1024 * 1024 * 10; //10M

            if (Request.Files.Count <= 0)
            {
                return Fail("上传文件不能为空");
            }

            var postedFileBase = Request.Files[0];

            if (postedFileBase == null || postedFileBase.ContentLength < 1)
            {
                return Fail("上传文件不能为空");
            }

            if (postedFileBase.ContentLength >= fileSize)
            {
                return Fail("上传凭证大小不能超过" + fileSize + "M");
            }

            var imgName = Guid.NewGuid().ToString().Replace("-", "");
            var exn = Path.GetExtension(postedFileBase.FileName);
            var fileName = imgName + exn;

            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(postedFileBase.InputStream))
            {
                fileData = binaryReader.ReadBytes(postedFileBase.ContentLength);
            }

            var folder = string.Format("{0}/{1}", "tempfile", DateTime.Now.ToString("yyyyMMdd"));

            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);

            var rvalue = FileUploadHelper.Instance.UploadFile(fileData, fileName, savePath);
            return !rvalue ? Fail("文件上传失败") : Success(new { fileName });
        }

        public ActionResult UEditorFileUpLoad(string fileType)
        {
            const int size = 10; //文件大小限制,单位mb
            string[] filetype = { ".gif", ".png", ".jpg", ".jpeg", ".bmp" };//文件允许格式

            //上传图片
            var info = FileUploadHelper.Instance.UpFile(HttpContext, fileType, filetype, size); //获取上传状态

            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(info));
        }
    }
}

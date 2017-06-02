using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Utility.Helper
{
    public class FileUploadHelper
    {
        string _state = "SUCCESS";
        string _url = null;
        string _currentType = null;
        string _uploadpath = null;
        string _filename = null;
        string _originalName = null;
        HttpPostedFileBase _uploadFile = null;
        private static readonly string ImgUrl = ConfigHelper.GetValue("ImgUrl");
        private static readonly string EditImagePath = ConfigHelper.GetValue("EditImagePath");

        public static FileUploadHelper Instance
        {
            get
            {
                return new FileUploadHelper();
            }
        }

        /// <summary>
        /// 上传文件的主处理方法
        /// </summary>
        /// <param name="cxt"></param>
        /// <param name="pathbase"></param>
        /// <param name="filetype"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Hashtable UpFile(HttpContextBase cxt, string pathbase, string[] filetype, int size)
        {
            var imgPath = pathbase;
            pathbase = string.Format("{0}/{1}/{2}", EditImagePath, imgPath, DateTime.Now.ToString("yyyyMMdd"));

            _uploadpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathbase);

            try
            {
                _uploadFile = cxt.Request.Files[0];

                if (_uploadFile == null)
                {
                    _state = "请选择文件";
                    return null;
                }

                _originalName = _uploadFile.FileName;
                //目录创建
                CreateFolder();

                //格式验证
                if (CheckType(filetype))
                {
                    _state = "不允许的文件类型";
                }

                //大小验证
                if (CheckSize(size))
                {
                    _state = "文件大小超出网站限制";
                }

                //保存图片
                if (_state == "SUCCESS")
                {
                    _filename = ReName();

                    _uploadFile.SaveAs(_uploadpath + "/" + _filename);
                    _url = string.Format("{0}/{1}/{2}", ImgUrl, pathbase, _filename);
                }
            }
            catch (Exception e)
            {
                _state = e.Message;
                _url = "";
                LogHelper.WriteError(e);
            }
            return GetUploadInfo();
        }

        /// <summary>
        /// 上传涂鸦的主处理方法
        /// </summary>
        /// <param name="cxt"></param>
        /// <param name="pathbase"></param>
        /// <param name="tmppath"></param>
        /// <param name="base64Data"></param>
        /// <returns></returns>
        public Hashtable UpScrawl(HttpContext cxt, string pathbase, string tmppath, string base64Data)
        {
            pathbase = pathbase + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            _uploadpath = cxt.Server.MapPath(pathbase);//获取文件上传路径
            FileStream fs = null;
            try
            {
                //创建目录
                CreateFolder();
                //生成图片
                _filename = Guid.NewGuid() + ".png";
                fs = File.Create(_uploadpath + _filename);
                var bytes = Convert.FromBase64String(base64Data);
                fs.Write(bytes, 0, bytes.Length);

                _url = pathbase + _filename;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex);
                _state = "未知错误";
                _url = "";
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            return GetUploadInfo();
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="cxt"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public string GetOtherInfo(HttpContext cxt, string field)
        {
            string info = null;
            if (cxt.Request.Form[field] != null && !String.IsNullOrEmpty(cxt.Request.Form[field]))
            {
                info = field == "fileName" ? cxt.Request.Form[field].Split(',')[1] : cxt.Request.Form[field];
            }
            return info;
        }

        //获取上传信息
        private Hashtable GetUploadInfo()
        {
            var infoList = new Hashtable
            {
                {"state", _state},
                {"url", _url},
                {"originalName", _originalName},
                {"name", Path.GetFileName(_url)},
                {"size", _uploadFile.ContentLength},
                {"type", Path.GetExtension(_originalName)}
            };

            return infoList;
        }

        /// <summary>
        /// 获取上传信息
        /// </summary>
        /// <returns></returns>
        private string ReName()
        {
            return Guid.NewGuid() + GetFileExt();
        }

        /// <summary>
        /// 文件类型检测
        /// </summary>
        /// <param name="filetype"></param>
        /// <returns></returns>
        private bool CheckType(string[] filetype)
        {
            LogHelper.WriteInfo("getFileExt");
            _currentType = GetFileExt();
            return Array.IndexOf(filetype, _currentType) == -1;
        }

        /// <summary>
        /// 文件大小检测
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private bool CheckSize(int size)
        {
            return _uploadFile.ContentLength >= (size * 1024 * 1024);
        }

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        /// <returns></returns>
        private string GetFileExt()
        {
            LogHelper.WriteInfo("uploadFile.FileName:" + _uploadFile.FileName);
            var temp = _uploadFile.FileName.Split('.');
            return "." + temp[temp.Length - 1].ToLower();
        }

        /// <summary>
        /// 按照日期自动创建存储文件夹
        /// </summary>
        private void CreateFolder()
        {
            if (!Directory.Exists(_uploadpath))
            {
                Directory.CreateDirectory(_uploadpath);
            }
        }

        /// <summary>
        /// 上传文件到本地文件夹
        /// </summary>
        /// <param name="fs">文件</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="path">保存路径</param>
        /// <returns></returns>
        public bool UploadFile(byte[] fs, string fileName, string savePath)
        {
            MemoryStream m = null;
            FileStream f = null;
            try
            {
                m = new MemoryStream(fs);

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                f = new FileStream(savePath + "\\" + fileName, FileMode.Create);
                //把内存里的数据写入物理文件
                m.WriteTo(f);
                m.Close();
                f.Close();
                f = null;
                m = null;

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex);
                return false;
            }
            finally
            {
                if (f != null)
                {
                    f.Close();
                }

                if (m != null)
                {
                    m.Close();
                }
            }
        }
    }
}

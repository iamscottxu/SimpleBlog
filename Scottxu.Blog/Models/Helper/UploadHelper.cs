using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using Scottxu.Blog.Models.Entitys;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.EntityFrameworkCore;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.StaticFiles;
using ICSharpCode.SharpZipLib.Core;

namespace Scottxu.Blog.Models.Helper
{
    /// <summary>
    /// 文件上传帮助类
    /// </summary>
    public class UploadHelper
    {
        readonly BlogSystemContext dataBaseContext;
        readonly IHostingEnvironment hostingEnvironment;
        const string UPLOAD_PATH = "/upload";

        /// <summary>
        /// 上传文件信息类
        /// </summary>
        public class FormFileInfo
        {
            /// <summary>
            /// 获取或设置上传文件实体。
            /// </summary>
            /// <value>上传文件实体</value>
            public UploadedFile UploadedFile { get; set; }

            /// <summary>
            /// 获取或设置文件名。
            /// </summary>
            /// <value>文件名</value>
            public string FileName { get; set; }

            /// <summary>
            /// 获取或设置MIME类型。
            /// </summary>
            /// <value>MIME类型</value>
            public string MIME { get; set; }

            /// <summary>
            /// 获取或设置文件在压缩包中的相对路径
            /// </summary>
            /// <value>文件在压缩包中的相对路径</value>
            public string VirtualPath { get; set; }
        }

        /// <summary>
        /// 初始化 <see cref="T:Scottxu.Blog.Models.Helper.UploadHelper"/> 类的实例。
        /// </summary>
        /// <param name="dataBaseContext">数据库环境</param>
        /// <param name="hostingEnvironment">主机环境信息</param>
        public UploadHelper(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment)
            => (this.dataBaseContext, this.hostingEnvironment) = (dataBaseContext, hostingEnvironment);

        /// <summary>
        /// 保存文件并写入记录到数据库。
        /// </summary>
        /// <returns>上传文件信息的集合</returns>
        /// <param name="httpContext">Http上下文</param>
        public List<FormFileInfo> SaveFiles(HttpContext httpContext)
        {
            var request = httpContext.Request;
            //var session = httpContext.Session;
            var formFileInfos = request.Form.Files.ToList().Select(q =>
            {
                var formFileInfo = new FormFileInfo()
                {
                    FileName = q.FileName,
                    MIME = q.ContentType
                };
                var sha1 = GetFileSHA1(q.OpenReadStream());
                var uploadedFile = dataBaseContext.UploadedFiles.FirstOrDefault(m => m.SHA1 == sha1);
                if (uploadedFile == null) formFileInfo.UploadedFile = new UploadedFile()
                {
                    FileName = SaveFileToDisk(q.OpenReadStream()),
                    SHA1 = sha1,
                    //SessionId = session.Id
                };
                else formFileInfo.UploadedFile = uploadedFile;
                return formFileInfo;
            }).ToList();
            dataBaseContext.UploadedFiles.AddRange(formFileInfos.Select(o => o.UploadedFile).Where(q => false));
            return formFileInfos;
        }


        /// <summary>
        /// 保存Zip文件并写入记录到数据库。
        /// </summary>
        /// <returns>上传文件信息的集合</returns>
        /// <param name="httpContext">Http上下文</param>
        public List<FormFileInfo> SaveZipFiles(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var formFile = request.Form.Files[0];
            return SaveZipFiles(httpContext, formFile.OpenReadStream());
        }

        /// <summary>
        /// 保存Zip文件并写入记录到数据库。
        /// </summary>
        /// <returns>上传文件信息的集合</returns>
        /// <param name="httpContext">Http上下文</param>
        public List<FormFileInfo> SaveZipFiles(HttpContext httpContext, Stream fileStream)
        {
            //var session = httpContext.Session;
            List<FormFileInfo> formFileInfos = new List<FormFileInfo>();
            using (var zipInputStream = new ZipInputStream(fileStream))
            {
                ZipEntry zipEntry;
                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                {
                    if (zipEntry.IsFile)
                    {
                        var memoryStream = new MemoryStream();
                        byte[] buffer = new byte[4096];
                        StreamUtils.Copy(zipInputStream, memoryStream, buffer);
                        memoryStream.Position = 0;
                        string sha1 = GetFileSHA1(memoryStream);
                        var uploadedFile = dataBaseContext.UploadedFiles.FirstOrDefault(m => m.SHA1 == sha1);
                        if (uploadedFile == null) uploadedFile = formFileInfos.Select(o => o.UploadedFile).FirstOrDefault(m => m.SHA1 == sha1);
                        if (uploadedFile == null)
                        {
                            memoryStream.Position = 0;
                            string fileName = SaveFileToDisk(memoryStream);
                            uploadedFile = new UploadedFile
                            {
                                FileName = fileName,
                                SHA1 = sha1,
                                //SessionId = session.Id
                            };
                        }
                        var formFileInfo = new FormFileInfo()
                        {
                            UploadedFile = uploadedFile,
                            FileName = Path.GetFileName(zipEntry.Name),
                            VirtualPath = $"/{zipEntry.Name}",
                            MIME = GetMappings(zipEntry.Name)
                        };
                        formFileInfos.Add(formFileInfo);
                    }
                }
                dataBaseContext.UploadedFiles.AddRange(formFileInfos.Select(o => o.UploadedFile).Where(q => false));
                return formFileInfos;
            }
        }

        /// <summary>
        /// 检查并删除文件。
        /// </summary>
        /// <param name="uploadedFileGuid">要删除文件在数据库中的Guid</param>
        public void CheckAndDeleteFile(Guid uploadedFileGuid)
        {
            UploadedFile uploadedFile = dataBaseContext.UploadedFiles
                                                       .Include(o => o.TemplateFiles)
                                                       .Include(o => o.UploadedFileArticles)
                                                       .FirstOrDefault(q => q.Guid == uploadedFileGuid);

            var templateFiles = new List<TemplateFile>();
            uploadedFile.TemplateFiles.ToList().ForEach(n =>
            {
                if (dataBaseContext.Entry(n).State != EntityState.Deleted) templateFiles.Add(n);
            });
            var UploadedFileArticles = new List<UploadedFileArticle>();
            uploadedFile.UploadedFileArticles.ToList().ForEach(n =>
            {
                if (dataBaseContext.Entry(n).State != EntityState.Deleted) UploadedFileArticles.Add(n);
            });
            if (!UploadedFileArticles.Any() && !templateFiles.Any())
            {
                DeleteFileFromDisk(GetFileInfo(uploadedFile));
                dataBaseContext.UploadedFiles.Remove(uploadedFile);
            }
        }

        /// <summary>
        /// 获取文件信息。
        /// </summary>
        /// <returns>文件信息</returns>
        /// <param name="uploadedFile">上传文件实体</param>
        public FileInfo GetFileInfo(UploadedFile uploadedFile)
            => new FileInfo($"{hostingEnvironment.ContentRootPath}{UPLOAD_PATH}/{uploadedFile.FileName}");

        /// <summary>
        /// 保存文件到磁盘
        /// </summary>
        /// <returns>文件名</returns>
        /// <param name="stream">文件流</param>
        string SaveFileToDisk(Stream stream)
        {
            var fileName = Guid.NewGuid().ToString("N");
            var fileStream = File.Create($"{hostingEnvironment.ContentRootPath}{UPLOAD_PATH}/{fileName}");
            stream.CopyTo(fileStream);
            fileStream.Close();
            return fileName;
        }

        /// <summary>
        /// 从磁盘删除文件。
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        static void DeleteFileFromDisk(FileInfo fileInfo)
        {
            if (fileInfo.Exists) fileInfo.Delete();
        }

        /// <summary>
        /// 获取文件的SHA1值。
        /// </summary>
        /// <returns>SHA1值</returns>
        /// <param name="stream">文件流</param>
        static string GetFileSHA1(Stream stream)
            => BytesToHexString(new SHA1CryptoServiceProvider().ComputeHash(stream));

        /// <summary>
        /// 将字节数组转换为十六进制字符串。
        /// </summary>
        /// <returns>十六进制字符串</returns>
        /// <param name="bytes">字节数组</param>
        static string BytesToHexString(byte[] bytes)
        {
            var stringBuilder = new StringBuilder();
            bytes.ToList().ForEach(q => stringBuilder.Append(q.ToString("x2")));
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 获取指定文件的MIME类型
        /// </summary>
        /// <returns>MIME类型</returns>
        /// <param name="fileName">File name.</param>
        static string GetMappings(string fileName)
        {
            string extensionName = Path.GetExtension(fileName);
            var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
            if (!fileExtensionContentTypeProvider.Mappings.TryGetValue(extensionName, out string value))
                value = "application/octet-stream";
            return value;
        }
    }
}

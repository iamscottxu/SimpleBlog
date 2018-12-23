using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Scottxu.Blog.Models.Entities;

namespace Scottxu.Blog.Models.Units
{
    /// <summary>
    /// 文件上传帮助类
    /// </summary>
    public class UploadUnit
    {
        readonly BlogSystemContext _dataBaseContext;
        readonly IHostingEnvironment _hostingEnvironment;
        const string UploadPath = "/upload";

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
        /// 初始化 <see cref="T:Scottxu.Blog.Models.Units.UploadUnit"/> 类的实例。
        /// </summary>
        /// <param name="dataBaseContext">数据库环境</param>
        /// <param name="hostingEnvironment">主机环境信息</param>
        public UploadUnit(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment)
            => (this._dataBaseContext, this._hostingEnvironment) = (dataBaseContext, hostingEnvironment);

        /// <summary>
        /// 保存文件并写入记录到数据库。
        /// </summary>
        /// <returns>上传文件信息的集合</returns>
        /// <param name="httpContext">Http上下文</param>
        public List<FormFileInfo> SaveFiles(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var formFileInfos = new List<FormFileInfo>();
            request.Form.Files.ToList().ForEach(q =>
            {
                var sha1 = GetFileSHA1(q.OpenReadStream());

                var uploadedFile = (_dataBaseContext.UploadedFiles.FirstOrDefault(m => m.SHA1 == sha1) ??
                                    formFileInfos.Select(o => o.UploadedFile)
                                        .FirstOrDefault(m => m.SHA1 == sha1)) ??
                                   new UploadedFile
                                   {
                                       FileName = SaveFileToDisk(q.OpenReadStream()),
                                       GzipFileName = SaveGzipFileToDisk(q.OpenReadStream()),
                                       Size = q.OpenReadStream().Length,
                                       SHA1 = sha1
                                   };
                formFileInfos.Add(new FormFileInfo()
                {
                    UploadedFile = uploadedFile,
                    FileName = Path.GetFileName(q.FileName),
                    VirtualPath = q.FileName,
                    MIME = GetMappings(q.FileName)
                });
            });
            _dataBaseContext.UploadedFiles.AddRange(formFileInfos.Select(o => o.UploadedFile).Where(q => false));
            return formFileInfos;
        }


        /// <summary>
        /// 保存Zip文件并写入记录到数据库。
        /// </summary>
        /// <returns>上传文件信息的集合</returns>
        /// <param name="httpContext">Http上下文</param>
        public (Template, List<FormFileInfo>) SaveZipFiles(HttpContext httpContext)
        {
            var formFile = httpContext.Request.Form.Files[0];
            return SaveZipFiles(formFile.OpenReadStream());
        }

        /// <summary>
        /// 保存Zip文件并写入记录到数据库。
        /// </summary>
        /// <returns>上传文件信息的集合</returns>
        /// <param name="fileStream">文件流</param>
        public (Template, List<FormFileInfo>) SaveZipFiles(Stream fileStream)
        {
            Template template = null;
            var memoryFileStream = new MemoryStream();
            fileStream.CopyTo(memoryFileStream);
            fileStream.Position = 0;
            memoryFileStream.Position = 0;
            using (var zipInputStream = new ZipInputStream(memoryFileStream))
            {
                ZipEntry zipEntry;
                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                {
                    if (!(
                        zipEntry.IsFile
                        && zipEntry.Name.Equals("configs.xml", StringComparison.OrdinalIgnoreCase)
                    )) continue;
                    var memoryStream = new MemoryStream();
                    var buffer = new byte[4096];
                    StreamUtils.Copy(zipInputStream, memoryStream, buffer);
                    memoryStream.Position = 0;
                    template = DeserializeTemplateFrom(memoryStream);
                    break;
                }
            }

            if (template == null) throw new Exception.TemplateFileAddItemConfigFileNotExistException("找不到模板配置文件。");

            var formFileInfos = new List<FormFileInfo>();
            memoryFileStream = new MemoryStream();
            fileStream.CopyTo(memoryFileStream);
            fileStream.Position = 0;
            memoryFileStream.Position = 0;
            using (var zipInputStream = new ZipInputStream(memoryFileStream))
            {
                ZipEntry zipEntry;
                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                {
                    if (!(
                        zipEntry.IsFile &&
                        zipEntry.Name.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase)
                    )) continue;
                    var fileMemoryStream = new MemoryStream();
                    StreamUtils.Copy(zipInputStream, fileMemoryStream, new byte[4096]);

                    var sha1 = GetFileSHA1(fileMemoryStream);

                    var uploadedFile = (_dataBaseContext.UploadedFiles.FirstOrDefault(m => m.SHA1 == sha1) ??
                                        formFileInfos.Select(o => o.UploadedFile)
                                            .FirstOrDefault(m => m.SHA1 == sha1)) ??
                                       new UploadedFile
                                       {
                                           FileName = SaveFileToDisk(fileMemoryStream),
                                           GzipFileName = SaveGzipFileToDisk(fileMemoryStream),
                                           Size = fileMemoryStream.Length,
                                           SHA1 = sha1
                                       };
                    fileStream.Close();
                    formFileInfos.Add(new FormFileInfo()
                    {
                        UploadedFile = uploadedFile,
                        FileName = Path.GetFileName(zipEntry.Name),
                        VirtualPath = zipEntry.Name.Remove(0, "wwwroot".Length),
                        MIME = GetMappings(zipEntry.Name)
                    });
                }
            }
            memoryFileStream.Close();
            _dataBaseContext.UploadedFiles.AddRange(formFileInfos.Select(o => o.UploadedFile).Where(q => false));
            return (template, formFileInfos);
        }

        /// <summary>
        /// 检查并删除文件。
        /// </summary>
        /// <param name="uploadedFileGuid">要删除文件在数据库中的Guid</param>
        public void CheckAndDeleteFile(Guid uploadedFileGuid)
        {
            var uploadedFile = _dataBaseContext.UploadedFiles
                .Include(o => o.TemplateFiles)
                .Include(o => o.DiskFiles)
                .FirstOrDefault(q => q.Guid == uploadedFileGuid);

            var templateFiles = new List<TemplateFile>();
            if (uploadedFile == null) return;
            uploadedFile.TemplateFiles.ToList().ForEach(n =>
            {
                if (_dataBaseContext.Entry(n).State != EntityState.Deleted) templateFiles.Add(n);
            });
            var diskFiles = new List<DiskFile>();
            uploadedFile.DiskFiles.ToList().ForEach(n =>
            {
                if (_dataBaseContext.Entry(n).State != EntityState.Deleted) diskFiles.Add(n);
            });
            if (diskFiles.Any() || templateFiles.Any()) return;
            DeleteFileFromDisk(GetFileInfo(uploadedFile, true));
            DeleteFileFromDisk(GetFileInfo(uploadedFile, false));
            _dataBaseContext.UploadedFiles.Remove(uploadedFile);
        }

        /// <summary>
        /// 获取文件信息。
        /// </summary>
        /// <returns>文件信息</returns>
        /// <param name="uploadedFile">上传文件实体</param>
        /// <param name="enableGzip">启用Gzip</param>
        public FileInfo GetFileInfo(UploadedFile uploadedFile, bool enableGzip)
        {
            if (enableGzip && string.IsNullOrEmpty(uploadedFile.GzipFileName)) return null;
            if (!enableGzip && string.IsNullOrEmpty(uploadedFile.FileName)) return null;
            return new FileInfo(Path.Join($"{_hostingEnvironment.ContentRootPath}{UploadPath}",
                enableGzip ? uploadedFile.GzipFileName : uploadedFile.FileName));
        }

        /// <summary>
        /// 保存文件到磁盘
        /// </summary>
        /// <returns>文件名</returns>
        /// <param name="stream">文件流</param>
        string SaveFileToDisk(Stream stream)
        {
            stream.Position = 0;
            var fileName = Guid.NewGuid().ToString("N");
            using (var fileStream = File.Create($"{_hostingEnvironment.ContentRootPath}{UploadPath}/{fileName}"))
            {
                stream.CopyTo(fileStream);
            }
            return fileName;
        }

        /// <summary>
        /// 保存Gzip压缩到文件到磁盘
        /// </summary>
        /// <returns>文件名</returns>
        /// <param name="stream">文件流</param>
        string SaveGzipFileToDisk(Stream stream)
        {
            stream.Position = 0;
            using (var gzipFileMemoryStream = new MemoryStream())
            {
                using (var compressedZipStream = new GZipStream(gzipFileMemoryStream, CompressionMode.Compress, true))
                {
                    stream.CopyTo(compressedZipStream);
                }
                return SaveFileToDisk(gzipFileMemoryStream);
            }
        }

        /// <summary>
        /// 从磁盘删除文件。
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        static void DeleteFileFromDisk(FileSystemInfo fileInfo)
        {
            if (fileInfo != null && fileInfo.Exists) fileInfo.Delete();
        }

        /// <summary>
        /// 获取文件的SHA1值。
        /// </summary>
        /// <returns>SHA1值</returns>
        /// <param name="stream">文件流</param>
        static string GetFileSHA1(Stream stream)
        {
            stream.Position = 0;
            return BytesToHexString(new SHA1CryptoServiceProvider().ComputeHash(stream));
        }

        /// <summary>
        /// 将字节数组转换为十六进制字符串。
        /// </summary>
        /// <returns>十六进制字符串</returns>
        /// <param name="bytes">字节数组</param>
        static string BytesToHexString(IEnumerable<byte> bytes)
        {
            var stringBuilder = new StringBuilder();
            bytes.ToList().ForEach(q => stringBuilder.Append(q.ToString("x2")));
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 获取指定文件的MIME类型
        /// </summary>
        /// <returns>MIME类型</returns>
        /// <param name="fileName">文件名</param>
        static string GetMappings(string fileName)
        {
            var extensionName = Path.GetExtension(fileName);
            var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
            if (!fileExtensionContentTypeProvider.Mappings.TryGetValue(extensionName, out var value))
                value = "application/octet-stream";
            return value;
        }

        /// <summary>
        /// 将模板配置文件流序列化成对象
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns>页面模板</returns>
        static Template DeserializeTemplateFrom(Stream stream) =>
            (Template) new XmlSerializer(typeof(Template)).Deserialize(stream);
    }
}
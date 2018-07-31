using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helper;
using Scottxu.Blog.Models.ViewModel;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 页面模板文件
    /// </summary>
    public class TemplateFile : GuidEntity
    {
        /// <summary>
        /// 获取或设置文件的名称。
        /// </summary>
        /// <value>文件的名称</value>
        [StringLength(260)]
        public String Name { get; set; }

        /// <summary>
        /// 获取或设置虚拟路径。
        /// </summary>
        /// <value>虚拟路径</value>
        [Required, StringLength(500)]
        public String VirtualPath { get; set; }

        /// <summary>
        /// 获取或设置上传文件。
        /// </summary>
        /// <value>上传文件</value>
        [Required]
        public virtual UploadedFile UploadedFile { get; set; }

        [Required]
        public Guid UploadedFileGuid { get; set; }

        /// <summary>
        /// 获取或设置文件MIME类型。
        /// </summary>
        /// <value>MIME类型</value>
        [MaxLength]
        public String MIME { get; set; }

        public static List<TemplateFile> GetData(BlogSystemContext dataBaseContext, PageInfoViewModel pageInfo, string searchMessage)
        {
            var efHelper = new EFHelper(dataBaseContext);

            IQueryable<TemplateFile> q = dataBaseContext.TemplateFiles.Include(o => o.UploadedFile);

            // 表单搜索
            string searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.Name.Contains(searchText));
            }

            if (pageInfo != null)
            {
                // 在添加条件之后，排序和分页之前获取总记录数
                pageInfo.RecordCount = q.Count();

                // 排列和数据库分页
                q = efHelper.SortAndPage(q, pageInfo);
            }

            return q.ToList();
        }

        public static void Delete(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment, Guid[] deleteGuid)
        {
            var uploadHelper = new UploadHelper(dataBaseContext, hostingEnvironment);
            deleteGuid.ToList().ForEach(d =>
            {
                var templateFiles = dataBaseContext.TemplateFiles.FirstOrDefault(q => q.Guid == d);
                var uploadedFileGuid = templateFiles.UploadedFileGuid;
                dataBaseContext.TemplateFiles.Remove(templateFiles);
                uploadHelper.CheckAndDeleteFile(uploadedFileGuid);
            });
        }

        public delegate List<UploadHelper.FormFileInfo> AddItemSaveFilesDelegate(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment);

        public static void AddItem(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment, string virtualPath, AddItemSaveFilesDelegate saveFiles)
        {
            FormatVerificationHelper.FormatVerification(virtualPath, FormatVerificationHelper.FormatType.VirtualPath, new ParametersFormatErrorException("虚拟路径格式错误。"));
            if (dataBaseContext.TemplateFiles.Where(q => q.VirtualPath == virtualPath).Count() > 0)
                throw new TemplateFileAddItemVirtualPathIsExistException("此虚拟路径已经存在。");
            var formFileInfos = saveFiles(dataBaseContext, hostingEnvironment);
            dataBaseContext.TemplateFiles.AddRange(formFileInfos.Select(formFile => new TemplateFile()
            {
                MIME = formFile.MIME,
                Name = formFile.FileName,
                UploadedFile = formFile.UploadedFile,
                VirtualPath = virtualPath
            }));
        }

        public static void EditItem(BlogSystemContext dataBaseContext, Guid guid, string virtualPath)
        {
            FormatVerificationHelper.FormatVerification(virtualPath, FormatVerificationHelper.FormatType.VirtualPath, new ParametersFormatErrorException("虚拟路径格式错误。"));
            if (dataBaseContext.TemplateFiles.Where(q => q.VirtualPath == virtualPath).Count() > 0)
                throw new TemplateFileAddItemVirtualPathIsExistException("此虚拟路径已经存在。");
            var templateFiles = dataBaseContext.TemplateFiles.Where(q => q.Guid == guid).FirstOrDefault();
            templateFiles.VirtualPath = virtualPath;
        }

        public static void DeleteAll(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment)
        {
            var uploadHelper = new UploadHelper(dataBaseContext, hostingEnvironment);

            //清除所有模板文件
            dataBaseContext.TemplateFiles.Select(o => o.Guid).ToList().ForEach(d =>
            {
                var templateFile = dataBaseContext.TemplateFiles.FirstOrDefault(q => q.Guid == d);
                var uploadedFileGuid = templateFile.UploadedFileGuid;
                dataBaseContext.TemplateFiles.Remove(templateFile);
                uploadHelper.CheckAndDeleteFile(uploadedFileGuid);
            });
        }

        public delegate List<UploadHelper.FormFileInfo> AddZipFileSaveFileDelegate(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment);

        public static void AddZipFile(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment, AddZipFileSaveFileDelegate saveFile)
        {
            var formFileInfos = saveFile(dataBaseContext, hostingEnvironment);
            dataBaseContext.TemplateFiles.AddRange(formFileInfos.Select(formFile => new TemplateFile()
            {
                MIME = formFile.MIME,
                Name = formFile.FileName,
                UploadedFile = formFile.UploadedFile,
                VirtualPath = formFile.VirtualPath
            }));
        }
    }
}

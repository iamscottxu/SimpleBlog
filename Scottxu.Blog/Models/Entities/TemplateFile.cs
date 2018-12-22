using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helpers;
using Scottxu.Blog.Models.Units;
using Scottxu.Blog.Models.ViewModels;

namespace Scottxu.Blog.Models.Entities
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
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置页面模板。
        /// </summary>
        /// <value>页面模板</value>
        [Required]
        public virtual Template Template { get; set; }

        [Required] public Guid TemplateGuid { get; set; }

        /// <summary>
        /// 获取或设置虚拟路径。
        /// </summary>
        /// <value>虚拟路径</value>
        [Required, StringLength(500)]
        public string VirtualPath { get; set; }

        /// <summary>
        /// 获取或设置上传文件。
        /// </summary>
        /// <value>上传文件</value>
        [Required]
        public virtual UploadedFile UploadedFile { get; set; }

        [Required] public Guid UploadedFileGuid { get; set; }

        /// <summary>
        /// 获取或设置文件MIME类型。
        /// </summary>
        /// <value>MIME类型</value>
        [MaxLength]
        public string MIME { get; set; }

        public static List<TemplateFile> GetData(BlogSystemContext dataBaseContext, PageInfoViewModel pageInfo,
            string searchMessage, Guid? templateGuid = null)
        {
            var efHelper = new EFHelper(dataBaseContext);

            IQueryable<TemplateFile> q = dataBaseContext.TemplateFiles.Include(o => o.UploadedFile);

            if (templateGuid != null) q = q.Where(o => o.TemplateGuid == templateGuid);

            // 表单搜索
            var searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.Name.Contains(searchText));
            }

            if (pageInfo == null) return q.ToList();
            // 在添加条件之后，排序和分页之前获取总记录数
            pageInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = efHelper.SortAndPage(q, pageInfo);

            return q.ToList();
        }

        public static void Delete(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment,
            IEnumerable<Guid> deleteGuid)
        {
            var uploadUnit = new UploadUnit(dataBaseContext, hostingEnvironment);
            deleteGuid.ToList().ForEach(d =>
            {
                var templateFiles = dataBaseContext.TemplateFiles.FirstOrDefault(q => q.Guid == d);
                if (templateFiles == null) return;
                var uploadedFileGuid = templateFiles.UploadedFileGuid;
                dataBaseContext.TemplateFiles.Remove(templateFiles);
                uploadUnit.CheckAndDeleteFile(uploadedFileGuid);
            });
        }

        public delegate List<UploadUnit.FormFileInfo> AddItemSaveFilesDelegate(BlogSystemContext dataBaseContext,
            IHostingEnvironment hostingEnvironment);

        public static void AddItem(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment, 
            string virtualPath, Guid templateGuid, AddItemSaveFilesDelegate saveFiles)
        {
            FormatVerificationHelper.FormatVerification(virtualPath, FormatVerificationHelper.FormatType.VirtualPath,
                new ParametersFormatErrorException("虚拟路径格式错误。"));
            if (dataBaseContext.TemplateFiles.Any(q => q.VirtualPath == virtualPath))
                throw new TemplateFileAddItemVirtualPathIsExistException("此虚拟路径已经存在。");
            var formFileInfos = saveFiles(dataBaseContext, hostingEnvironment);
            dataBaseContext.TemplateFiles.AddRange(formFileInfos.Select(formFile => new TemplateFile()
            {
                MIME = formFile.MIME,
                Name = formFile.FileName,
                UploadedFile = formFile.UploadedFile,
                TemplateGuid = templateGuid,
                VirtualPath = virtualPath
            }));
        }

        public static void EditItem(BlogSystemContext dataBaseContext, Guid guid, string virtualPath)
        {
            FormatVerificationHelper.FormatVerification(virtualPath, FormatVerificationHelper.FormatType.VirtualPath,
                new ParametersFormatErrorException("虚拟路径格式错误。"));
            if (dataBaseContext.TemplateFiles.Any(q => q.VirtualPath == virtualPath))
                throw new TemplateFileAddItemVirtualPathIsExistException("此虚拟路径已经存在。");
            var templateFile = dataBaseContext.TemplateFiles.FirstOrDefault(q => q.Guid == guid);
            if (templateFile != null) templateFile.VirtualPath = virtualPath;
        }
    }
}
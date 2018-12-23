using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helpers;
using Scottxu.Blog.Models.Units;
using Scottxu.Blog.Models.ViewModels;

namespace Scottxu.Blog.Models.Entities
{
    /// <summary>
    /// 页面模板
    /// </summary>
    [Serializable]
    public class Template : IKeyGuid
    {
        /// <summary>
        /// 获取或设置GUID。
        /// </summary>
        /// <value>GUID</value>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Guid { get; set; }

        /// <summary>
        /// 获取或设置模板的名称。
        /// </summary>
        /// <value>模板的名称</value>
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置模板集合。
        /// </summary>
        /// <value>模板集合</value>
        [XmlIgnore]
        public virtual ICollection<TemplateFile> TemplateFiles { get; set; }

        public static List<Template> GetData(BlogSystemContext dataBaseContext, PageInfoViewModel pageInfo,
            string searchMessage)
        {
            var efHelper = new EFHelper(dataBaseContext);

            IQueryable<Template> q = dataBaseContext.Templates;

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
            deleteGuid.ToList().ForEach(e =>
            {
                var template = dataBaseContext.Templates.FirstOrDefault(q => q.Guid == e);
                if (template == null) return;
                var uploadUnit = new UploadUnit(dataBaseContext, hostingEnvironment);
                //清除所有模板文件
                dataBaseContext.TemplateFiles
                    .Where(t => t.TemplateGuid == template.Guid)
                    .Select(o => o.Guid)
                    .ToList().ForEach(d =>
                    {
                        var templateFile = dataBaseContext.TemplateFiles.FirstOrDefault(q => q.Guid == d);
                        if (templateFile == null) return;
                        var uploadedFileGuid = templateFile.UploadedFileGuid;
                        dataBaseContext.TemplateFiles.Remove(templateFile);
                        uploadUnit.CheckAndDeleteFile(uploadedFileGuid);
                    });
                dataBaseContext.Templates.Remove(template);
            });
        }

        public static void AddItem(BlogSystemContext dataBaseContext, string name)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.TemplateName,
                new ParametersFormatErrorException("模板名称格式错误。"));
            dataBaseContext.Templates.Add(new Template()
            {
                Guid = Guid.NewGuid(),
                Name = name
            });
        }

        public delegate (Template template, List<UploadUnit.FormFileInfo>) AddZipFileSaveFileDelegate(
            BlogSystemContext dataBaseContext,
            IHostingEnvironment hostingEnvironment);

        public static void AddZipFile(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment,
            AddZipFileSaveFileDelegate saveFile)
        {
            var (template, formFileInfos) = saveFile(dataBaseContext, hostingEnvironment);
            if (dataBaseContext.Templates.Any(q => q.Guid == template.Guid))
                Delete(dataBaseContext, hostingEnvironment, new List<Guid> {template.Guid});
            else dataBaseContext.Templates.Add(template);
            dataBaseContext.TemplateFiles.AddRange(formFileInfos.Select(formFile => new TemplateFile()
            {
                MIME = formFile.MIME,
                Name = formFile.FileName,
                UploadedFile = formFile.UploadedFile,
                Template = template,
                VirtualPath = formFile.VirtualPath
            }));
        }

        public static void EditItem(BlogSystemContext dataBaseContext, Guid guid, string name)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.TemplateName,
                new ParametersFormatErrorException("模板名称格式错误。"));
            var template = dataBaseContext.Templates.FirstOrDefault(q => q.Guid == guid);
            if (template != null) template.Name = name;
        }
    }
}
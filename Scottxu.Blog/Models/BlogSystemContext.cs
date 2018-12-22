using Microsoft.EntityFrameworkCore;
using Scottxu.Blog.Models.Entities;
using System.Linq;

namespace Scottxu.Blog.Models
{
    /// <summary>
    /// 博客系数据库环境
    /// </summary>
    public class BlogSystemContext : DbContext
    {
        /// <summary>
        /// 获取或设置文章数据集。
        /// </summary>
        /// <value>文章数据集</value>
        public DbSet<Article> Articles { get; set; }

        /// <summary>
        /// 获取或设置文章标签数据集。
        /// </summary>
        /// <value>文章标签数据集</value>
        public DbSet<ArticleLabel> ArticleLabels { get; set; }

        /// <summary>
        /// 获取或设置文章类别数据集。
        /// </summary>
        /// <value>文章类别数据集</value>
        public DbSet<ArticleType> ArticleTypes { get; set; }

        /// <summary>
        /// 获取或设置页面模板数据集。
        /// </summary>
        /// <value>页面模板数据集</value>
        public DbSet<Template> Templates { get; set; }

        /// <summary>
        /// 获取或设置页面模板文件数据集。
        /// </summary>
        /// <value>页面模板文件数据集</value>
        public DbSet<TemplateFile> TemplateFiles { get; set; }

        /// <summary>
        /// 获取或设置上传文件数据集。
        /// </summary>
        /// <value>上传文件数据集</value>
        public DbSet<UploadedFile> UploadedFiles { get; set; }

        /// <summary>
        /// 获取或设置文章标签文章数据集。
        /// </summary>
        /// <value>文章标签文章数据集</value>
        public DbSet<ArticleLabelArticle> ArticleLabelArticles { get; set; }

        /// <summary>
        /// 获取或设置上传文件文章数据集。
        /// </summary>
        /// <value>上传文件文章数据集</value>
        public DbSet<UploadedFileArticle> UploadedFileArticles { get; set; }

        /// <summary>
        /// 获取或设置配置数据集。
        /// </summary>
        /// <value>配置数据集</value>
        public DbSet<Config> Configs { get; set; }

        public BlogSystemContext(DbContextOptions options) : base(options)
        {
        }

        public bool DataBaseIsExist
        {
            get
            {
                try
                {
                    Configs.Any();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //
            modelBuilder.Entity<ArticleType>()
                .HasMany(r => r.Articles)
                .WithOne(l => l.ArticleType)
                .HasForeignKey(f => f.ArticleTypeGuid)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            //
            modelBuilder.Entity<ArticleLabelArticle>()
                .HasKey(k => new {k.ArticleGuid, k.ArticleLabelGuid});

            modelBuilder.Entity<ArticleLabelArticle>()
                .HasOne(r => r.Article)
                .WithMany(l => l.ArticleLabelArticles)
                .HasForeignKey(f => f.ArticleGuid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleLabelArticle>()
                .HasOne(r => r.ArticleLabel)
                .WithMany(l => l.ArticleLabelArticles)
                .HasForeignKey(f => f.ArticleLabelGuid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleLabelArticle>()
                .HasKey(k => new {k.ArticleGuid, k.ArticleLabelGuid});

            //
            modelBuilder.Entity<UploadedFileArticle>()
                .HasKey(k => new {k.UploadedFileGuid, k.ArticleGuid});

            modelBuilder.Entity<UploadedFileArticle>()
                .HasOne(r => r.Article)
                .WithMany(l => l.UploadedFileArticles)
                .HasForeignKey(f => f.ArticleGuid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UploadedFileArticle>()
                .HasOne(r => r.UploadedFile)
                .WithMany(l => l.UploadedFileArticles)
                .HasForeignKey(f => f.UploadedFileGuid)
                .OnDelete(DeleteBehavior.Cascade);

            //
            modelBuilder.Entity<UploadedFile>()
                .HasIndex(i => i.SHA1)
                .IsUnique(true);

            modelBuilder.Entity<UploadedFile>()
                .HasIndex(i => i.FileName)
                .IsUnique(true);

            modelBuilder.Entity<UploadedFile>()
                .HasMany(r => r.TemplateFiles)
                .WithOne(l => l.UploadedFile)
                .IsRequired(true)
                .HasForeignKey(f => f.UploadedFileGuid)
                .OnDelete(DeleteBehavior.Cascade);

            //
            modelBuilder.Entity<Template>()
                .HasMany(r => r.TemplateFiles)
                .WithOne(l => l.Template)
                .IsRequired(true)
                .HasForeignKey(f => f.TemplateGuid)
                .OnDelete(DeleteBehavior.Cascade);


            //
            modelBuilder.Entity<TemplateFile>()
                .HasIndex(i => i.VirtualPath)
                .IsUnique(true);


            modelBuilder.Entity<ArticleType>()
                .HasOne(l => l.ParentArticleType)
                .WithMany(r => r.ChildArticleTypes)
                .HasForeignKey(f => f.ParentArticleTypeGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
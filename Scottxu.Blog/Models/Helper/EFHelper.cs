using System;
using System.Collections.Generic;
using System.Linq;
using Scottxu.Blog.Models.Entitys;
using Scottxu.Blog.Models.Util;
using Scottxu.Blog.Models.ViewModel;

namespace Scottxu.Blog.Models.Helper
{
    public class EFHelper
    {
        BlogSystemContext DataBaseContext { get; }
        public EFHelper(BlogSystemContext dataBaseContext) => DataBaseContext = dataBaseContext;

        #region EF相关

        public IQueryable<T> Sort<T>(IQueryable<T> q, PageInfoViewModel pageInfo) => q.SortBy(pageInfo.SortField + " " + pageInfo.SortDirection);

        // 排序
        public IQueryable<T> Sort<T>(IQueryable<T> q, string sortField, string sortDirection) => q.SortBy(sortField + " " + sortDirection);

        public IQueryable<T> SortAndPage<T>(IQueryable<T> q, PageInfoViewModel pageInfo) => SortAndPage(q, pageInfo.PageIndex, pageInfo.PageSize, pageInfo.RecordCount, pageInfo.SortField, pageInfo.SortDirection);

        // 排序后分页
        public IQueryable<T> SortAndPage<T>(IQueryable<T> q, int pageIndex, int pageSize, int recordCount, string sortField, string sortDirection)
        {
            if (recordCount == 0) return q;
            //// 对传入的 pageIndex 进行有效性验证//////////////
            int pageCount = recordCount / pageSize;
            if (recordCount % pageSize != 0)
            {
                pageCount++;
            }
            if (pageIndex > pageCount - 1)
            {
                pageIndex = pageCount - 1;
            }
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            ///////////////////////////////////////////////

            return Sort(q, sortField, sortDirection).Skip(pageIndex * pageSize).Take(pageSize);
        }


        // 附加实体到数据库上下文中（首先在Local中查找实体是否存在，不存在才Attach，否则会报错）
        // http://patrickdesjardins.com/blog/entity-framework-4-3-an-object-with-the-same-key-already-exists-in-the-objectstatemanager
        protected T Attach<T>(Guid keyGuid) where T : class, IKeyGuid, new()
        {
            T t = DataBaseContext.Set<T>().Local.Where(x => x.Guid == keyGuid).FirstOrDefault();
            if (t == null)
            {
                t = new T { Guid = keyGuid };
                DataBaseContext.Set<T>().Attach(t);
            }
            return t;
        }

        // 向现有实体集合中添加新项
        protected void AddEntities<T>(ICollection<T> existItems, Guid[] newItemGuids) where T : class, IKeyGuid, new()
        {
            foreach (var roleGuid in newItemGuids)
            {
                T t = Attach<T>(roleGuid);
                existItems.Add(t);
            }
        }

        // 替换现有实体集合中的所有项
        // http://stackoverflow.com/questions/2789113/entity-framework-update-entity-along-with-child-entities-add-update-as-necessar
        protected void ReplaceEntities<T>(ICollection<T> existEntities, Guid[] newEntityGuids) where T : class, IKeyGuid, new()
        {
            if (newEntityGuids.Length == 0)
            {
                existEntities.Clear();
            }
            else
            {
                Guid[] tobeAdded = newEntityGuids.Except(existEntities.Select(x => x.Guid)).ToArray();
                Guid[] tobeRemoved = existEntities.Select(x => x.Guid).Except(newEntityGuids).ToArray();

                AddEntities<T>(existEntities, tobeAdded);

                existEntities.Where(x => tobeRemoved.Contains(x.Guid)).ToList().ForEach(e => existEntities.Remove(e));
            }
        }

        // http://patrickdesjardins.com/blog/validation-failed-for-one-or-more-entities-see-entityvalidationerrors-property-for-more-details-2
        // ((System.Data.Entity.Validation.DbEntityValidationException)$exception).EntityValidationErrors

        #endregion
    }
}

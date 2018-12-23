namespace Scottxu.Blog.Models.Exception
{
    /// <summary>
    /// 数据库不存在的异常
    /// </summary>
    class NotDataBaseException : System.Exception
    {
        public NotDataBaseException()
        {
        }
        
        public NotDataBaseException(string message) : base(message)
        {
        }
    }
}
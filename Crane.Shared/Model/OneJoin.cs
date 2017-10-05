namespace SprocMapperLibrary.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TJoin1"></typeparam>
    public class OneJoin<TResult, TJoin1>
    {
        /// <summary>
        /// 
        /// </summary>
        public TResult Result { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TJoin1 Join1 { get; set; }
    }
}

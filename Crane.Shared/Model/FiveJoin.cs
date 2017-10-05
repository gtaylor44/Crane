namespace SprocMapperLibrary.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TJoin1"></typeparam>
    /// <typeparam name="TJoin2"></typeparam>
    /// <typeparam name="TJoin3"></typeparam>
    /// <typeparam name="TJoin4"></typeparam>
    /// <typeparam name="TJoin5"></typeparam>
    public class FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>
    {
        /// <summary>
        /// 
        /// </summary>
        public TResult Result { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TJoin1 Join1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TJoin2 Join2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TJoin3 Join3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TJoin4 Join4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TJoin5 Join5 { get; set; }
    }
}

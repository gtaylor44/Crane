namespace SprocMapperLibrary.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TJoin1"></typeparam>
    /// <typeparam name="TJoin2"></typeparam>
    public class TwoJoin<TResult, TJoin1, TJoin2>
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
    }
}

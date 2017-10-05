namespace Crane.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TJoin1"></typeparam>
    /// <typeparam name="TJoin2"></typeparam>
    /// <typeparam name="TJoin3"></typeparam>
    public class ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>
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
    }
}

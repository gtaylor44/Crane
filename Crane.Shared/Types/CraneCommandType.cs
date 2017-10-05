namespace Crane.Shared.Types
{
    /// <summary>
    /// 
    /// </summary>
    public enum CraneCommandType
    {
        /// <summary>
        /// Determines if query is Text or Stored Procedure automatically. This is the default option. 
        /// </summary>
        Autodetect = 1,
        /// <summary>
        /// Execute a text based command
        /// </summary>
        Text = 2,
        /// <summary>
        /// Execute a stored procedure
        /// </summary>
        StoredProcedure = 3
    }
}

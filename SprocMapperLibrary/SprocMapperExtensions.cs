using System;
using System.Data;
using System.Data.SqlClient;

namespace SprocMapperLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public static class SprocMapperExtensions
    {
        /// <summary>
        /// Entry point for performing a select.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static Select Select(this IDbConnection conn)
        {
            return new Select();
        }

        /// <summary>
        /// Entry point for performing a procedure.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static Procedure Procedure(this IDbConnection conn)
        {
            return new Procedure();
        }

        /// <summary>
        /// Extension method for safely getting output parameters from SqlParameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlParameter"></param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this SqlParameter sqlParameter)
        {
            if (sqlParameter.Value == DBNull.Value)
            {
                if (typeof(T).IsValueType)
                    return (T) Activator.CreateInstance(typeof(T));

                return (default(T));
            }

            return (T) sqlParameter.Value;
        }
    }
}

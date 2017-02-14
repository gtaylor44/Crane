using System;

namespace SprocMapperLibrary.CustomException
{
    internal class SprocMapperException : Exception
    {
        public SprocMapperException(string message) : base(message)
        {

        }
    }
}

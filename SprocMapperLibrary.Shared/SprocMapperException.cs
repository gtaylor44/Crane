using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SprocMapperLibrary.UnitTests")]
[assembly: InternalsVisibleTo("SprocMapperLibrary.IntegrationTests")]
namespace SprocMapperLibrary
{
    internal class SprocMapperException : Exception
    {
        public SprocMapperException(string message) : base(message)
        {

        }
    }
}

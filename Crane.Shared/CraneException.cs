using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SprocMapperLibrary.UnitTests")]
[assembly: InternalsVisibleTo("SprocMapperLibrary.IntegrationTests")]
namespace SprocMapperLibrary
{
    internal class CraneException : Exception
    {
        public CraneException(string message) : base(message){}
    }
}

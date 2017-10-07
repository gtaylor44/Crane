using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Crane.UnitTests")]
[assembly: InternalsVisibleTo("Crane.IntegrationTests")]
namespace Crane
{
    internal class CraneException : Exception
    {
        public CraneException(string message) : base(message){}
    }
}

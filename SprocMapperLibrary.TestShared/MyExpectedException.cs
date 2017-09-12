using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SprocMapperLibrary.TestCommon
{
    public sealed class MyExpectedException : ExpectedExceptionBaseAttribute
    {
        private readonly Type _expectedExceptionType;
        private readonly string _expectedExceptionMessage;

        public MyExpectedException(Type expectedExceptionType)
        {
            _expectedExceptionType = expectedExceptionType;
            _expectedExceptionMessage = string.Empty;
        }

        public MyExpectedException(Type expectedExceptionType, string expectedExceptionMessage)
        {
            _expectedExceptionType = expectedExceptionType;
            _expectedExceptionMessage = expectedExceptionMessage;
        }

        protected override void Verify(Exception exception)
        {
            Assert.IsNotNull(exception);

            Assert.IsInstanceOfType(exception, _expectedExceptionType, "Wrong type of exception was thrown.");

            if (!_expectedExceptionMessage.Length.Equals(0))
            {
                Assert.AreEqual(_expectedExceptionMessage, exception.Message, "Wrong exception message was returned.");
            }
        }
    }
}

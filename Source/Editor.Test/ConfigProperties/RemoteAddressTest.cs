﻿// Copyright © 2020 Alex Leendertsen

using Editor.ConfigProperties;
using Editor.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Editor.Test.ConfigProperties
{
    [TestFixture]
    public class RemoteAddressTest
    {
        private RemoteAddress mSut;

        [SetUp]
        public void SetUp()
        {
            mSut = new RemoteAddress();
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("str", false)]
        [TestCase("-1", false)]
        [TestCase("65536", true)]
        [TestCase("1234", true)]
        [TestCase("1.2.3.4", true)]
        [TestCase("-1.2.3.4", false)]
        [TestCase("256.256.256.256", false)]
        public void TryValidate_ShouldReturnCorrectValue(string value, bool expectedResult)
        {
            mSut.Value = value;

            Assert.AreEqual(expectedResult, mSut.TryValidate(Substitute.For<IMessageBoxService>()));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("str")]
        [TestCase("-1")]
        [TestCase("-1.2.3.4")]
        [TestCase("256.256.256.256")]
        public void TryValidate_ShouldCallShowError(string value)
        {
            mSut.Value = value;

            IMessageBoxService messageBoxService = Substitute.For<IMessageBoxService>();
            mSut.TryValidate(messageBoxService);

            messageBoxService.Received(1).ShowError("Remote address must be a valid IP address.");
        }

        [Test]
        public void Name_ShouldBeCorrect()
        {
            Assert.AreEqual("Remote Address:", mSut.Name);
        }

        [Test]
        public void TryValidate_ShouldNotCallShowError()
        {
            mSut.Value = "1.2.3.4";

            IMessageBoxService messageBoxService = Substitute.For<IMessageBoxService>();
            mSut.TryValidate(messageBoxService);

            messageBoxService.DidNotReceive().ShowError(Arg.Any<string>());
        }
    }
}

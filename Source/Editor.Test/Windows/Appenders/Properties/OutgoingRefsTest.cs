﻿// Copyright © 2018 Alex Leendertsen

using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using Editor.Models;
using Editor.Windows.Appenders.Properties;
using Editor.Windows.PropertyCommon;
using NUnit.Framework;

namespace Editor.Test.Windows.Appenders.Properties
{
    [TestFixture]
    public class OutgoingRefsTest
    {
        private OutgoingRefs mSut;
        private XmlDocument mXmlDoc;

        [SetUp]
        public void SetUp()
        {
            mXmlDoc = new XmlDocument();
            mXmlDoc.LoadXml("<log4net>\n" +
                            "  <appender name=\"appender0\">\n" +
                            "    <appender-ref ref=\"appender1\" />\n" +
                            "    <appender-ref ref=\"appender2\" />\n" +
                            "  </appender>\n" +
                            "  <appender name=\"appender1\">\n" +
                            "    <appender-ref ref=\"appender2\" />\n" +
                            "  </appender>\n" +
                            "  <appender name=\"appender2\">\n" +
                            "  </appender>\n" +
                            "  <appender name=\"appender3\">\n" +
                            "  </appender>\n" +
                            "  <appender name=\"asyncAppender\" type=\"Log4Net.Async.AsyncForwardingAppender,Log4Net.Async\">\n" +
                            "  </appender>\n" +
                            "  <root>\n" +
                            "  </root>\n" +
                            "</log4net>");

            mSut = new OutgoingRefs(mXmlDoc.FirstChild, new ObservableCollection<IProperty>(), mXmlDoc.FirstChild["appender"]);
        }

        [Test]
        public void Name_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual("↑ Refs:", mSut.Name);
        }

        [Test]
        public void Description_ShouldBeInitializedCorrectly()
        {
            Assert.AreEqual("This element can reference the following appenders:", mSut.Description);
        }

        [Test]
        public void Ctor_ShouldLoadAvailableRefs()
        {
            //3 appenders + 1 async appender = 4
            Assert.AreEqual(4, mSut.RefsCollection.Count);
            Assert.AreEqual(4, mSut.RefsCollection.Count(r => !r.IsEnabled));
        }

        [Test]
        public void Load_ShouldLoadEnabledRefs()
        {
            mSut.Load(mXmlDoc.FirstChild["appender"]);

            Assert.AreEqual(4, mSut.RefsCollection.Count);
            Assert.AreEqual(2, mSut.RefsCollection.Count(r => r.IsEnabled));
        }

        [Test]
        public void Save_ShouldSaveEnabledRefs()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement appender = xmlDoc.CreateElement("appender");

            mSut.RefsCollection = new ObservableCollection<LoggerModel>
            {
                new LoggerModel("name0", appender, true),
                new LoggerModel("name1", appender, true),
                new LoggerModel("name2", appender, false)
            };

            mSut.Save(xmlDoc, appender);

            Assert.AreEqual(2, appender.ChildNodes.Count);
        }
    }
}

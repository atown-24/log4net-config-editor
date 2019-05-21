﻿// Copyright © 2018 Alex Leendertsen

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using SystemInterface.Xml;
using SystemWrapper.Xml;
using Editor.Descriptors;
using Editor.Enums;
using Editor.Interfaces;
using Editor.Models;
using Editor.Models.ConfigChildren;
using Editor.Utilities;
using Editor.XML;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace Editor.Test.XML
{
    [TestFixture]
    public class ConfigurationXmlTest
    {
        private IXmlDocument mXmlDoc;
        private IMessageBoxService mMessageBoxService;
        private ICanLoadAndSaveXml mLoadAndSave;
        private ConfigurationXml mSut;

        [SetUp]
        public void SetUp()
        {
            mMessageBoxService = Substitute.For<IMessageBoxService>();

            mXmlDoc = new XmlDocumentWrap();
            mXmlDoc.Load(GetTestConfigXml());

            mLoadAndSave = Substitute.For<ICanLoadAndSaveXml>();
            mLoadAndSave.Load().Returns(mXmlDoc);

            mSut = new ConfigurationXml(mMessageBoxService,
                                        mLoadAndSave, 
                                        Substitute.For<IAppenderFactory>());
        }

        /// <summary>
        /// Retrieves a path to the test-config.xml file.
        /// </summary>
        /// <returns></returns>
        private static string GetTestConfigXml()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"XML\test-config.xml");
        }

        [Test]
        public void Ctor_ShouldThrow_WhenMessageBoxServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationXml(null, Substitute.For<ICanLoadAndSaveXml>(), Substitute.For<IAppenderFactory>()));
        }

        [Test]
        public void Ctor_ShouldThrow_WhenLoadSaveIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationXml(mMessageBoxService, null, Substitute.For<IAppenderFactory>()));
        }

        [Test]
        public void Debug_ShouldBeInitializedToFalse()
        {
            Assert.IsFalse(mSut.Debug);
        }

        [Test]
        public void Update_ShouldBeInitializedToMerge()
        {
            Assert.AreEqual(Update.Merge, mSut.Update);
        }

        [Test]
        public void Threshold_ShouldBeInitializedToAll()
        {
            Assert.AreEqual(Level.All, mSut.Threshold);
        }

        [Test]
        public void Load_ShouldLoadEverything()
        {
            //Reload is actually the method that loads everything, which should be called within Load
            //Let's just gather all the tests for Reload and run them
            IEnumerable<MethodInfo> reloadMethods = GetType().GetMethods().Where(method => method.Name.StartsWith(nameof(ConfigurationXml.Reload)));

            CollectionAssert.IsNotEmpty(reloadMethods);

            foreach (MethodInfo methodInfo in reloadMethods)
            {
                SetUp();
                methodInfo.Invoke(this, null);
            }
        }

        [Test]
        public void Load_ShouldShowWarning_ForUnknownAppender()
        {
            mXmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><log4net><appender></appender></log4net>");

            mSut.Load();

            mMessageBoxService.Received(1).ShowWarning("At least one unrecognized appender was found in this configuration.");
        }

        [Test]
        public void Reload_ShouldThrow_WhenNotLoadedFirst()
        {
            Assert.Throws<InvalidOperationException>(() => mSut.Reload());
        }

        [Test]
        public void Reload_ShouldShowError_WhenNoLog4NetElements()
        {
            mXmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><root></root>");

            mSut.Load();
            mMessageBoxService.ClearReceivedCalls();

            mSut.Reload();

            mMessageBoxService.Received(1).ShowError("Could not find log4net configuration.");
        }

        [Test]
        public void Reload_ShouldShowWarning_WhenMoreThanOneLog4NetElement()
        {
            mXmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><root><log4net></log4net><log4net></log4net></root>");

            mSut.Load();
            mMessageBoxService.ClearReceivedCalls();

            mSut.Reload();

            mMessageBoxService.Received(1).ShowWarning("More than one 'log4net' element was found in the specified file. Using the first occurrence.");
        }

        [Test]
        public void Reload_ShouldChooseFirstLog4NetElement()
        {
            mXmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><root><log4net></log4net><log4net></log4net></root>");

            mSut.Load();
            mMessageBoxService.ClearReceivedCalls();

            mSut.Reload();

            Assert.AreSame(mXmlDoc.DocumentElement.FirstChild, mSut.Log4NetNode);
        }

        [Test]
        public void Reload_ShouldLoadAppenders()
        {
            mSut.Load();
            mSut.Reload();

            Assert.AreEqual(2, mSut.Children.OfType<AppenderModel>().Count());
            Assert.AreEqual(1, mSut.Children.OfType<AsyncAppenderModel>().Count());
            Assert.AreEqual(1, mSut.Children.OfType<AsyncAppenderModel>().First().IncomingReferences);
        }

        [Test]
        public void Reload_ShouldLoadRenderers()
        {
            mSut.Load();
            mSut.Reload();

            Assert.AreEqual(1, mSut.Children.OfType<RendererModel>().Count());
        }

        [Test]
        public void Reload_ShouldLoadParams()
        {
            mSut.Load();
            mSut.Reload();

            Assert.AreEqual(1, mSut.Children.OfType<ParamModel>().Count());
        }

        [Test]
        public void Reload_ShouldLoadLoggers()
        {
            mSut.Load();
            mSut.Reload();

            Assert.AreEqual(1, mSut.Children.OfType<LoggerModel>().Count());
            Assert.AreEqual(1, mSut.Children.OfType<RootLoggerModel>().Count());
        }

        [Test]
        public void Reload_ShouldLoadAppenderRefLocations()
        {
            mSut.Load();
            mSut.Reload();

            Assert.AreEqual(2, mSut.Children.OfType<IAcceptAppenderRef>().Count());
        }

        [Test]
        public void Reload_ShouldLoadAttributes()
        {
            mSut.Load();
            mSut.Reload();

            Assert.IsTrue(mSut.Debug);
            Assert.AreEqual(Update.Overwrite, mSut.Update);
            Assert.AreEqual(Level.Error, mSut.Threshold);
        }

        [Test]
        public void Reload_ShouldLoadDefaultAttributes_WhenNonExistent()
        {
            mXmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><log4net></log4net>");
            mSut.Load();
            mSut.Reload();

            Assert.IsFalse(mSut.Debug);
            Assert.AreEqual(Update.Merge, mSut.Update);
            Assert.AreEqual(Level.All, mSut.Threshold);
        }

        [Test]
        public void Save_ShouldSaveAttributes()
        {
            mSut.Load();

            //Load will set these all to non-defaults (they're all non-default in test-config.xml)
            //Let's set them back to default to make sure the attributes are removed
            mSut.Debug = false;
            mSut.Update = Update.Merge;
            mSut.Threshold = Level.All;

            mSut.Save();

            XmlNode log4NetNode = mXmlDoc.DocumentElement.FirstChild;

            Assert.IsNull(log4NetNode.Attributes[Log4NetXmlConstants.Debug]);
            Assert.IsNull(log4NetNode.Attributes[Log4NetXmlConstants.Update]);
            Assert.IsNull(log4NetNode.Attributes[Log4NetXmlConstants.Threshold]);
        }

        [Test]
        public void Save_ShouldSave()
        {
            mSut.Load();

            mSut.Save();

            mLoadAndSave.Received(1).Save();
        }

        [Test]
        public void RemoveRefsTo_ShouldRemoveRefsToAppender()
        {
            mSut.Load();

            AsyncAppenderModel appender = mSut.Children.OfType<AsyncAppenderModel>().First();
            mSut.RemoveRefsTo(appender);

            CollectionAssert.IsEmpty(XmlUtilities.FindAppenderRefs(mXmlDoc.DocumentElement.FirstChild, appender.Name));
        }

        [Test]
        public void RemoveChild_ShouldRemoveChildFromXml()
        {
            mSut.Load();

            AsyncAppenderModel appender = mSut.Children.OfType<AsyncAppenderModel>().First();
            mSut.RemoveChild(appender);

            CollectionAssert.IsEmpty(mXmlDoc.DocumentElement.FirstChild.SelectNodes($"//appender[@name='{appender.Name}']"));
        }

        [Test]
        public void RemoveChild_ShouldRemoveChildFromCollection()
        {
            mSut.Load();

            AsyncAppenderModel appender = mSut.Children.OfType<AsyncAppenderModel>().First();
            mSut.RemoveChild(appender);

            CollectionAssert.IsEmpty(mSut.Children.OfType<AsyncAppenderModel>());
        }

        [Test]
        public void RemoveChild_ShouldRemoveReferencesToChild()
        {
            mSut.Load();

            AsyncAppenderModel appender = mSut.Children.OfType<AsyncAppenderModel>().First();
            mSut.RemoveChild(appender);

            CollectionAssert.IsEmpty(mXmlDoc.DocumentElement.FirstChild.SelectNodes($"//appender-ref[@ref='{appender.Name}']"));
        }

        [Test]
        public void RemoveChild_ShouldOnlyRemoveRefsToAppenders()
        {
            mSut.Load();

            //RemoveChild also removes refs to the specified child, if it's an appender.
            //If it's not an appender, it should return silently.
            Assert.DoesNotThrow(() => mSut.Children.OfType<LoggerModel>().First());
        }

        [Test]
        public void ConfigXml_ShouldReturnUnderlyingConfig()
        {
            mSut.Load();

            Assert.AreSame(((XmlDocumentWrap)mXmlDoc).XmlDocumentInstance, mSut.ConfigXml);
        }

        [Test]
        public void CreateElementConfigurationFor_ShouldCreateElementConfigWithCorrectProperties_WhenModelIsNull()
        {
            mSut.Load();

            IElementConfiguration elementConfiguration = mSut.CreateElementConfigurationFor(null, AppenderDescriptor.Async.ElementName);

            Assert.IsNull(elementConfiguration.OriginalNode);
            Assert.AreEqual(AppenderDescriptor.Async.ElementName, elementConfiguration.NewNode.Name);
            Assert.AreSame(mSut.ConfigXml, elementConfiguration.ConfigXml);
            Assert.AreSame(mSut.Log4NetNode, elementConfiguration.Log4NetNode);
        }

        [Test]
        public void CreateElementConfigurationFor_ShouldCreateElementConfigWithCorrectProperties()
        {
            mSut.Load();

            AsyncAppenderModel originalModel = mSut.Children.OfType<AsyncAppenderModel>().First();
            IElementConfiguration elementConfiguration = mSut.CreateElementConfigurationFor(originalModel, AppenderDescriptor.Async.ElementName);

            Assert.AreSame(originalModel.Node, elementConfiguration.OriginalNode);
            Assert.AreEqual(AppenderDescriptor.Async.ElementName, elementConfiguration.NewNode.Name);
            Assert.AreSame(mSut.ConfigXml, elementConfiguration.ConfigXml);
            Assert.AreSame(mSut.Log4NetNode, elementConfiguration.Log4NetNode);
        }
    }
}

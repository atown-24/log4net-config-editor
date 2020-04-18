// Copyright © 2020 Alex Leendertsen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Editor.Interfaces;
using Editor.Utilities;

namespace Editor.XML
{
    //TODO two implementations: one for a new element (no original node) and one for an existing one (original and new nodes)
    internal class ElementConfiguration : IElementConfiguration
    {
        public ElementConfiguration(XmlDocument xmlDocument, XmlNode log4NetNode, XmlNode originalNode, XmlNode newNode)
        {
            ConfigXml = xmlDocument;
            Log4NetNode = log4NetNode;
            OriginalNode = originalNode;
            NewNode = newNode;
        }

        public ElementConfiguration(IConfiguration configuration, XmlNode originalNode, XmlNode newNode)
            : this(configuration.ConfigXml, configuration.Log4NetNode, originalNode, newNode)
        {
        }

        public XmlNode OriginalNode { get; }

        public bool Load(string attributeName, out IValueResult result, params string[] childElementNames)
        {
            XmlNode child = OriginalNode;
            IList<string> actualNames = new List<string>();

            if (childElementNames.Length > 0)
            {
                child = OriginalNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => string.Equals(n.LocalName, childElementNames.First(), StringComparison.OrdinalIgnoreCase));

                if (child != null)
                {
                    actualNames.Add(child.LocalName);
                }

                foreach (string childElementName in childElementNames.Skip(1))
                {
                    child = child?.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => string.Equals(n.LocalName, childElementName, StringComparison.OrdinalIgnoreCase));

                    if (child == null)
                    {
                        break;
                    }

                    actualNames.Add(child.LocalName);
                }
            }

            //Find the attribute on the element
            XmlAttribute attr = child?.Attributes?.Cast<XmlAttribute>().FirstOrDefault(a => string.Equals(a.LocalName, attributeName, StringComparison.OrdinalIgnoreCase));

            if (attr == null)
            {
                result = null;
                return false;
            }

            result = new ValueResult(actualNames, attr.LocalName, attr.Value);
            return true;
        }

        public XmlNode NewNode { get; }

        public void Save(string elementName, string attributeName, string value)
        {
            ConfigXml.CreateElementWithAttribute(elementName, attributeName, value).AppendTo(NewNode);
        }

        public void Save(string attributeName, string value)
        {
            NewNode.AppendAttribute(ConfigXml, attributeName, value);
        }

        public XmlDocument ConfigXml { get; }

        public XmlNode Log4NetNode { get; }

        private class ValueResult : IValueResult
        {
            public ValueResult(IEnumerable<string> actualElementNames, string actualAttributeName, string attributeValue)
            {
                ActualElementNames = actualElementNames;
                ActualAttributeName = actualAttributeName;
                AttributeValue = attributeValue;
            }

            public IEnumerable<string> ActualElementNames { get; }

            public string ActualAttributeName { get; }

            public string AttributeValue { get; }
        }
    }
}

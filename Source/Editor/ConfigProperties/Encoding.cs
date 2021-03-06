﻿// Copyright © 2019 Alex Leendertsen

using System.Linq;
using System.Windows;
using System.Xml;
using Editor.ConfigProperties.Base;
using Editor.Utilities;

namespace Editor.ConfigProperties
{
    internal class Encoding : MultiValuePropertyBase<string>
    {
        private static readonly string[] sValues =
        {
            string.Empty,
            System.Text.Encoding.ASCII.WebName,
            System.Text.Encoding.Unicode.WebName,
            System.Text.Encoding.BigEndianUnicode.WebName,
            System.Text.Encoding.UTF7.WebName,
            System.Text.Encoding.UTF8.WebName,
            System.Text.Encoding.UTF32.WebName
        };

        private readonly string mElementName;

        public Encoding(string name, string elementName)
            : base(GridLength.Auto, name, sValues, 75)
        {
            SelectedValue = sValues[0];
            mElementName = elementName;
        }

        public override void Load(XmlNode originalNode)
        {
            string valueStr = originalNode.GetValueAttributeValueFromChildElement(mElementName);
            if (sValues.Contains(valueStr))
            {
                SelectedValue = valueStr;
            }
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            if (!string.IsNullOrEmpty(SelectedValue))
            {
                xmlDoc.CreateElementWithValueAttribute(mElementName, SelectedValue).AppendTo(newNode);
            }
        }
    }
}

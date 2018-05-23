﻿// Copyright © 2018 Alex Leendertsen

using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;
using Editor.Utilities;
using Editor.Windows.PropertyCommon;

namespace Editor.Windows.Appenders.Properties
{
    public class MaxSizeRollBackups : StringValueProperty
    {
        private const string MaxSizeRollBackupsName = "maxSizeRollBackups";

        public MaxSizeRollBackups(ObservableCollection<IProperty> container)
            : base(container, GridLength.Auto, "Max Size Roll Backups:")
        {
            ToolTip = "The maximum number of backup files that are kept before the oldest is erased.";
        }

        public override void Load(XmlNode originalNode)
        {
            SetValueIfNotNullOrEmpty(originalNode.GetValueAttributeValueFromChildElement(MaxSizeRollBackupsName));
        }

        public override bool TryValidate(IMessageBoxService messageBoxService)
        {
            if (!int.TryParse(Value, out int _))
            {
                MessageBox.Show("Max size roll backups must be a valid integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return base.TryValidate(messageBoxService);
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            xmlDoc.CreateElementWithValueAttribute(MaxSizeRollBackupsName, Value).AppendTo(newNode);
        }
    }
}

﻿// Copyright © 2018 Alex Leendertsen

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Editor.Descriptors;
using Editor.Enums;
using Editor.Models;
using Editor.Utilities;
using Editor.Windows.Filters;
using Editor.Windows.PropertyCommon;

namespace Editor.Windows.Appenders.Properties
{
    public class Filters : PropertyBase
    {
        private readonly XmlDocument mXmlDoc;
        private readonly XmlNode mAppenderNode;
        private readonly IMessageBoxService mMessageBoxService;

        public Filters(XmlDocument xmlDoc, XmlNode appenderNode, ObservableCollection<IProperty> container, IMessageBoxService messageBoxService)
            : base(container, new GridLength(1, GridUnitType.Star))
        {
            mXmlDoc = xmlDoc;
            mAppenderNode = appenderNode;
            mMessageBoxService = messageBoxService;

            AvailableFilters = new[]
            {
                FilterDescriptor.DenyAll,
                FilterDescriptor.LevelMatch,
                FilterDescriptor.LevelRange,
                FilterDescriptor.LoggerMatch,
                FilterDescriptor.String,
                FilterDescriptor.Property
            };

            ExistingFilters = new ObservableCollection<FilterModel>();

            AddFilter = new Command(AddFilterOnClick);
            Help = new Command(HelpOnClick);
        }

        public IEnumerable<FilterDescriptor> AvailableFilters { get; }

        public ObservableCollection<FilterModel> ExistingFilters { get; }

        public ICommand AddFilter { get; }

        public ICommand Help { get; }

        private void AddFilterOnClick(object filter)
        {
            FilterDescriptor descriptor = (FilterDescriptor)filter;

            if (descriptor != FilterDescriptor.DenyAll)
            {
                ShowFilterWindow(new FilterModel(descriptor, null, ShowFilterWindow, Remove, MoveUp, MoveDown));
            }
            else
            {
                XmlElement filterElement = mXmlDoc.CreateElementWithAttribute("filter", "type", FilterDescriptor.DenyAll.TypeNamespace);
                Add(new FilterModel(FilterDescriptor.DenyAll, filterElement, ShowFilterWindow, Remove, MoveUp, MoveDown));
            }
        }

        private void HelpOnClick()
        {
            mMessageBoxService.ShowInformation("Filters form a chain that the event has to pass through. Any filter along the way can accept the event and stop processing, deny the event and stop processing, or allow the event on to the next filter. If the event gets to the end of the filter chain without being denied it is implicitly accepted and will be logged. To reorder filters, click either '↑' or '↓'.");
        }

        private void ShowFilterWindow(FilterModel filterModel)
        {
            Window filterWindow;

            switch (filterModel.Descriptor.Type)
            {
                case FilterType.LevelMatch:
                    filterWindow = new LevelMatchFilterWindow(filterModel, mAppenderNode, mXmlDoc, Add);
                    break;
                case FilterType.LevelRange:
                    filterWindow = new LevelRangeFilterWindow(filterModel, mAppenderNode, mXmlDoc, Add);
                    break;
                case FilterType.LoggerMatch:
                    filterWindow = new LoggerMatchFilterWindow(filterModel, mAppenderNode, mXmlDoc, Add);
                    break;
                case FilterType.Property:
                    filterWindow = new PropertyFilterWindow(filterModel, mAppenderNode, mXmlDoc, Add);
                    break;
                case FilterType.String:
                    filterWindow = new StringMatchFilterWindow(filterModel, mAppenderNode, mXmlDoc, Add);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            mMessageBoxService.ShowWindow(filterWindow);
        }

        private void Add(FilterModel filterModel)
        {
            ExistingFilters.Add(filterModel);
        }

        private void Remove(FilterModel filterModel)
        {
            ExistingFilters.Remove(filterModel);
        }

        private void MoveUp(FilterModel filterModel)
        {
            int oldIndex = ExistingFilters.IndexOf(filterModel);
            int newIndex = oldIndex == 0 ? 0 : oldIndex - 1;
            ExistingFilters.Move(oldIndex, newIndex);
        }

        private void MoveDown(FilterModel filterModel)
        {
            int oldIndex = ExistingFilters.IndexOf(filterModel);
            int newIndex = oldIndex == ExistingFilters.Count - 1 ? ExistingFilters.Count - 1 : oldIndex + 1;
            ExistingFilters.Move(oldIndex, newIndex);
        }

        public override void Load(XmlNode originalNode)
        {
            XmlNodeList filterNodes = originalNode.SelectNodes("filter");

            if (filterNodes == null)
            {
                return;
            }

            foreach (XmlNode filterNode in filterNodes)
            {
                if (FilterDescriptor.TryFindByTypeNamespace(filterNode.Attributes?["type"]?.Value, out FilterDescriptor filterDescriptor))
                {
                    Add(new FilterModel(filterDescriptor, filterNode, ShowFilterWindow, Remove, MoveUp, MoveDown));
                }
            }
        }

        public override void Save(XmlDocument xmlDoc, XmlNode newNode)
        {
            foreach (FilterModel filter in ExistingFilters)
            {
                newNode.AppendChild(filter.Node);
            }
        }
    }
}

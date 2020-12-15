﻿// Copyright © 2020 Alex Leendertsen

using System.Xml;

namespace Editor.Interfaces
{
    /// <summary>
    /// Encapsulates two nodes to facilitate the loading and saving of new elements.
    /// Typically, the UI is loaded with the <see cref="OriginalNode"/> and values
    /// are saved to the <see cref="NewNode"/>.
    /// </summary>
    public interface IElementConfiguration : IConfiguration
    {
        /// <summary>
        /// The original node, if editing. If not, this is null.
        /// </summary>
        XmlNode OriginalNode { get; }

        /// <summary>
        /// Attempts to load the specified attribute value from the specified child element of the original node (appender).
        /// Case insensitive. Should not be called if no original node exists.
        /// </summary>
        /// <param name="attributeName">Name of the desired attribute</param>
        /// <param name="result">The value of the desired attribute as well as it's original name</param>
        /// <param name="childElementNames">
        /// Hierarchical child elements (each value is a child of the previous), where the last child holds the desired attribute.
        /// If no child elements are specified, attribute name is loaded from original node.
        /// </param>
        /// <returns>False if not found, true otherwise</returns>
        bool Load(string attributeName, out IValueResult result, params string[] childElementNames);

        /// <summary>
        /// The new node that will replace the original (if it exists).
        /// </summary>
        XmlNode NewNode { get; }

        /// <summary>
        /// Saves the specified value as an attribute with the specified name to the new node (the root node - not a child).
        /// </summary>
        /// <param name="attributeName">Name of the desired attribute</param>
        /// <param name="value">Attribute value</param>
        void Save(string attributeName, string value);

        /// <summary>
        /// Saves the specified children along with their corresponding attributes to the new node.
        /// Child elements with attributes are automatically created and appended.
        /// </summary>
        /// <param name="children">Set of children complete with attributes and values to be added to the new node</param>
        void Save(params (string ElementName, string AttributeName, string AttributeValue)[] children);
    }
}

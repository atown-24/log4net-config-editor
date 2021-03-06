// Copyright © 2018 Alex Leendertsen

using System.Xml;
using Editor.Descriptors;

namespace Editor.Models
{
    public class AppenderModel : ChildModel
    {
        public AppenderModel(AppenderDescriptor descriptor, XmlNode appender, string name, int incomingReferences)
            : base(descriptor.ElementName, appender)
        {
            Descriptor = descriptor;
            Name = name;
            IncomingReferences = incomingReferences;
        }

        public AppenderDescriptor Descriptor { get; }

        public string Name { get; }

        public int IncomingReferences { get; }
    }
}

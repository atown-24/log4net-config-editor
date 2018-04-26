﻿// Copyright © 2018 Alex Leendertsen

using System.Collections.Generic;
using Editor.Enums;

namespace Editor.Descriptors
{
    public class FilterDescriptor
    {
        public static readonly FilterDescriptor DenyAll, LevelMatch, LevelRange, LoggerMatch, Mdc, Ndc, Property, String;
        private static readonly IDictionary<string, FilterDescriptor> sDescriptorsByTypeNamespace;

        static FilterDescriptor()
        {
            DenyAll = new FilterDescriptor("Deny All", FilterType.DenyAll, "log4net.Filter.DenyAllFilter");
            LevelMatch = new FilterDescriptor("Level Match", FilterType.LevelMatch, "log4net.Filter.LevelMatchFilter");
            LevelRange = new FilterDescriptor("Level Range", FilterType.LevelRange, "log4net.Filter.LevelRangeFilter");
            LoggerMatch = new FilterDescriptor("Logger Match", FilterType.LoggerMatch, "log4net.Filter.LoggerMatchFilter");
            Mdc = new FilterDescriptor("MDC", FilterType.Mdc, "log4net.Filter.MdcFilter");
            Ndc = new FilterDescriptor("NDC", FilterType.Ndc, "log4net.Filter.NdcFilter");
            Property = new FilterDescriptor("Property", FilterType.Property, "log4net.Filter.PropertyFilter");
            String = new FilterDescriptor("String", FilterType.String, "log4net.Filter.StringMatchFilter");

            sDescriptorsByTypeNamespace = new Dictionary<string, FilterDescriptor>
            {
                { DenyAll.TypeNamespace, DenyAll },
                { LevelMatch.TypeNamespace, LevelMatch },
                { LevelRange.TypeNamespace, LevelRange },
                { LoggerMatch.TypeNamespace, LoggerMatch },
                { Mdc.TypeNamespace, Mdc },
                { Ndc.TypeNamespace, Ndc },
                { Property.TypeNamespace, Property },
                { String.TypeNamespace, String }
            };
        }

        private FilterDescriptor(string name, FilterType type, string typeNamespace)
        {
            Name = name;
            Type = type;
            TypeNamespace = typeNamespace;
        }

        public static bool TryFindByTypeNamespace(string typeNamespace, out FilterDescriptor filter)
        {
            if (typeNamespace == null)
            {
                filter = null;
                return false;
            }

            return sDescriptorsByTypeNamespace.TryGetValue(typeNamespace, out filter);
        }

        public string Name { get; }

        public FilterType Type { get; }

        public string TypeNamespace { get; }
    }
}

//Class	            Description	                                                Properties
//-                 -                                                           -
//DenyAllFilter	    This filter drops all LoggingEvent.	
//LevelMatchFilter	This is a very simple filter based on Level matching.	    AcceptOnMatch, LevelToMatch
//LevelRangeFilter	This is a simple filter based on Level matching.	        AcceptOnMatch, LevelMax, LevelMin
//LoggerMatchFilter	Simple filter to match a string in the event's logger name.	AcceptOnMatch, LoggerToMatch
//MdcFilter	        Simple filter to match a keyed string in the MDC	        AcceptOnMatch, Key, RegexToMatch, StringTo Match
//NdcFilter	        Simple filter to match a string in the NDC	                AcceptOnMatch, Key, RegexToMatch, StringTo Match
//PropertyFilter	Simple filter to match a string an event property	        AcceptOnMatch, Key, RegexToMatch, StringTo Match
//StringMatchFilter	Simple filter to match a string in the rendered message	    AcceptOnMatch, RegexToMatch, StringTo Match

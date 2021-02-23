﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OneOneCode.Core.Diagnostic.Resolver
{
    public class ObjectAttribute : ParameterResolverAttribute
    {
        public Type TargetType { get; set; }

        public override object Resolve(object value)
        {
            if (TargetType == null || value == null)
            {
                return value;
            }

            if (TargetType == value.GetType())
            {
                return value;
            }

            if (TargetType.IsInstanceOfType(value))
            {
                return value;
            }

            return null;
        }
    }
}

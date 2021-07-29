// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Loader;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.DotNet.HotReload.Utils.Generator.EnC
{
    //
    // Inspired by https://github.com/dotnet/roslyn/issues/8962
    public class ChangeMaker {

        private const string csharpCodeAnalysisAssemblyName = "Microsoft.CodeAnalysis.CSharp.Features";
        private const string codeAnalysisFeaturesAssemblyName = "Microsoft.CodeAnalysis.Features";

        private const string capabilitiesTypeName = "Microsoft.CodeAnalysis.EditAndContinue.EditAndContinueCapabilities";

        struct Reflected {
            internal readonly Type _capabilities {get; init;}

        }

        private readonly Reflected _reflected;

        public Type EditAncContinueCapabilitiesType => _reflected._capabilities;

        public ChangeMaker () {
            _reflected = ReflectionInit();
        }
        // Get all the Roslyn stuff we need
        private static Reflected ReflectionInit ()
        {
            var an = new AssemblyName (csharpCodeAnalysisAssemblyName);
            var assm = AssemblyLoadContext.Default.LoadFromAssemblyName(an)!;

            an = new AssemblyName(codeAnalysisFeaturesAssemblyName);
            assm = AssemblyLoadContext.Default.LoadFromAssemblyName(an);

            var caps = assm.GetType (capabilitiesTypeName);

            if (caps == null) {
                throw new Exception ("Couldn't find EditAndContinueCapabilities type");
            }

            return new Reflected() { _capabilities =  caps,
                                    };
        }

        /// Convert my EditAndContinueCapabilities enum value to
        ///  [Microsoft.CodeAnalysis.Features]Microsoft.CodeAnalysis.EditAndContinue.EditAndContinueCapabilities
        public object ConvertCapabilities (EditAndContinueCapabilities myCaps)
        {
            int i = (int)myCaps;
            object theirCaps = Enum.ToObject(_reflected._capabilities, i);
            return theirCaps;
        }
    }
}

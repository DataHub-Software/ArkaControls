// Copyright (c) 2026 DataHub Software
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;

namespace ArkaControls
{
    /// <summary>
    /// Provenance of this library. Show it in an "About" or "Open-source notices" screen to satisfy the
    /// attribution requirement (Apache-2.0 Section 4 and the NOTICE file).
    /// </summary>
    public static class ArkaAbout
    {
        public const string Name = "ArkaControls";
        public const string Owner = "DataHub Software";
        public const string Repository = "https://github.com/DataHub-Software/ArkaControls";
        public const string License = "Apache-2.0";
        public const string Copyright = "Copyright (c) 2026 DataHub Software";

        /// <summary>Ready-made attribution line for an About dialog.</summary>
        public static string Notice => Name + " - " + Copyright + " - " + License + " - " + Repository;

        public static string Version =>
            typeof(ArkaAbout).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(ArkaAbout).Assembly.GetName().Version?.ToString() ?? "unknown";
    }
}

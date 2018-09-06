﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.TemplateEngine.Abstractions;
using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Locations;
using Microsoft.Templates.Fakes;
using Microsoft.Templates.UI;

namespace Microsoft.Templates.Test
{
    public sealed class StyleCopGenerationTestsFixture : BaseGenAndBuildFixture, IDisposable
    {
        private string _testExecutionTimeStamp = DateTime.Now.FormatAsDateHoursMinutes();

        private static bool syncExecuted = false;

        public override string GetTestRunPath() => $"{Path.GetPathRoot(Environment.CurrentDirectory)}\\UIT\\SC\\{_testExecutionTimeStamp}\\";

        public TemplatesSource Source => new LocalTemplatesSource("StyleCop");

        [SuppressMessage(
         "Usage",
         "VSTHRD002:Synchronously waiting on tasks or awaiters may cause deadlocks",
         Justification = "Required for unit testing.")]
        private static void InitializeTemplates(TemplatesSource source)
        {
            GenContext.Bootstrap(source, new FakeGenShell(Platforms.Uwp, ProgrammingLanguages.CSharp), ProgrammingLanguages.CSharp);

            if (!syncExecuted == true)
            {
                GenContext.ToolBox.Repo.SynchronizeAsync(true).Wait();
                syncExecuted = true;
            }
        }

        public override void InitializeFixture(IContextProvider contextProvider, string framework = "")
        {
            GenContext.Current = contextProvider;

            InitializeTemplates(Source);
        }

        public static IEnumerable<object[]> GetProjectTemplatesForStyleCop()
        {
            InitializeTemplates(new LocalTemplatesSource("StyleCop"));

            List<object[]> result = new List<object[]>();

            // foreach (var platform in Platforms.GetAllPlatforms())
            var platform = Platforms.Uwp;
            var templateProjectTypes = GenComposer.GetSupportedProjectTypes(platform);

            var projectTypes = GenContext.ToolBox.Repo.GetProjectTypes(platform)
                        .Where(m => templateProjectTypes.Contains(m.Name) && !string.IsNullOrEmpty(m.Description))
                        .Select(m => m.Name);

            foreach (var projectType in projectTypes)
            {
                var projectFrameworks = GenComposer.GetSupportedFx(projectType, platform);

                var targetFrameworks = GenContext.ToolBox.Repo.GetFrameworks(platform)
                                            .Where(m => projectFrameworks.Contains(m.Name))
                                            .Select(m => m.Name).ToList();

                foreach (var framework in targetFrameworks)
                {
                    result.Add(new object[] { projectType, framework, platform });
                }
            }

            return result;
        }
    }
}

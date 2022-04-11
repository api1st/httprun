using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using HttpExecutor.Abstractions;
using HttpExecutor.Ioc;
using HttpExecutor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HttpExecutor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var commandLineParser = new CommandLine.Parser(settings => settings.HelpWriter = null);

            var options = commandLineParser.ParseArguments<AppOptions>(args);
            
            options
                .WithParsed(opts => MainAsync(opts).GetAwaiter().GetResult())
                .WithNotParsed(errors =>
                {
                    var helptext = HelpText.AutoBuild(options);
                    helptext.Heading = new HeadingInfo("httprun", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    helptext.Copyright = $"Copyright (C) {DateTime.Now.Year} Apiway Ltd";
                    helptext.AutoHelp = false;
                    System.Console.WriteLine(helptext.ToString());
                });
        }

        private static string RenderWarning(Warning warning)
        {
            switch (warning.Type)
            {
                case WarningType.DuplicateVariableDeclaration:
                    return $"Redefinition of variable @{warning.Name}.";

                case WarningType.VariableResolutionFailure:
                    return $"Unresolved variable {warning.Name}";
            }

            return String.Empty;
        }

        private static async Task MainAsync(IAppOptions options)
        {
            var services = new ServiceCollection();
            services.AddSingleton(options);
            services.AddExecutorServices();
            services.AddTransient<IConsole, ConsoleWrapper>();
            services.AddTransient<IEnvironment, EnvironmentWrapper>();
            services.AddTransient<ISleeper, ThreadWrapper>();
            services.AddTransient<ITextFileLineReader, TextFileLineReader>();
            services.AddTransient<IDateTimeNowProvider, DateTimeNowProvider>();
            
            var provider = services.BuildServiceProvider();

            var lineReader = provider.GetRequiredService<ITextFileLineReader>();
            var console = provider.GetRequiredService<IConsole>();
            var environment = provider.GetRequiredService<IEnvironment>();

            if (!System.IO.File.Exists(options.Filename))
            {
                console.WriteLine($"Script file not found.");
                environment.Exit(-1);
            }

            var lines = await lineReader.ReadAllLinesAsync(options.Filename);

            // TODO - abstract this
            var parentDirectory = System.IO.Directory.GetParent(options.Filename);
            System.IO.Directory.SetCurrentDirectory(parentDirectory.FullName);
            
            var parser = provider.GetRequiredService<IParser>();
            var executor = provider.GetRequiredService<IBlockExecutor>();
            var thread = provider.GetRequiredService<ISleeper>();

            var httpFile = parser.Parse(lines);

            var exitCode = 0;
            var scriptWarnings = new List<string>();

            foreach (var block in httpFile.Blocks)
            {
                var (warnings, request, response) = await executor.ExecuteAsync(block);

                if (warnings != null && warnings.Any())
                {
                    console.WriteLine("# ----------- WARNING -------------");
                    foreach (var warning in warnings)
                    {
                        console.WriteLine(RenderWarning(warning));
                    }

                    console.WriteLine();
                }

                if (request != null)
                {
                    console.WriteLine(request.ToString());
                    console.WriteLine();
                }

                if (options.TerminateOnVariableResolutionFailure &&
                    warnings.Any(x => x.Type == WarningType.VariableResolutionFailure))
                {
                    console.WriteLine($"Variable resolution failure starting at line {block.Lines.First().LineNumber}, terminating script execution.");
                    exitCode = -1;
                    break;
                }

                if (response != null)
                {
                    console.WriteLine(response.ToString());
                    console.WriteLine();
                }

                console.WriteLine("###");
                console.WriteLine();

                if (response != null)
                {
                    if (response.StatusCode == -1)
                    {
                        // No response received, terminate execution.
                        console.WriteLine($"Invalid response received from request starting at line {block.Lines.First().LineNumber}, terminating script execution.");
                        exitCode = -1;
                        break;
                    }

                    if (!options.SuccessCodes.Contains(response.StatusCode))
                    {
                        console.WriteLine($"Non-success response status code: {response.StatusCode}. At line {block.Lines.First().LineNumber}. terminating script execution.");
                        exitCode = -1;
                        break;
                    }
                }

                await thread.SleepAsync(options.DelayBetweenRequests);
            }

            if (scriptWarnings.Any())
            {
                console.WriteLine("### Warnings raised during script execution ###");
                foreach (var warning in scriptWarnings)
                {
                    console.WriteLine($"- {warning}");
                }
            }

            environment.Exit(exitCode);
        }
    }
}

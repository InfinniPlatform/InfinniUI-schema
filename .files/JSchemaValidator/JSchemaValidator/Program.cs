using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Schema;

namespace JSchemaValidator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args == null || args.Length != 2)
                {
                    WriteLineColored(ConsoleColor.White, @"USAGE");
                    WriteLineColored(ConsoleColor.Gray, @"  JSchemaValidator.exe <schema-base-uri> <schema-local-directory>");
                    Console.WriteLine();

                    WriteLineColored(ConsoleColor.White, @"EXAMPLE");
                    WriteLineColored(ConsoleColor.Gray, @"  JSchemaValidator.exe ""https://raw.githubusercontent.com/InfinniPlatform/InfinniUI-schema/master/"" ""C:\Projects\InfinniUI-schema""");
                    Console.WriteLine();

                    return 1;
                }

                var schemaBaseUri = args[0];
                var schemaLocalPath = args[1];

                var errorFiles = new List<string>();
                var jSchemaResolver = new JSchemaResolverLocal(schemaBaseUri, schemaLocalPath);
                var jSchemaFiles = Directory.GetFiles(schemaLocalPath, "*.json", SearchOption.AllDirectories);

                foreach (var file in jSchemaFiles)
                {
                    try
                    {
                        WriteLineColored(ConsoleColor.Gray, file);

                        var jSchemaContent = File.ReadAllText(file);

                        JSchema.Parse(jSchemaContent, jSchemaResolver);

                        WriteLineColored(ConsoleColor.Green, "OK");
                    }
                    catch (Exception exception)
                    {
                        errorFiles.Add(file);

                        while (exception != null)
                        {
                            WriteColored(ConsoleColor.Red, exception.Message);
                            WriteColored(ConsoleColor.Red, " -> ");
                            exception = exception.InnerException;
                        }

                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }

                if (errorFiles.Count != 0)
                {
                    WriteLineColored(ConsoleColor.Red, "Next files have errors:");

                    foreach (var file in errorFiles)
                    {
                        WriteLineColored(ConsoleColor.Red, "   " + file);
                    }
                }
                else
                {
                    WriteLineColored(ConsoleColor.Green, "All files are correct!");
                }

                return errorFiles.Count;
            }
            catch (Exception exception)
            {
                WriteLineColored(ConsoleColor.Red, exception.ToString());
                return 1;
            }
        }
        
        private static void WriteLineColored(ConsoleColor color, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }

        private static void WriteColored(ConsoleColor color, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.Write(format, args);
            Console.ResetColor();
        }
    }


    public class JSchemaResolverLocal : JSchemaResolver
    {
        private readonly string _baseUri;
        private readonly string _localPath;

        public JSchemaResolverLocal(string baseUri, string localPath)
        {
            _baseUri = baseUri;
            _localPath = localPath;
        }

        public override Stream GetSchemaResource(ResolveSchemaContext context, SchemaReference reference)
        {
            var schemaAbsoluteUri = reference.BaseUri.AbsoluteUri;

            if (!string.IsNullOrWhiteSpace(schemaAbsoluteUri) && schemaAbsoluteUri.StartsWith(_baseUri))
            {
                var schemaRelativeUri = schemaAbsoluteUri.Substring(_baseUri.Length);
                var schemaRelativePath = schemaRelativeUri.Replace('/', Path.DirectorySeparatorChar);
                var schemaAbsolutePath = Path.Combine(_localPath, schemaRelativePath);

                if (File.Exists(schemaAbsolutePath))
                {
                    return File.OpenRead(schemaAbsolutePath);
                }
            }

            return null;
        }
    }
}
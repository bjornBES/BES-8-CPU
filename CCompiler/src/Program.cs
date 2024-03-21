using Ccompiler.nodes;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Ccompiler
{
    class Program
    {
        static Dictionary<string, Func<string[], bool>> ArgumentList = new Dictionary<string, Func<string[], bool>>
        {
            {"-d", Compiler_Debug},
            {"-I", Include_Directory}
        };
        static Generator generator = new Generator();

        static List<FileInfo> files = new List<FileInfo>();

        const string StdIOCLibraryPaths = "./";
        static string UserLibraryPath = "./";
        static int m_index = 0;
        static string inputFile = "\0";
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("ERROR");
                return -1;
            }

            for (m_index = 0; m_index < args.Length; m_index++)
            {
                args[m_index] = args[m_index].Replace('\\', '/');
                if (ArgumentList.ContainsKey(args[m_index]))
                {
                    ArgumentList[args[m_index]](args);
                }
                else if (args[m_index].Contains(".c") || args[m_index].Contains("/") || args[m_index].Contains("./"))
                {
                    inputFile = args[m_index];
                    continue;
                }
            }

            string FileContents = $"filename_TYPE {inputFile}\n";
            FileContents += File.ReadAllText(inputFile);
            FileContents = FileContents.Replace("\r\n", "\n");
            files.Add(new FileInfo(inputFile));

            string[] SplitedContents;
            for (int i = 0; i < FileContents.Split('\n').Length; i++)
            {
                FileContents = FileContents.Replace("\r\n", "\n");
                SplitedContents = FileContents.Split('\n');
                if (SplitedContents[i].StartsWith('#'))
                {
                    switch (SplitedContents[i].TrimStart('#').Split(' ')[0])
                    {
                        case "include":
                            string file = SplitedContents[i].Split(' ').Last();
                            file = file.Replace("\"", "");
                            if (file.StartsWith("./"))
                            {
                                if (File.Exists(file))
                                {
                                    files.Add(new FileInfo(file));
                                    FileContents += $"filename_TYPE {file}\n";
                                    FileContents = FileContents.Replace(SplitedContents[i], File.ReadAllText(file));
                                    i = -1;
                                    continue;
                                }
                            }
                            else
                            {
                                file = "./" + file;
                                if (File.Exists(file))
                                {
                                    files.Add(new FileInfo(file));
                                    FileContents += $"filename_TYPE {file}\n";
                                    FileContents = FileContents.Replace(SplitedContents[i], File.ReadAllText(file));
                                    i = -1;
                                    continue;
                                }
                            }
                            break;
                        case "pragma":
                            FileContents = FileContents.Replace(SplitedContents[i], "");
                            break;
                        default:
                            break;
                    }
                }
            }

            Tokenizer tokenizer = new Tokenizer();
            if (!File.Exists("./tokens.txt"))
            {
                File.Create("./tokens.txt").Close();
            }
            Token[] tokens = tokenizer.tokenize(FileContents);
            string format = "";
            for (int i = 0; i < tokens.Length; i++)
            {
                string value = "";
                if (!string.IsNullOrEmpty(tokens[i].value))
                {
                    value = tokens[i].value;
                }
                format += $"{tokens[i].Type}:".PadRight(16, ' ') + value.PadRight(20, ' ') + $" in file {tokens[i].File}:{tokens[i].line}:{tokens[i].column}\n";
            }
            File.WriteAllText("./tokens.txt", format);

            Parser parser = new Parser();
            NodeProg nodeProg = parser.Parse_prog(tokens);

            string json = JsonConvert.SerializeObject(nodeProg, Formatting.Indented, new JsonSerializerSettings()
            {
                MaxDepth = 0x800,
                TypeNameHandling = TypeNameHandling.All,
            });
            File.WriteAllText("./nodes.json", json);
            File.WriteAllLines("./out.basm", generator.gen_prog(nodeProg));
            return 0;
        }

        static bool Include_Directory(string[] args)
        {
            UserLibraryPath = args[m_index];
            return true;
        }

        static bool Compiler_Debug(string[] args)
        {
            generator.DebugEnabled = true;
            return true;
        }
    }
}

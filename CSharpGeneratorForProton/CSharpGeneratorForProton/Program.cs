using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneratorForProton {
    class Program {
        static void Main(string[] args) {
            if(args.Length > 0) {
                try {
                    var cmds = Utils.GetCommondLines(args);
                    if(cmds.ContainsKey("-h")) {
                        ShowHelpInfo();
                        return;
                    }

                    string schemaFile = cmds.GetArgument("-p");
                    string format = cmds.GetArgument("-e");
                    string output = cmds.GetArgument("-f");
                    string nameSpace = cmds.GetArgument("-n");
                    string suffix = cmds.GetArgument("-t", true);
                    string dataDir = cmds.GetArgument("-d", true);
                    string extension = cmds.GetArgument("-b", true);

                    Worker w = new Worker(new Worker.Args() {
                        SchemaFile = schemaFile,
                        Foramt = Utils.ToFormat(format),
                        OutPut = output,
                        Namespace = nameSpace,
                        Suffix = suffix,
                        DataDir = dataDir,
                        Extension = extension,
                    });
                    w.Do();
                }
                catch(CmdArgumentException e) {
                    Console.Error.WriteLine(e.ToString());
                    ShowHelpInfo();
                    Environment.ExitCode = -1;
                }
                catch(Exception e) {
                    Console.Error.WriteLine(e.ToString());
                    Environment.ExitCode = -1;
                }
            }
            else {
                ShowHelpInfo();
                Environment.ExitCode = -1;
            }
        }

        private static void ShowHelpInfo() {
            const string kHelpMessage = @"Usage: CSharpGeneratorForProton [-p schemaFile] [-e format] [-f output] [-n namespace]
Arguments 
-p              : schema file, Proton output
-e              : format, json or xml or protobuf
-f              : output  directory, will put the generated class code
-n              : namespace

Options
-t              : suffix, generates the suffix for the class  
-d              : protobuf binary data output directory, use only when '-e protbuf'  
-b              : protobuf binary data file extension, use only when '-e protbuf'  
-h              : show the help message    
";
            Console.Error.WriteLine(kHelpMessage);
        }
    }
}

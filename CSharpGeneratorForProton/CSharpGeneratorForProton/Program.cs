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
                    string output = cmds.GetArgument("-f");
                    string nameSpace = cmds.GetArgument("-n");
                    string suffix = cmds.GetArgument("-t", true);

                    bool isToProtobuf = cmds.ContainsKey("-e");
                    string dataDir = cmds.GetArgument("-d", true);
                    string extension = cmds.GetArgument("-b", true);

                    Worker w = new Worker(new Worker.Args() {
                        SchemaFile = schemaFile,
                        OutPut = output,
                        Namespace = nameSpace,
                        Suffix = suffix,
                        IsToProtobuf = isToProtobuf,
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
            const string kHelpMessage = @"Usage: CSharpGeneratorForProton [-p schemaFile] [-f output] [-n namespace]
Arguments 
-p              : schema file, Proton output
-f              : output directory, will put the generated class code
-n              : namespace of the generated class 

Options
-t              : suffix, generates the suffix for the class  
-e              : open convert exportfile to protobuf
-d              : protobuf binary data output directory, use only when '-e' exists  
-b              : protobuf binary data file extension, use only when '-e' exists
-h              : show the help message and exit    
";
            Console.Error.WriteLine(kHelpMessage);
        }
    }
}

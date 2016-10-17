# CSharpGeneratorForProton
CSharpGeneratorForProton generated C # code that reads xml, json, protobuf for [proton] (https://github.com/sy-yanghuan/proton). And xml, json can be converted to protobuf binary format (using protobuf-net).
## Command Line Parameters
```cmd
Usage: CSharpGeneratorForProton [-p schemaFile] [-f output] [-n namespace]
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
```
## *License*
[Apache 2.0 license](LICENSE).

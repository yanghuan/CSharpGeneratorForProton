[English](https://github.com/sy-yanghuan/CSharpGeneratorForProton#csharpgeneratorforproton)   [Chinese](https://github.com/sy-yanghuan/CSharpGeneratorForProton#csharpgeneratorforproton-1)  
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
## Generated Code Import
Generated C # code is not associated with the specific format, the specific read operation, are assigned to the GeneratorUtility class for processing, so the need to add the corresponding class into the project. The code is under the [Directory GeneratorUtility] (https://github.com/sy-yanghuan/CSharpGeneratorForProton/tree/master/CSharpGeneratorForProton/GeneratorUtility), you can modify the code according to the specific requirements, such as replacing the namespace, replace the read Library and so on.
- [GeneratorUtility for xml](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/CSharpGeneratorForProton/CSharpGeneratorForProton/GeneratorUtility/XmlLoader.cs)
- [GeneratorUtility for json](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/CSharpGeneratorForProton/CSharpGeneratorForProton/GeneratorUtility/JsonLoader.cs)
- [GeneratorUtility for protobuf](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/CSharpGeneratorForProton/CSharpGeneratorForProton/GeneratorUtility/ProtobufLoader.cs)  

## Example
[Example] (https://github.com/sy-yanghuan/CSharpGeneratorForProton/tree/master/CSharpGeneratorForProton/Example), A project is an instance of a full load configuration that generated by [proton's sample](https://github.com/sy-yanghuan/proton/tree/master/sample).

## *License*
[Apache 2.0 license](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/LICENSE).

_____________________
# CSharpGeneratorForProton
CSharpGeneratorForProton 是为[proton] (https://github.com/sy-yanghuan/proton)产生读取xml、json、protobuf的C#的代码。其还可将xml、jsond配置文件转换成protobuf二进制格式。
## 命令行参数
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
## 导入生成的代码
生成的C#代码并不与具体格式相关联，具体读取操作，均外派给GeneratorUtility工具类进行处理，所以还需将对应工具类添加入工程。代码均在[目录GeneratorUtility](https://github.com/sy-yanghuan/CSharpGeneratorForProton/tree/master/CSharpGeneratorForProton/CSharpGeneratorForProton/GeneratorUtility)下，可按具体使用要求修改其代码，例如更换命名空间、更换读取库等。
- [GeneratorUtility for xml](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/CSharpGeneratorForProton/CSharpGeneratorForProton/GeneratorUtility/XmlLoader.cs)
- [GeneratorUtility for json](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/CSharpGeneratorForProton/CSharpGeneratorForProton/GeneratorUtility/JsonLoader.cs)
- [GeneratorUtility for protobuf](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/CSharpGeneratorForProton/CSharpGeneratorForProton/GeneratorUtility/ProtobufLoader.cs)  

## 实例工程
[Example](https://github.com/sy-yanghuan/CSharpGeneratorForProton/tree/master/CSharpGeneratorForProton/Example)工程是一个完整的载入配置的实例，其载入配置是通过[proton的实例](https://github.com/sy-yanghuan/proton/tree/master/sample)导出的。

##*许可证*
[Apache 2.0 license](https://github.com/sy-yanghuan/CSharpGeneratorForProton/blob/master/LICENSE).

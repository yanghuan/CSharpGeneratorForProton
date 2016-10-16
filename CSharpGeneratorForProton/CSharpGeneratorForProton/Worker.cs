using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.CSharp;
using Newtonsoft.Json.Linq;

namespace CSharpGeneratorForProton {
    public sealed class Worker {
        public sealed class Args {
            public string SchemaFile;
            public bool IsToProtobuf;
            public string OutPut;
            public string Namespace;
            public string Suffix;
            public string DataDir;
            public string Extension;
        }

        private CodeDomProvider provider_ = CodeDomProvider.CreateProvider("CSharp");
        private CodeGeneratorOptions options_ = new CodeGeneratorOptions() { BracingStyle = "C"};
        private Args args_;

        public Worker(Args args) {
            args_ = args;
        }

        public void Do() {
            string json = File.ReadAllText(args_.SchemaFile, Encoding.UTF8);
            JArray array = JArray.Parse(json);
            if(array.Count == 0) {
                return;
            }

            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Utils.CreateDirectory(args_.OutPut);
            Utils.CreateDirectory(args_.DataDir);

            List<CodeUnitCreator> units = new List<CodeUnitCreator>();
            foreach(JObject item in array) {
                CodeUnitCreator codeUnitCreator = new CodeUnitCreator(args_, item);
                units.Add(codeUnitCreator);
            }

            if(args_.IsToProtobuf) {
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters cp = new CompilerParameters();
                cp.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "protobuf-net.dll"));
                cp.ReferencedAssemblies.Add(GetType().Assembly.Location);
                cp.GenerateExecutable = false;
                cp.GenerateInMemory = true;

                CompilerResults cr = provider.CompileAssemblyFromDom(cp, units.Select(i => i.GetCodeCompileUnit()).ToArray());
                if(cr.Errors.Count > 0) {
                    StringBuilder sb = new StringBuilder();
                    foreach(CompilerError ce in cr.Errors) {
                        sb.AppendFormat(" {0}", ce.ToString());
                        sb.AppendLine();
                    }
                    throw new System.Exception(sb.ToString());
                }

                string dir = Path.GetDirectoryName(units.First().ExportFile);
                Xml.GeneratorConfig.ConfigDir = dir;
                Json.GeneratorConfig.ConfigDir = dir;

                Assembly assembly = cr.CompiledAssembly;
                foreach(CodeUnitCreator unit in units) {
                    ToProtobuf(assembly, unit);
                }
            }
            else {
                foreach(CodeUnitCreator unit in units) {
                    Save(unit);
                }
            }
        }

        private void Save(CodeUnitCreator unit) {
            const int kDynamicVersionLineNum = 3;

            using(MemoryStream stream = new MemoryStream()) {
                StreamWriter sourceWriter = new StreamWriter(stream);
                CodeCompileUnit compileUnit = unit.GetCodeCompileUnit();
                provider_.GenerateCodeFromCompileUnit(compileUnit, sourceWriter, options_);
                sourceWriter.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                int count = 0;
                string path = Path.Combine(args_.OutPut, unit.RootClassName + ".cs");
                StreamReader reader = new StreamReader(stream);
                using(StreamWriter fileWriter = new StreamWriter(path)) {
                    while(true) {
                        string line = reader.ReadLine();
                        if(line != null) {
                            if(count == kDynamicVersionLineNum) {   //去掉动态版本号,每次编译都会不一样
                                int post = line.LastIndexOf('.');
                                if(post != -1) {
                                    line = line.Substring(0, post);
                                }
                            }
                            else if(count > kDynamicVersionLineNum) {
                                line = line.Replace(CodeUnitCreator.kPropertieSignReplace, CodeUnitCreator.kPropertieSign);  //生成的自动属性代码会多一个';',去掉它
                            }
                            fileWriter.WriteLine(line);
                        }
                        else {
                            break;
                        }
                        ++count;
                    }
                }
            }
        }

        private void ToProtobuf(Assembly assembly, CodeUnitCreator unit) {
            string fullName = args_.Namespace + '.' + unit.RootClassName;
            Type type = assembly.GetType(fullName);
            MethodInfo methodInfo = type.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == CodeUnitCreator.kLoadMethodName && !m.IsGenericMethodDefinition);
            object ins = methodInfo.Invoke(null, null);
            if(ins != null) {
                string path = Path.Combine(args_.DataDir, unit.Root);
                if(!string.IsNullOrEmpty(args_.Extension)) {
                    path += '.' + args_.Extension.TrimStart('.');
                }
                using(FileStream file = File.Create(path)) {
                    ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(file, ins);
                }
            }
            unit.RemoveProtoCode();
            Save(unit);
        }
    }

    public sealed class CodeUnitCreator {
        private const string kRoot = "root";
        private const string kItem = "item";
        public const string kExportFile = "exportfile";
        private const string kSchema = "schema";
        private const string kIGeneratorObject = "IGeneratorObject";
        private const string kRootParentSign = "<Sign>";
        public const string kLoadMethodName = "Load";
        private const string kGeneratorUtility = "GeneratorUtility";

        public const string kPropertieSign = " { get; private set; }";
        public const string kPropertieSignReplace = " { get; private set; };";

        private static readonly Dictionary<string, string> baseTypes = new Dictionary<string, string>() {
            ["int"] = "System.Int32",
            ["double"] = "System.Double",
            ["string"] = "System.String",
            ["bool"] = "System.Boolean",
        };

        private Worker.Args args_;
        private string root_;
        private string item_;
        private string exportFile_;
        private JObject schemaObj_;
        private string className_;
        private bool isItemTable;
        private CodeCompileUnit resultCodeCompileUnit_;
        private Action removeProtoCode_;

        public CodeUnitCreator(Worker.Args arg, JObject obj) {
            args_ = arg;
            root_ = obj[kRoot].ToString();
            item_ = obj[kItem].ToString();
            exportFile_ = obj[kExportFile].ToString();
            schemaObj_ = (JObject)obj[kSchema];
            className_ = item_.ToFirstCharUpper();
            if(!string.IsNullOrEmpty(args_.Suffix)) {
                className_ += args_.Suffix.ToFirstCharUpper();
            }
            isItemTable = root_.Contains(item_ + 's');          //判断是格式1,还是格式二
            resultCodeCompileUnit_ = BuildCodeCompileUnit();
        }

        private CodeCompileUnit BuildCodeCompileUnit() {
            CodeTypeDeclaration parent = new CodeTypeDeclaration(kRootParentSign);
            BuildObject(className_, schemaObj_, null, parent);
            CodeTypeDeclaration typeDeclaration = (CodeTypeDeclaration)parent.Members[0];
            typeDeclaration.Members.Add(CreateLoadMethod());
            typeDeclaration.Members.Add(CreateGenericsLoadMethod());

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace nameSpace = new CodeNamespace(args_.Namespace);
            if(IsProtobuf) {
                CodeNamespaceImport protoBufImport = new CodeNamespaceImport("ProtoBuf");
                nameSpace.Imports.Add(protoBufImport);
                string exportForamt = Path.GetExtension(exportFile_).Remove(0, 1).ToFirstCharUpper();
                if(exportForamt != "Json" && exportForamt != "Xml") {
                    throw new NotSupportedException("exportfile must be json or xml");
                }
                nameSpace.Imports.Add(new CodeNamespaceImport(GetType().Namespace + '.' + exportForamt));

                removeProtoCode_ += () => {
                    nameSpace.Imports.Clear();
                    nameSpace.Imports.Add(protoBufImport);
                };
            }
            nameSpace.Types.Add(typeDeclaration);
            compileUnit.Namespaces.Add(nameSpace);
            return compileUnit;
        }

        public CodeCompileUnit GetCodeCompileUnit() {
            return resultCodeCompileUnit_;
        }

        public void RemoveProtoCode() {
            if(removeProtoCode_ != null) {
                removeProtoCode_();
            }
        }

        public string RootClassName {
            get {
                return className_;
            }
        }

        public string Root {
            get {
                return root_;
            }
        }

        public string ExportFile {
            get {
                return exportFile_;
            }
        }

        private bool IsProtobuf {
            get {
                return args_.IsToProtobuf;
            }
        }

        private void GetTypeInfo(JArray a, out JToken type, out string description) {
            type = a[0];
            description = a.Count > 1 ? a[1].ToString() : null;
        }

        private CodeMemberMethod CreateLoadMethod() {
            CodeMemberMethod loadMethod = new CodeMemberMethod() {
                Name = kLoadMethodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
            };
            loadMethod.ReturnType = new CodeTypeReference(className_ + (isItemTable ? "[]" : ""));
            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeVariableReferenceExpression(string.Format("{0}<{1}>()", kLoadMethodName, className_)));
            loadMethod.Statements.Add(returnStatement);
            return loadMethod;
        }

        private CodeMemberMethod CreateGenericsLoadMethod() {
            CodeMemberMethod loadMethod = new CodeMemberMethod() {
                Name = kLoadMethodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
            };
            loadMethod.ReturnType = new CodeTypeReference("T" + (isItemTable ? "[]" : ""));

            CodeTypeParameter typeParameter = new CodeTypeParameter("T");
            typeParameter.HasConstructorConstraint = true;
            typeParameter.Constraints.Add(new CodeTypeReference(className_));
            loadMethod.TypeParameters.Add(typeParameter);

            string express;
            if(isItemTable) {
                express = string.Format("{0}.{1}<T>(\"{2}\", \"{3}\")", kGeneratorUtility, kLoadMethodName, root_, item_);
            }
            else {
                express = string.Format("{0}.{1}<T>(\"{2}\")", kGeneratorUtility, kLoadMethodName, root_);
            }
            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeVariableReferenceExpression(express));
            loadMethod.Statements.Add(returnStatement);
            return loadMethod;
        }

        private void CreateClassMethod(CodeTypeDeclaration typeDeclaration, CodeStatementCollection statements, bool isRootType) {
            const string kReadName = "Read";
            const string kOnInitName = "OnInit";
            const string kConfigElement = "ConfigElement";
            const string kElement = "element";

            CodeMemberMethod readMethod = new CodeMemberMethod() {
                Name = kReadName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
            };
            CodeParameterDeclarationExpression element = new CodeParameterDeclarationExpression(kConfigElement, kElement);
            readMethod.Parameters.Add(element);
            readMethod.Statements.AddRange(statements);
            if(isRootType) {
                readMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), kOnInitName));
            }
            typeDeclaration.Members.Add(readMethod);
            if(IsProtobuf) {
                removeProtoCode_ += () => typeDeclaration.Members.Remove(readMethod);
            }

            if(isRootType) {
                CodeMemberMethod initMethod = new CodeMemberMethod() {
                    Name = kOnInitName,
                    Attributes = IsProtobuf ? MemberAttributes.Public : MemberAttributes.Family,
                };
                typeDeclaration.Members.Add(initMethod);
            }
            else if(IsProtobuf) {
                CodeMemberMethod initMethod = new CodeMemberMethod() {
                    Name = kOnInitName,
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                };
                typeDeclaration.Members.Add(initMethod);
            }
        }

        private string BuildObject(string name, JObject type, string descripion, CodeTypeDeclaration parent) {
            bool isRootType = parent.Name == kRootParentSign;
            CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(name) {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public,
            };
            if(!isRootType) {
                typeDeclaration.TypeAttributes |= TypeAttributes.Sealed;
            }
            typeDeclaration.BaseTypes.Add(kIGeneratorObject);
            if(IsProtobuf) {
                typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration("ProtoContract"));
            }
            if(!string.IsNullOrEmpty(descripion)) {
                typeDeclaration.Comments.Add(new CodeCommentStatement(descripion));
            }

            CodeStatementCollection statements = new CodeStatementCollection();
            foreach(var i in type) {
                JArray a = (JArray)i.Value;
                JToken itemType;
                string itemDescription;
                GetTypeInfo(a, out itemType, out itemDescription);
                string typeName = Build(i.Key, itemType, typeDeclaration);
                var statement = AddProperty(i.Key, typeName, itemDescription, typeDeclaration);
                statements.Add(statement);
            }
            CreateClassMethod(typeDeclaration, statements, isRootType);
            parent.Members.Add(typeDeclaration);
            return name;
        }

        private string BuildArray(string name, JArray array, CodeTypeDeclaration parent) {
            JToken baseType;
            string _;
            GetTypeInfo(array, out baseType, out _);
            string baseName = name.Remove(name.Length - 1);
            return Build(baseName, baseType, parent) + "[]";
        }

        private string Build(string name, JToken type, CodeTypeDeclaration parent) {
            switch(type.Type) {
                case JTokenType.String: {
                        return baseTypes[type.ToString()];
                    }
                case JTokenType.Array: {
                        return BuildArray(name, (JArray)type, parent);
                    }
                case JTokenType.Object: {
                        name = name.ToFirstCharUpper() + '_';
                        return BuildObject(name, (JObject)type, null, parent);
                    }
                default: {
                        throw new NotSupportedException();
                    }
            }
        }

        private CodeAssignStatement AddProperty(string name, string typeName, string description, CodeTypeDeclaration parent) {
            string fieldName = name.ToFirstCharUpper();
            CodeMemberField field = new CodeMemberField(typeName, fieldName) { Attributes = MemberAttributes.Public | MemberAttributes.Final };
            if(!string.IsNullOrEmpty(description)) {
                field.Comments.Add(new CodeCommentStatement(description));
            }
            parent.Members.Add(field);

            if(IsProtobuf) {
                const string kFieldCount = "FieldCount";
                int count;
                if(parent.UserData.Contains(kFieldCount)) {
                    count = (int)parent.UserData[kFieldCount];
                    ++count;
                    parent.UserData[kFieldCount] = count;
                }
                else {
                    count = 1;
                    parent.UserData.Add(kFieldCount, count);
                }
                CodeAttributeArgument codeAttr = new CodeAttributeArgument(new CodePrimitiveExpression(count));
                CodeAttributeDeclaration protoMemberAttribute = new CodeAttributeDeclaration("ProtoMember", codeAttr);
                field.CustomAttributes.Add(protoMemberAttribute);
            }
            else {
                field.Name += kPropertieSign;       //使字段,变成自动属性
            }

            CodeAssignStatement assign = new CodeAssignStatement(
                new CodeVariableReferenceExpression("this." + fieldName), 
                new CodeVariableReferenceExpression(string.Format("{0}.Get(element, \"{1}\", {2})", kGeneratorUtility,  name, "this." + fieldName)));
            return assign;
        }
    }
}

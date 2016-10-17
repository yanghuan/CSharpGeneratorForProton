using System;
using System.Collections.Generic;
using System.IO;

using ProtoBuf.Meta;

namespace CSharpGeneratorForProton.Protobuf {
    internal interface IGeneratorObject {
        void OnInit();
    }

    public interface IDelayInit {
        void OnDelayInit();
    }

    public static class GeneratorConfig {
        public static string ConfigDir = "";
        private static List<Action> DelayInitAction = new List<Action>();

        public static void InvokeDelayInitAction() {
            while(DelayInitAction != null && DelayInitAction.Count > 0) {
                var list = DelayInitAction;
                DelayInitAction = null;
                foreach(Action action in list) {
                    action();
                }
            }
            DelayInitAction = null;
        }

        public static void AddDelayAction(Action action) {
            if(DelayInitAction == null) {
                DelayInitAction = new List<Action>();
            }
            DelayInitAction.Add(action);
        }
    }

    internal static class GeneratorUtility {
        public static T[] Load<T>(string fileName, string itemName) where T : IGeneratorObject, new() {
            TryAddSubTypeTypeModel(typeof(T));
            using(Stream stream = GetContentStream(fileName)) {
                List<T> list = new List<T>();
                if(stream != null) {
                    RuntimeTypeModel.Default.Deserialize(stream, list, list.GetType());
                    foreach(var item in list) {
                        InitObj(item);
                    }
                }
                return list.ToArray();
            }
        }

        public static T Load<T>(string fileName) where T : IGeneratorObject, new() {
            TryAddSubTypeTypeModel(typeof(T));
            using(Stream stream = GetContentStream(fileName)) {
                T t = new T();
                if(stream != null) {
                    RuntimeTypeModel.Default.Deserialize(stream, t, t.GetType());
                    InitObj(t);
                }
                return t;
            }
        }

        /// <summary>
        ///  动态注入派生类
        /// </summary>
        private static void TryAddSubTypeTypeModel(Type type) {
            Type baseType = type.BaseType;
            if(baseType != null && typeof(IGeneratorObject).IsAssignableFrom(baseType)) {
                if(!RuntimeTypeModel.Default.IsDefined(type)) {
                    var baseClassModel = RuntimeTypeModel.Default[baseType];
                    var subClassModel = RuntimeTypeModel.Default[type];

                    /** 使用继承方式序列化时,派生类需重新添加一遍所需序列化的字段,此外若派生类使用了同名字段覆盖基类,还需要修改protobuf-net的源码
                        将protobuf-net\Meta\MetaType.cs中AddField方法中的    
                        if(members != null && members.Length == 1) mi = members[0];
                        修改为
                        if(members != null)
                        {
         	                foreach(MemberInfo m in members)
                            {
                                bool isSuccess = m.IsDefined(typeof(ProtoMemberAttribute), true);
                                if(isSuccess)
                                {
                                    mi = m;
                                    break;
                                }
                            }
                        }
                        //if(members != null && members.Length == 1) mi = members[0];
                    **/

                    foreach(var f in baseClassModel.GetFields()) {
                        subClassModel.AddField(f.FieldNumber, f.Name);
                    }
                }
            }
        }

        private static void InitObj(IGeneratorObject t) {
            t.OnInit();
            IDelayInit delayInit = t as IDelayInit;
            if(delayInit != null) {
                GeneratorConfig.AddDelayAction(delayInit.OnDelayInit);
            }
        }

        private static Stream GetContentStream(string fileName) {
            string path = Path.Combine(GeneratorConfig.ConfigDir, fileName + ".bytes");
            if(!File.Exists(path)) {
                return null;
            }
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}

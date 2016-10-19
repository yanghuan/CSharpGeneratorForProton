using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Ice.Project.Config {
    public interface ConfigElement {
        string GetAttribute(string name);
        string GetText();
        IEnumerable<ConfigElement> GetElements(string name);
        ConfigElement GetElement(string name);
    }

    public interface IGeneratorObject {
        void Read(ConfigElement item);
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

    public static class GeneratorUtility {
        public static int Get(ConfigElement element, string name, int _) {
            string s = element.GetAttribute(name);
            return Convert(s, _);
        }

        public static double Get(ConfigElement element, string name, double _) {
            string s = element.GetAttribute(name);
            return Convert(s, _);
        }

        public static string Get(ConfigElement element, string name, string _) {
            string s = element.GetAttribute(name);
            return Convert(s, _);
        }

        public static bool Get(ConfigElement element, string name, bool _) {
            string s = element.GetAttribute(name);
            return Convert(s, _);
        }

        public static T Get<T>(ConfigElement element, string name, T _) where T : IGeneratorObject, new() {
            var node = element.GetElement(name);
            return Convert(node, _);
        }

        public static int[] Get(ConfigElement element, string itemName, int[] _) {
            return GetArray<int>(element, itemName, _, Convert);
        }

        public static double[] Get(ConfigElement element, string itemName, double[] _) {
            return GetArray<double>(element, itemName, _, Convert);
        }

        public static string[] Get(ConfigElement element, string itemName, string[] _) {
            return GetArray<string>(element, itemName, _, Convert);
        }

        public static T[] Get<T>(ConfigElement element, string itemName, T[] _) where T : IGeneratorObject, new() {
            return GetArray<T>(element, itemName, _, Convert);
        }

        public static T[] Load<T>(string fileName, string itemName) where T : IGeneratorObject, new() {
            using(var stream = GetContentStream(fileName)) {
                var root = LoadRootElement(stream);
                T[] items = GetArray(root, itemName + 's', default(T[]), Convert);
                return items != null ? items : new T[0];
            }
        }

        public static T Load<T>(string fileName) where T : IGeneratorObject, new() {
            using(var stream = GetContentStream(fileName)) {
                var root = LoadRootElement(stream);
                return Convert(root, default(T));
            }
        }

        private static T[] GetArray<T>(ConfigElement element, string itemName, T[] _, Func<ConfigElement, T, T> convert) {
            string childName = itemName.Remove(itemName.Length - 1);

            var listNode = element.GetElement(itemName);
            if(listNode != null) {
                List<T> list = new List<T>();
                foreach(var node in listNode.GetElements(childName)) {
                    list.Add(convert(node, default(T)));
                }

                return list.Count > 0 ? list.ToArray() : null;
            }

            return null;
        }

        private static int Convert(ConfigElement e, int _) {
            return Convert(e.GetText(), _);
        }

        private static double Convert(ConfigElement e, double _) {
            return Convert(e.GetText(), _);
        }

        private static string Convert(ConfigElement e, string _) {
            return Convert(e.GetText(), _);
        }

        private static bool Convert(ConfigElement e, bool _) {
            return Convert(e.GetText(), _);
        }

        private static T Convert<T>(ConfigElement e, T _) where T : IGeneratorObject, new() {
            if(e == null) {
                return default(T);
            }

            T t = new T();
            t.Read(e);

            IDelayInit delayInit = t as IDelayInit;
            if(delayInit != null) {
                GeneratorConfig.AddDelayAction(delayInit.OnDelayInit);
            }
            return t;
        }

        private static int Convert(string s, int _) {
            if(string.IsNullOrEmpty(s)) {
                return 0;
            }

            return int.Parse(s);
        }

        private static double Convert(string s, double _) {
            if(string.IsNullOrEmpty(s)) {
                return 0.0;
            }

            return double.Parse(s);
        }

        private static string Convert(string s, string _) {
            return s;
        }

        private static bool Convert(string s, bool _) {
            if(string.IsNullOrEmpty(s)) {
                return false;
            }
            return bool.Parse(s);
        }

        private static Stream GetContentStream(string fileName) {
            string path = Path.Combine(GeneratorConfig.ConfigDir, fileName + ".xml");
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream;
        }

        private static ConfigElement LoadRootElement(Stream stream) {
            XDocument document = XDocument.Load(stream);
            var root = document.Root;
            return new XDocumentConfigElement(root);
        }

        private sealed class XDocumentConfigElement : ConfigElement {
            private XElement element_;

            public XDocumentConfigElement(XElement element) {
                element_ = element;
            }

            public string GetAttribute(string name) {
                var attribute = element_.Attribute(name);
                return attribute != null ? attribute.Value : null;
            }

            public string GetText() {
                return element_.Value;
            }

            public IEnumerable<ConfigElement> GetElements(string name) {
                return element_.Elements(name).Select(i => new XDocumentConfigElement(i));
            }

            public ConfigElement GetElement(string name) {
                var e = element_.Element(name);
                return e != null ? new XDocumentConfigElement(e) : null;
            }
        }
    }
}

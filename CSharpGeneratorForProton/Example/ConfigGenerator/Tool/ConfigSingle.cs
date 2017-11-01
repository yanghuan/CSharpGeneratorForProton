using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ice.Project.Config {
  public class ConfigSingle<T, T1>
      where T : ConfigSingle<T, T1>, new()
      where T1 : class, IGeneratorObject, new() {

    protected static T instance_;

    public static T Instance {
      get {
        Contract.Assert(instance_ != null);
        return instance_;
      }
    }

    public static bool IsInstanceCreated {
      get {
        return instance_ != null;
      }
    }

    public ConfigSingle() {
      Contract.Assert(instance_ == null);
    }

    protected virtual void Init(T1[] array) {
    }

    protected virtual void Init(T1 template) {

    }

    public static void Load() {
      Type t1 = typeof(T1);
      MethodInfo methodInfo = t1.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).First(m => m.Name == "Load" && m.IsGenericMethodDefinition);
      methodInfo = methodInfo.MakeGenericMethod(typeof(T1));
      object o = methodInfo.Invoke(null, null);

      instance_ = new T();

      T1[] array = o as T1[];
      if (array != null) {
        instance_.Init(array);
      } else {
        T1 t = (T1)o;
        instance_.Init(t);
      }
    }

    public string Name {
      get {
        return this.GetType().Name;
      }
    }
  }

  public abstract class ConfigSingleExtend<T, T1, K> : ConfigSingle<T, T1>
      where T : ConfigSingle<T, T1>, new()
      where T1 : class, IGeneratorObject, new() {
    private Dictionary<K, T1> items_;

    protected override void Init(T1[] array) {
      items_ = new Dictionary<K, T1>(array.Length);
      foreach (var i in array) {
        K key = GetId(i);
        if (items_.ContainsKey(key)) {
          throw new Exception(string.Format("{0} is already in {1}", key, Name));
        }
        items_.Add(key, i);
      }
    }

    public T1 GetItemTemplate(K id) {
      T1 item;
      if (items_.TryGetValue(id, out item)) {
        return item;
      }

      Console.Error.WriteLine("{0} is not in {1}", id, Name);
      return default(T1);
    }

    public bool IsExists(K id) {
      return items_.ContainsKey(id);
    }

    protected abstract K GetId(T1 item);

    protected Dictionary<K, T1> Dict {
      get {
        return items_;
      }
    }
  }

  public abstract class ConfigSingleExtend<T, T1> : ConfigSingleExtend<T, T1, int>
      where T : ConfigSingle<T, T1>, new()
      where T1 : class, IGeneratorObject, new() {

  }
}

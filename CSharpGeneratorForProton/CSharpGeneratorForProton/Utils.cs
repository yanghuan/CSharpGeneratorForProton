using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpGeneratorForProton {
  public sealed class CmdArgumentException : Exception {
    public CmdArgumentException(string message) : base(message) {
    }
  }

  public static class Utils {
    public static Dictionary<string, string[]> GetCommondLines(string[] args) {
      Dictionary<string, string[]> cmds = new Dictionary<string, string[]>();

      string key = "";
      List<string> values = new List<string>();

      foreach (var i in args) {
        if (i.StartsWith("-")) {
          if (!string.IsNullOrEmpty(key)) {
            cmds.Add(key, values.ToArray());
            key = "";
            values.Clear();
          }
          key = i;
        } else {
          values.Add(i);
        }
      }

      if (!string.IsNullOrEmpty(key)) {
        cmds.Add(key, values.ToArray());
      }
      return cmds;
    }

    public static T GetOrDefault<K, T>(this IDictionary<K, T> dict, K key, T t = default(T)) {
      T v;
      if (dict.TryGetValue(key, out v)) {
        return v;
      }
      return t;
    }

    public static string GetArgument(this Dictionary<string, string[]> args, string name, bool isOption = false) {
      string[] values = args.GetOrDefault(name);
      if (values == null || values.Length == 0) {
        if (isOption) {
          return null;
        }
        throw new CmdArgumentException(name + " is not found");
      }
      return values[0];
    }

    public static string ToFirstCharUpper(this string s) {
      char first = s[0];
      if (!char.IsUpper(first)) {
        return char.ToUpper(first) + s.Substring(1);
      }
      return s;
    }

    public static void CreateDirectory(string dir) {
      if (!string.IsNullOrEmpty(dir)) {
        if (!Directory.Exists(dir)) {
          Directory.CreateDirectory(dir);
        }
      }
    }
  }
}

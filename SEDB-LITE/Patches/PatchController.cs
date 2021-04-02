using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using VRage.Utils;
using HarmonyLib;

namespace SEDB_LITE {

    public static class PatchController {
        public static MyLog Log = new MyLog();

        public class TargetMethod : Attribute {
            public Type Type { get; set; }
            public string Method { get; set; }
        }

        public class PrefixMethod : Attribute {

        }

        public class PatchingClass : Attribute {

        }

        public static void PatchMethods() {
            var harmony = new Harmony("SEDB-LITE");
            Log.WriteLineAndConsole("Patching methods...");
            var assembly = Assembly.GetExecutingAssembly();

            foreach(var PatchingClass in GetPatchingClasses(assembly)) {
                foreach (var method in PatchingClass.GetMethods().Where(x => x.GetCustomAttributes(typeof(PrefixMethod), false).FirstOrDefault() != null)) {
                    TargetMethod TargetMethodData = (TargetMethod)method.GetCustomAttribute(typeof(TargetMethod));
                    Log.WriteLineAndConsole($"Patching {TargetMethodData.Method} with {method.Name} (Prefix)");
                    harmony.Patch(TargetMethodData.Type.GetMethod(TargetMethodData.Method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public), new HarmonyMethod(method));
                }
            }
        }

        public static IEnumerable<Type> GetPatchingClasses(Assembly assembly) {
            foreach (Type type in assembly.GetTypes()) {
                if (type.GetCustomAttributes(typeof(PatchingClass), true).Length > 0) {
                    yield return type;
                }
            }
        }
    }
}

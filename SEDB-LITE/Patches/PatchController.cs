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

        public class PostFixMethod : Attribute {

        }

        public class PatchingClass : Attribute {

        }

        public static void PatchMethods() {
            Log.WriteLineAndConsole("Patching methods...");
            var assembly = Assembly.GetExecutingAssembly();

            foreach(var PatchingClass in GetPatchingClassesAndInitalize(assembly)) {

                foreach (var method in PatchingClass.GetMethods().Where(x => x.GetCustomAttributes(typeof(PrefixMethod), false).FirstOrDefault() != null)) {
                    Patch(method, typeof(PrefixMethod));
                }

                foreach (var method in PatchingClass.GetMethods().Where(x => x.GetCustomAttributes(typeof(PostFixMethod), false).FirstOrDefault() != null)) {
                    Patch(method, typeof(PostFixMethod));
                }
            }
        }

        public static void Patch(MethodInfo newMethod,Type typeOfPatch) {
            var harmony = new Harmony("SEDB-LITE");
            TargetMethod TargetMethodData = (TargetMethod)newMethod.GetCustomAttribute(typeof(TargetMethod));

            if (Plugin.DEBUG) {

                var methods = TargetMethodData.Type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                Log.WriteLineAndConsole($"Listing methods in {TargetMethodData.Type.Name}");
                foreach (var method in methods) {
                    Log.WriteLineAndConsole($"Method name: {method.Name}");
                }
            }

            Log.WriteLineAndConsole($"Patching {TargetMethodData.Method} with {newMethod.Name}");

            if (typeOfPatch == typeof(PrefixMethod)) {
                harmony.Patch(TargetMethodData.Type.GetMethod(TargetMethodData.Method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public), new HarmonyMethod(newMethod));

            }
            else {
                harmony.Patch(TargetMethodData.Type.GetMethod(TargetMethodData.Method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public), null, new HarmonyMethod(newMethod));

            }
        }

        public static IEnumerable<Type> GetPatchingClassesAndInitalize(Assembly assembly) {
            foreach (Type type in assembly.GetTypes()) {
                if (type.GetCustomAttributes(typeof(PatchingClass), true).Length > 0) {

                    Activator.CreateInstance(type, Plugin.PluginInstance);
                    if (Plugin.DEBUG) {
                        Log.WriteLineAndConsole($"Found patching class: {type.Name}");

                    }
                    yield return type;
                }
            }
        }
    }
}

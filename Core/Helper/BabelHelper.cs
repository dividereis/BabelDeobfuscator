using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BabelDeobfuscator.Core.Helper
{
    class BabelHelper
    {

        public static ModuleDefMD assem;
        public static MethodDef DecryptionMethod_premium;
        public static TypeDef DecryptionType_premium;
        public static MethodDef DecryptionMethod_free;
        public static TypeDef DecryptionType_free;

        /// <summary>
        /// Basically we will crawl into the assembly and check 
        /// if a method contains some very specific <see cref="System.Collections.Hashtable"/> methods.
        /// The premium / genuine version of Babel contains Streams and TypeBuilders.
        /// </summary>
        /// <param name="asm">Obvious is obvious</param>
        /// <returns>True or False, come one dude...</returns>
        public static bool IsFreeVersion(ModuleDefMD asm)
        {
            /**/
            assem = asm;
            /**/
            /*.maxstack 2
	        .locals init (
		        [0] class [mscorlib]System.Collections.Hashtable
	        )*/

            foreach (TypeDef type in asm.Types)
            {
                foreach (MethodDef meth in type.Methods)
                {
                    if (!meth.HasBody) continue;
                    if (!meth.Body.HasVariables) continue;
                    if (!meth.IsHideBySig) continue;
                    if (!meth.HasReturnType) continue;
                    if (meth.ReturnType.ToString() != "System.String") continue;

                    var Locals = meth.Body.Variables;
                    if (Locals.Count == 1)
                    {
                        foreach (var local in Locals)
                        {
                            if (local.Type.ToString().ToLower().Contains("hashtable"))
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("[!] Assemly may have been protected with a premium version");
                                Console.WriteLine("     " + meth.FullName + "(RVA:" + meth.RVA + ")");
                                DecryptionMethod_premium = meth;
                                DecryptionType_premium = type;
                                Console.ForegroundColor = ConsoleColor.White;
                                return false;
                            }
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[!] Assemly may have been protected with a free version");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }


        /// <summary>
        /// We simply need to crawl into every methods and check
        /// for each time the decryption method is called.
        /// This is not an absolutly needed step though :)
        /// </summary>
        /// <returns></returns>
        public static int GetCalls_premium()
        {
            int calledtime = 0;
            var asm = assem;
            var dec_meth = DecryptionMethod_premium;

            foreach (TypeDef type in asm.Types)
            {
                foreach (MethodDef meth in type.Methods)
                {
                    if (!meth.HasBody) continue;
                    var instrs = meth.Body.Instructions;
                    for (int i = 0; i < instrs.Count; i++)
                    {
                        if (meth.Body.Instructions[i].OpCode != OpCodes.Call) continue;
                        var CalledDecMethod = meth.Body.Instructions[i].Operand;
                        if (meth.Body.Instructions[i].Operand.ToString().Contains(dec_meth.FullName))
                        {
                            calledtime += 1;
                        }

                    }
                }
            }

            return calledtime;
        }


        /// <summary>
        /// the decryption method routine for registered version 
        /// of Babel Obfuscator.
        /// The idea comes from Alatrazz :)
        /// </summary>
        /// <returns></returns>
        public static int DecrypStrings_premium()
        {
            int decryptedstr = 0;
            var asm = assem;
            var dec_meth = DecryptionMethod_premium;
            foreach (TypeDef type in asm.Types)
            {
                foreach (MethodDef meth in type.Methods)
                {
                    if (!meth.HasBody) continue;
                    CilBody body = meth.Body;
                    var instrs = meth.Body.Instructions;
                    for (int i = 0; i < instrs.Count; i++)
                    {
                        if (meth.Body.Instructions[i].OpCode != OpCodes.Call) continue;
                        var CalledDecMethod = meth.Body.Instructions[i].Operand;
                        if (meth.Body.Instructions[i].Operand.ToString().Contains(dec_meth.FullName))
                        {
                            if (meth.Body.Instructions[i - 1].IsLdcI4())
                            {
                                var decryptionkey = meth.Body.Instructions[i - 1].GetLdcI4Value();


                                /*======================================================================*/

                                Assembly assembly = Assembly.LoadFile(Program.asmpath);

                                if (asm.EntryPoint != null)
                                {
                                    object[] parametersZ;
                                    if (asm.EntryPoint.Parameters.Count == 0)
                                    {
                                        parametersZ = new object[] { };
                                    }
                                    else
                                    {
                                        parametersZ = new object[] { new string[] { } };
                                    }
                                    try
                                    {
                                        string res = "Error invoking entrypoint";
                                        try
                                        {
                                            assembly.EntryPoint.Invoke(null, parametersZ);
                                            Type typez = assembly.GetType(DecryptionType_premium.Name);
                                            if (typez != null)
                                            {
                                                MethodInfo methodInfo = typez.GetMethod(DecryptionMethod_premium.Name,
                                                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
                                                if (methodInfo != null)
                                                {
                                                    object result = null;
                                                    ParameterInfo[] parameters = methodInfo.GetParameters();
                                                    if (parameters.Length == 0)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        object[] parametersArray = new object[] { decryptionkey };
                                                        try
                                                        {
                                                            result = methodInfo.Invoke(/*methodInfo,*/null, parametersArray);
                                                            var decryptedstring = result.ToString();
                                                            decryptedstr += 1;
                                                            body.Instructions[i].OpCode = OpCodes.Ldstr;
                                                            body.Instructions[i].Operand = decryptedstring;
                                                            body.Instructions.RemoveAt(i - 1);
                                                            Console.ForegroundColor = ConsoleColor.Gray;
                                                            Console.WriteLine("     [+]" + decryptedstring + body.Instructions[i].GetOffset());
                                                            Console.ForegroundColor = ConsoleColor.White;
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Console.WriteLine(ex);
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            res = ex.Message;
                                        }
                                        Thread.Sleep(900);
                                    }
                                    catch
                                    {
                                        return decryptedstr;
                                    }
                                    /*======================================================================*/

                                    /*======================================================================*/
                                }
                                else
                                {
                                    return decryptedstr;
                                }
                            }
                        }
                    }
                }
            }
            return decryptedstr;
        }


        /// <summary>
        /// Search'N'Destroy method for the time-period limitation
        /// for assembly protected with the free version
        /// </summary>
        /// <param name="module"></param>
        public static void RemoveDemoFree(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;
                    if (method.Body.HasInstructions)
                    {
                        var instrs = method.Body.Instructions;
                        if (instrs.Count > 6)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                if (method.Body.Instructions[i].OpCode == OpCodes.Call &&
                                    method.Body.Instructions[i + 1].OpCode == OpCodes.Ldc_I8 &&
                                    method.Body.Instructions[i + 2].OpCode == OpCodes.Newobj &&
                                    method.Body.Instructions[i + 3].OpCode == OpCodes.Call &&
                                    method.Body.Instructions[i + 4].OpCode == OpCodes.Brfalse_S &&
                                    method.Body.Instructions[i + 5].OpCode == OpCodes.Newobj &&
                                    method.Body.Instructions[i + 6].OpCode == OpCodes.Throw)
                                {
                                    for (int j = 0; j < 7; j++)
                                        method.Body.Instructions.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// We have to get the string decryption method for applications
        /// protected with the free version
        /// </summary>
        /// <param name="module"></param>
        public static MethodDef FindStringDecrypterMethods(ModuleDefMD module)
        {
            MethodDef decmeth = null;

            /*------*/
            /* CALL */
            /*------*/
            /* 
             * Class2.smethod_0("", 62708);
             */

            /*-------------*/
            /* Method Code */
            /*-------------*/
            /*
	            public static string smethod_0(string string_0, int int_0)
	            {
		            return string.Intern(Class2.Class3.class3_0.method_0(string_0, int_0));
	            } 
             */
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method.HasBody == false)
                        continue;
                    if (method.Body.HasInstructions)
                    {
                        var param = method.Parameters.Count;
                        /*
                         string smethod_0 (
		                    string string_0,
		                    int32 int_0
	                     ) cil managed 
                         */

                        if (param != 2)
                            continue;
                        var returnedparam = method.ReturnType;
                        //System.String
                        if (returnedparam.ToString() != "System.String")
                            continue;
                        if (method.IsHideBySig && method.IsStatic)
                        {
                            var instrs = method.Body.Instructions;
                            //Should be between 4 and 7
                            if (instrs.Count > 5)
                            {
                                for (int i = 0; i < instrs.Count; i++)
                                {
                                    /*
                                    IL_0000: ldsfld         class Class2/Class3 Class2/Class3::class3_0
                                    IL_0005: ldarg.0
                                    IL_0006: ldarg.1
                                    IL_0007: callvirt       instance string Class2/Class3::method_0(string, int32)
                                    IL_000C: call           string[mscorlib] System.String::Intern(string)
                                    IL_0011: ret
                                    */

                                    if (instrs[i].Operand != null)
                                    {
                                        if (instrs[i].Operand.ToString().ToLower().Contains("intern"))
                                        {
                                            DecryptionMethod_free = method;
                                            decmeth = DecryptionMethod_free;
                                            DecryptionType_free = type;
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("[!] Assemly may have been protected with a premium version");
                                            Console.WriteLine("     " + DecryptionMethod_free.FullName + "(RVA:" + DecryptionMethod_free.RVA + ")");
                                            Console.ForegroundColor = ConsoleColor.White;
                                            return decmeth;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return decmeth;
        }


        /// <summary>
        /// the decryption method routine for free version 
        /// of Babel Obfuscator.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static int DecryptStringsInMethod(ModuleDefMD module)
        {
            var ConstantNum = 0;
            var DeobedStringNumber = 0;
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr && method.Body.Instructions[i + 1].IsLdcI4() && method.Body.Instructions[i + 2].OpCode == OpCodes.Call)
                        {
                            if (method.Body.Instructions[i + 1].IsLdcI4())
                                ConstantNum = (method.Body.Instructions[i + 1].GetLdcI4Value());
                            if (method.Body.Instructions[i + 2].Operand.ToString().Contains(DecryptionMethod_free.ToString()))
                            {
                                CilBody body = method.Body;
                                var string2decrypt = method.Body.Instructions[i].Operand.ToString();
                                string decryptedstring = null;

                                Assembly assembly = Assembly.LoadFile(Program.asmpath);
                                Type typez = assembly.GetType(DecryptionType_free.Name);
                                if (typez != null)
                                {
                                    MethodInfo methodInfo = typez.GetMethod(DecryptionMethod_free.Name,
                                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
                                    if (methodInfo != null)
                                    {
                                        object result = null;
                                        ParameterInfo[] parameters = methodInfo.GetParameters();
                                        if (parameters.Length == 0)
                                        {

                                        }
                                        else
                                        {
                                            object[] parametersArray = new object[] { string2decrypt, ConstantNum };
                                            result = methodInfo.Invoke(methodInfo, parametersArray);
                                            decryptedstring = result.ToString();
                                            DeobedStringNumber = DeobedStringNumber + 1;
                                            body.Instructions[i].OpCode = OpCodes.Ldstr;
                                            body.Instructions[i].Operand = decryptedstring;
                                            body.Instructions.RemoveAt(i + 1);
                                            body.Instructions.RemoveAt(i + 1);
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                            Console.WriteLine("     [+]" + decryptedstring + " : " + body.Instructions[i].GetOffset());
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return DeobedStringNumber;
        }
    }
}

using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BabelDeobfuscator
{
    static class Program
    {
        //The assembly
        static ModuleDefMD asm;
        public static string asmpath;
        public static int DeobedStringNumber;


        static void Main(string[] args)
        {
            Console.WriteLine(@"__________       ___.          .__  ________              ___.    ");
            Console.WriteLine(@"\______   \_____ \_ |__   ____ |  | \______ \   ____  ____\_ |__  ");
            Console.WriteLine(@" |    |  _/\__  \ | __ \_/ __ \|  |  |    |  \_/ __ \/  _ \| __ \ ");
            Console.WriteLine(@" |    |   \ / __ \| \_\ \  ___/|  |__|    `   \  ___(  <_> ) \_\ \");
            Console.WriteLine(@" |______  /(____  /___  /\___  >____/_______  /\___  >____/|___  /");
            Console.WriteLine(@"        \/      \/    \/     \/             \/     \/          \/ ");
            Console.WriteLine("                                                 V3.1 - XenocodeRCE");
            try
            {
                asm = ModuleDefMD.Load(args[0]);
                Console.WriteLine("[!]Loading assembly " + asm.FullName);
                asmpath = args[0];
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!]Error : Cannot load the file. Make sure it's a valid .NET file !");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            if (Core.Helper.BabelHelper.IsFreeVersion(asm))
            {
                //free
                Console.WriteLine("[!]Removing demo ...");
                try
                {
                    Core.Helper.BabelHelper.RemoveDemoFree(asm);
                    Console.WriteLine("[!]Demo limitation removed !");
                }
                catch (Exception)
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!]Demo limitation not found or not removed !");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.WriteLine("[!]Retrieving the string decryption method ...");
                //STRINGS
                var decmeth = Core.Helper.BabelHelper.FindStringDecrypterMethods(asm);
                if(decmeth != null)
                {
                    Console.WriteLine("[!]Decrypting Strings and Replacing Calls ...");
                    DeobedStringNumber = Core.Helper.BabelHelper.DecryptStringsInMethod(asm);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[!] " + DeobedStringNumber + " strings has been decrypte!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!]Error : Decryption Method not foud!");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                //INTEGERS32
                Console.WriteLine("[!]Retrieving the integer32 decryption method ...");
                decmeth = Core.Helper.BabelHelper.FindInteger32DecrypterMethods(asm);
                var DeobedIntegers = 0;
                if (decmeth != null)
                {
                    Console.WriteLine("[!]Decrypting Integers32 and Replacing Calls ...");
                    DeobedIntegers = Core.Helper.BabelHelper.DecryptIntegers32InMethod(asm);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[!] " + DeobedIntegers + " Integers32 has been decrypte!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!]Error : Decryption Method not foud!");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                //INTEGERS64
                Console.WriteLine("[!]Retrieving the integer64 decryption method ...");
                decmeth = Core.Helper.BabelHelper.FindInteger64DecrypterMethods(asm);
                if (decmeth != null)
                {
                    Console.WriteLine("[!]Decrypting Integers64 and Replacing Calls ...");
                    DeobedIntegers = Core.Helper.BabelHelper.DecryptIntegers64InMethod(asm);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[!] " + DeobedIntegers + " Integers64 has been decrypte!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!]Error : Decryption Method not foud!");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                //FLOAT32
                Console.WriteLine("[!]Retrieving the Float32 decryption method ...");
                decmeth = Core.Helper.BabelHelper.FindFloat32DecrypterMethods(asm);
                if (decmeth != null)
                {
                    Console.WriteLine("[!]Decrypting Float32 and Replacing Calls ...");
                    DeobedIntegers = Core.Helper.BabelHelper.DecryptFloat32InMethod(asm);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[!] " + DeobedIntegers + " Float32 has been decrypte!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!]Error : Decryption Method not foud!");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                //FLOAT64
                Console.WriteLine("[!]Retrieving the Float64 decryption method ...");
                decmeth = Core.Helper.BabelHelper.FindFloat64DecrypterMethods(asm);
                if (decmeth != null)
                {
                    Console.WriteLine("[!]Decrypting Float64 and Replacing Calls ...");
                    DeobedIntegers = Core.Helper.BabelHelper.DecryptFloat64InMethod(asm);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[!] " + DeobedIntegers + " Float64 has been decrypte!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!]Error : Decryption Method not foud!");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
            }
            else
            {
                //premium
                Console.WriteLine("[!]Retrieving every call to the decryption method ...");
                var number_of_calls = Core.Helper.BabelHelper.GetCalls_premium();
                if (number_of_calls == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!]Error : Decryption Method is never called !");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                else
                {
                    Console.WriteLine("[!]Decryption method is called " + number_of_calls + " times.");
                    Console.WriteLine("[!]Decrypting Strings and Replacing Calls ...");
                    DeobedStringNumber = Core.Helper.BabelHelper.DecrypStrings_premium();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[!] " + DeobedStringNumber + " strings have been decrypted!");
                    Console.ForegroundColor = ConsoleColor.White;
                }

            }

            //save module
            string text2 = Path.GetDirectoryName(args[0]);
            if (!text2.EndsWith("\\"))
            {
                text2 += "\\";
            }
            string path = text2 + Path.GetFileNameWithoutExtension(args[0]) + "_patched" +
                          Path.GetExtension(args[0]);
            var opts = new ModuleWriterOptions(asm);
            opts.Logger = DummyLogger.NoThrowInstance;
            asm.Write(path, opts);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[!]Assembly saved ! !");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace IONA.Tools
{
    /// <summary>
    /// C# Script runner - Compile on-the-fly and run C# scripts
    /// </summary>
    class CSharpRunner
    {
        private static readonly Dictionary<ulong, Assembly> CompileCache = new Dictionary<ulong, Assembly>();
        private static bool _cacheScripts = false;

        /// <summary>
        /// Determine whether scripts should be cached or not for faster loading times
        /// </summary>
        public static bool CacheScripts
        {
            get
            {
                return _cacheScripts;
            }
            set
            {
                _cacheScripts = value;
                if (!_cacheScripts)
                    CompileCache.Clear();
            }
        }

        /// <summary>
        /// Run the specified C# script file
        /// </summary>
        /// <param name="comHandler">Communication handler for forwarding messages</param>
        /// <param name="lines">Script file to run</param>
        /// <param name="args">Arguments to pass to the script</param>
        /// <param name="run">Set to false to compile and cache the script without launching it</param>
        /// <exception cref="CSharpException">Thrown if an error occured</exception>
        /// <returns>Result of the execution, returned by the script</returns>
        public static object Run(ComHandler comHandler, string[] lines, string[] args, bool run = true)
        {
            //Script compatibility check for handling future versions differently
            if (lines.Length < 1 || lines[0] != "//IONAScript 1.0")
                throw new CSharpException(CSErrorType.InvalidScript, new InvalidDataException());

            ulong scriptHash = QuickHash(lines);
            Assembly assembly = null;

            if (!CacheScripts || !CompileCache.ContainsKey(scriptHash))
            {
                //Process different sections of the script file
                bool scriptMain = true;
                List<string> script = new List<string>();
                List<string> extensions = new List<string>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("//IONAScript"))
                    {
                        if (line.EndsWith("Extensions"))
                            scriptMain = false;
                    }
                    else if (scriptMain)
                        script.Add(line);
                    else extensions.Add(line);
                }

                //Add return statement if missing
                if (script.All(line => !line.StartsWith("return ") && !line.Contains(" return ")))
                    script.Add("return null;");

                //Generate a class from the given script
                string code = String.Join("\n", new string[]
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using System.Text;",
                    "using System.IO;",
                    "using System.Threading;",
                    "using IONA;",
                    "using IONA.Tools;",
                    "namespace ScriptLoader {",
                    "public class Script {",
                    "public CSharpAPI IONA;",
                    "public object __run(CSharpAPI __apiHandler, string[] args) {",
                        "this.IONA = __apiHandler;",
                        String.Join("\n", script),
                    "}",
                        String.Join("\n", extensions),
                    "}}",
                });

                //Compile the C# class in memory using all the currently loaded assemblies
                CSharpCodeProvider compiler = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                parameters.ReferencedAssemblies
                    .AddRange(AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(a => !a.IsDynamic)
                            .Select(a => a.Location).ToArray());
                parameters.CompilerOptions = "/t:library";
                parameters.GenerateInMemory = true;
                CompilerResults result = compiler.CompileAssemblyFromSource(parameters, code);

                //Process compile warnings and errors
                if (result.Errors.Count > 0)
                    throw new CSharpException(CSErrorType.LoadError,
                        new InvalidOperationException(result.Errors[0].ErrorText));

                //Retrieve compiled assembly
                assembly = result.CompiledAssembly;
                if (CacheScripts)
                    CompileCache[scriptHash] = result.CompiledAssembly;
            }
            else if (CacheScripts)
                assembly = CompileCache[scriptHash];

            //Run the compiled assembly with exception handling
            if (run)
            {
                try
                {
                    object compiledScript
                        = CompileCache[scriptHash].CreateInstance("ScriptLoader.Script");
                    return
                        compiledScript
                        .GetType()
                        .GetMethod("__run")
                        .Invoke(compiledScript,
                            new object[] { new CSharpAPI(comHandler), args });
                }
                catch (Exception e) { throw new CSharpException(CSErrorType.RuntimeError, e); }
            }
            else return null;
        }

        /// <summary>
        /// Cache all scripts for the specified directory
        /// </summary>
        /// <param name="directory">Directory to look for scripts</param>
        public static void BuildScriptCache(string directory)
        {
            CacheScripts = true;
            if (Directory.Exists(directory))
                foreach (string scriptFile in Directory.EnumerateFiles(directory, "*.cs"))
                    try { Run(null, File.ReadAllLines(scriptFile), null, false); }
                    catch (CSharpException) { /* Invalid script */ }
        }

        /// <summary>
        /// Quickly calculate a hash for the given script
        /// </summary>
        /// <param name="lines">script lines</param>
        /// <returns>Quick hash as unsigned long</returns>
        private static ulong QuickHash(string[] lines)
        {
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    hashedValue += lines[i][j];
                    hashedValue *= 3074457345618258799ul;
                }
                hashedValue += '\n';
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }

    /// <summary>
    /// Describe a C# script error type
    /// </summary>
    public enum CSErrorType { FileReadError, InvalidScript, LoadError, RuntimeError };

    /// <summary>
    /// Describe a C# script error with associated error type
    /// </summary>
    public class CSharpException : Exception
    {
        private CSErrorType _type;
        public CSErrorType ExceptionType { get { return _type; } }
        public override string Message { get { return InnerException.Message; } }
        public override string ToString() { return InnerException.ToString(); }
        public CSharpException(CSErrorType type, Exception inner)
            : base(inner != null ? inner.Message : "", inner)
        {
            _type = type;
        }
    }

    /// <summary>
    /// Represents the C# API object accessible from C# Scripts
    /// </summary>
    public class CSharpAPI
    {
        private ComHandler _comHandler;
        private static readonly Dictionary<string, object> _vars = new Dictionary<string, object>();

        /// <summary>
        /// Create a new C# API Wrapper
        /// </summary>
        /// <param name="internalComHandler">The internal com handler</param>
        public CSharpAPI(ComHandler internalComHandler)
        {
            _comHandler = internalComHandler;
        }

        /// <summary>
        /// Output a message line
        /// </summary>
        /// <param name="line">Message line</param>
        public void OutputMessage(string line)
        {
            _comHandler.OutputMessage(line);
        }

        /// <summary>
        /// Perform an internal command
        /// </summary>
        /// <param name="command">Internal command to perform</param>
        public void PerformCommand(string command)
        {
            _comHandler.PerformCommand(command);
        }

        /// <summary>
        /// Get a global variable by name
        /// </summary>
        /// <param name="varName">Name of the variable</param>
        /// <returns>Value of the variable or null if no variable</returns>
        public object GetVar(string varName)
        {
            lock (_vars)
            {
                if (_vars.ContainsKey(varName))
                    return _vars[varName];
                return null;
            }
        }

        /// <summary>
        /// Get a global variable by name, as a string
        /// </summary>
        /// <param name="varName">Name of the variable</param>
        /// <returns>Value of the variable as string, or null if no variable</returns>
        public string GetVarAsString(string varName)
        {
            object val = GetVar(varName);
            if (val != null)
                return val.ToString();
            return null;
        }

        /// <summary>
        /// Get a global variable by name, as an integer
        /// </summary>
        /// <param name="varName">Name of the variable</param>
        /// <returns>Value of the variable as int, or 0 if no variable or not a number</returns>
        public int GetVarAsInt(string varName)
        {
            if (GetVar(varName) is int)
                return (int)GetVar(varName);
            int result;
            if (int.TryParse(GetVarAsString(varName), out result))
                return result;
            return 0;
        }

        /// <summary>
        /// Get a global variable by name, as a boolean
        /// </summary>
        /// <param name="varName">Name of the variable</param>
        /// <returns>Value of the variable as bool, or false if no variable or not a boolean</returns>
        public bool GetVarAsBool(string varName)
        {
            if (GetVar(varName) is bool)
                return (bool)GetVar(varName);
            bool result;
            if (bool.TryParse(GetVarAsString(varName), out result))
                return result;
            return false;
        }

        /// <summary>
        /// Set a global variable for further use in any other script
        /// </summary>
        /// <param name="varName">Name of the variable</param>
        /// <param name="varValue">Value of the variable</param>
        public void SetVar(string varName, object varValue)
        {
            lock (_vars)
            {
                _vars[varName] = varValue;
            }
        }

        /// <summary>
        /// Synchronously call another script and retrieve the result
        /// </summary>
        /// <param name="script">Script to call</param>
        /// <param name="args">Arguments to pass to the script</param>
        /// <returns>An object returned by the script, or null</returns>
        public object CallScript(string script, string[] args)
        {
            string[] lines = null;
            Commands.Script.LookForScript(ref script);
            try { lines = File.ReadAllLines(script); }
            catch (Exception e) { throw new CSharpException(CSErrorType.FileReadError, e); }
            return CSharpRunner.Run(_comHandler, lines, args);
        }

        /// <summary>
        /// Get Sunrise for current date
        /// </summary>
        public DateTime TimeSunrise
        {
            get
            {
                return TimeZone.CurrentTimeZone.ToLocalTime(SolarInfo.ForDate(Settings.Latitude, Settings.Longitude, DateTime.UtcNow).Sunrise);
            }
        }

        /// <summary>
        /// Get Sunrise for current date
        /// </summary>
        public DateTime TimeSunset
        {
            get
            {
                return TimeZone.CurrentTimeZone.ToLocalTime(SolarInfo.ForDate(Settings.Latitude, Settings.Longitude, DateTime.UtcNow).Sunset);
            }
        }
    }
}

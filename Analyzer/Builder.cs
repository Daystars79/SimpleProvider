﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using SimpleProvider.Analyzer.Structures;



namespace SimpleProvider.Analyzer
{
    /// <summary>
    ///     Build Dyamic Types and Compiles Emited C# code at run time.
    /// </summary>
    public class Builder
    {
        private readonly SnippetContainer _source;
        private Assembly _assembly;

        /// <summary>
        ///     Create instance of the builder
        /// </summary>
        /// <param name="source">Source file generated by the analyzer.</param>
        public Builder(SnippetContainer source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            LoadAssembly();
            if (_assembly != null) LoadTypes();
        }

        /// <summary>
        ///     Collection of the compiled types contained in the Builder
        /// </summary>
        public Type[] AssemblyTypes { get; set; } = new Type[0];

        /// <summary>
        ///     True if the source has been compiled.
        /// </summary>
        public bool Compiled { get; set; }

        /// <summary>
        ///     Return the Specified Type
        /// </summary>
        /// <param name="typeName">Name of the Type</param>
        /// <returns></returns>
        public Type GetTypeByName(string typeName)
        {
            return AssemblyTypes.FirstOrDefault(fd => fd.Name == typeName);
        }

        private void LoadAssembly()
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText($"{_source.Template}");
            string assemblyName = _source.Namespace;
            string path = RuntimeEnvironment.GetRuntimeDirectory();


            MetadataReference[] references =
            {
                MetadataReference.CreateFromFile(path + "\\system.runtime.dll"),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Provider).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { tree }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using MemoryStream ms = new();
            EmitResult result = compilation.Emit(ms);
            if (!result.Success) return;

            Compiled = true;
            _assembly = Assembly.Load(ms.ToArray()); /* Load the _assembly */
        }
        private void LoadTypes()
        {
            if (_assembly == null) return;
            if (_source.Types.Count <= 0) return;
            AssemblyTypes = new Type[_source.Types.Count];

            for (int index = 0; index < _source.Types.Count; index++)
            {
                Type type = _assembly.CreateInstance(_source.Types[index])?.GetType();
                if (type != null) AssemblyTypes[index] = type;
            }
        }

        /// <summary>
        /// Build Provided Source Code and return an assembly 
        /// </summary>
        /// <param name="name">Name of the new Assembly</param>
        /// <param name="source">C# Source Code to Build</param>
        /// <returns></returns>
        public static Assembly Build(string name, string source)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            string assemblyName = name;
            string path = RuntimeEnvironment.GetRuntimeDirectory();

            MetadataReference[] references =
            {
#if (NET5_0)
                MetadataReference.CreateFromFile(path + "\\system.runtime.dll"),
#endif
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Provider).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] {tree}, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using MemoryStream ms = new();
            EmitResult result = compilation.Emit(ms);
            if(result.Success) return Assembly.Load(ms.ToArray());
            return null;
        }
    }
}
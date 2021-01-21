using System;
using System.Collections.Generic;
using System.Linq;
using SimpleProvider.Analyzer.Extension;

namespace SimpleProvider.Analyzer.Structures
{
    /// <summary>
    ///     Snippet Container - Contains Snippets used by the builder for generating code at run time.
    /// </summary>
    public class SnippetContainer
    {
        private string _ns = string.Empty;

        /// <summary>
        ///     Blank instance
        /// </summary>
        public SnippetContainer()
        {
        }

        /// <summary>
        ///     Blank Instance
        /// </summary>
        /// <param name="ns">Default Namespace</param>
        public SnippetContainer(string ns)
        {
            Namespace = ns;
        }

        /// <summary>
        ///     Contained Code Snippets
        /// </summary>
        internal List<Snippet> Snippets { get; set; } = new();

        /// <summary>
        ///     Namespace for the contained types
        /// </summary>
        public string Namespace
        {
            get => _ns;
            set
            {
                if (Snippets.Count > 0)
                {
                    Types.Clear();
                    for (var x = 0; x < Snippets.Count; x++) Types.Add($"{value}.{Snippets[x].Name}");
                }

                _ns = value;
            }
        }

        /// <summary>
        ///     Code Template
        /// </summary>
        public string Template => Generate();

        /// <summary>
        ///     Type Names contained in the Snippets
        /// </summary>
        public List<string> Types { get; set; } = new();

        /// <summary>
        ///     Assemblies contained in the Using Block
        /// </summary>
        public List<string> Using { get; } = new()
            {"System", "System.Collections.Generic", "System.Linq", "SimpleProvider", "SimpleProvider.Attributes"};

        #region Code Generation

        private string Generate()
        {
            var template = string.Empty;

            for (var x = 0; x < Using.Count; x++) template += $"using {Using[x]};{Environment.NewLine}";
            /* Set the name space */
            template +=
                $@"{Environment.NewLine}#nullable enable{Environment.NewLine}namespace {Namespace}{Environment.NewLine}{'{'}{Environment.NewLine}{Environment.NewLine}";

            for (var x = 0; x < Snippets.Count; x++)
            {
                template += $"{Environment.NewLine}";
                template += $"{Snippets[x]}{Environment.NewLine}";
                template += $"{Environment.NewLine}";
            }

            template += $"{Environment.NewLine}{'}'}";
            return template;
        }

        #endregion

        /// <summary>
        ///     Returns the generated code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Generate();
        }

        #region Collection Methods

        /// <summary>
        ///     Add new code snippet to the collection.
        /// </summary>
        /// <param name="table">Table from the Analyzer</param>
        public void AddSnippet(Table table)
        {
            if (table == null) return;
            if (table.Columns.Count <= 0) return;
            /* Prevent Duplicates */
            if (Snippets.All(a => a.Name != table.ClassName.ToPascalCase())) Snippets.Add(new Snippet(table));
            if (!Types.Contains($"{Namespace}.{table.ClassName.ToPascalCase()}"))
                Types.Add($"{Namespace}.{table.ClassName.ToPascalCase()}");
        }

        /// <summary>
        ///     Remove an Code Snippet
        /// </summary>
        /// <param name="table">Table from the Analyzer</param>
        public void RemoveSnippet(Table table)
        {
            if (table == null) return;
            if (table.Columns.Count <= 0) return;

            var clsfile = Snippets.FirstOrDefault(fd => fd.Name == table.ClassName?.ToPascalCase());
            Snippets.Remove(clsfile);
            Types.Remove($"{Namespace}.{table.ClassName}");
        }

        /// <summary>
        ///     Clears the Snippet Collections
        /// </summary>
        public void Clear()
        {
            Snippets.Clear();
            Types.Clear();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleProvider.Analyzer.Enums;
using SimpleProvider.Analyzer.Extension;

namespace SimpleProvider.Analyzer.Structures
{
    internal class Snippet
    {
        private readonly CodeType codeType = CodeType.Class;

        public Snippet()
        {
        }

        public Snippet(Table table)
        {
            Generate(table);
        }

        public string Definition { get; set; }
        public string Name { get; set; }
        public List<string> Properties { get; set; } = new();

        private string CreateDefinition(Table table)
        {
            if (table == null) return string.Empty;
            if (table.Columns.Count <= 0)
                return $@" [Definition(""{table.Name}"", ""{table.Schema}"")]{Environment.NewLine}";
            var columns = new List<Column>();
            columns.AddRange(table.Columns.Where(w => w.PK));

            if (columns.Count > 0)
            {
                columns = columns.OrderBy(ob => ob.Id).ToList();
                var keys = string.Empty;
                for (var x = 0; x < columns.Count; x++) keys += $@"""{columns[x].Name}"",";
                keys = keys.Substring(0, keys.Length - 1);
                return $@" [Definition(""{table.Name}"", ""{table.Schema}"", {keys})]{Environment.NewLine}";
            }

            return $@" [Definition(""{table.Name}"", ""{table.Schema}"")]{Environment.NewLine}";
        }

        private void Generate(Table table)
        {
            if (table == null) return;
            if (table.Columns.Count <= 0) return;

            Definition = CreateDefinition(table);
            Name = table.ClassName.ToPascalCase();
            for (int index = 0; index < table.Columns.Count; index++)
            {
                Column c = table.Columns[index];

                string output = string.Empty;
                /* Just create an default column attribute */
                string attrib =
                    $@"     [Column(IsScope = {c.Identity}, DataType = typeof({c.NetType}), IsNullable = {c.Nullable}, Length = {c.Size}, Name = ""{c.Name}"")]{Environment.NewLine}";

                attrib = attrib.Replace("True", "true");
                attrib = attrib.Replace("False", "false");

                /* Additional Rules */
                //if (c.Nullable) attrib = attrib.Replace($" DataType = typeof({c.NetType}),", "");


                /* Default behavior */
                output += attrib;
                output += $"     public {c.NetType} {c.Name?.ToPascalCase()} " + "{ get; set; }" + Environment.NewLine;
                Properties.Add(output);
            }
        }

        private string GenerateTemplate()
        {
            string t_type = string.Empty;

            switch (codeType)
            {
                case CodeType.Class:
                    t_type = "class";
                    break;
                case CodeType.Struct:
                    t_type = "struct";
                    break;
                case CodeType.IFace:
                    t_type = "interface";
                    break;
            }

            var template = $@"{Environment.NewLine}{Definition} public {t_type} {Name}{Environment.NewLine}  {'{'}{Environment.NewLine}";
            for (var index = 0; index < Properties.Count; index++) template += Properties[index];
            return template + "  }";
        }

        /* Use this to return the code for an class file */
        public override string ToString()
        {
            return GenerateTemplate();
        }
    }
}
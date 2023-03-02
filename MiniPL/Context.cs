using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    using SymbolTable = Dictionary<string, TableEntry>;
    struct TableEntry
    {
        public readonly string Type;
        public object? Value { get; set; }
        public TableEntry(string type, object? value)
        {
            Type = type;
            Value = value;
        }
        public TableEntry(string type) : this(type, null) { }
    }
    internal class Context
    {
        public SymbolTable Table { get; protected set; }
        public Context()
        {
            Table = new SymbolTable();
        }
        public void Declare(string name, string type, object? value = null)
        {
            Table[name] = new TableEntry(type);
            if (value != null) Assign(name, type, value);
        }
        public void Assign(string name, string type, object value)
        {
            Table[name] = new TableEntry(type, value);
        }
        public bool ContainsVariable(string name)
        {
            return Table.ContainsKey(name);
        }
        public TableEntry GetVariable(string name)
        {
            return Table[name];
        }
        public string GetVariableType(string name)
        {
            return Table[name].Type;
        }
        public object? GetVariableValue(string name)
        {
            return Table[name].Value;
        }
    }
}

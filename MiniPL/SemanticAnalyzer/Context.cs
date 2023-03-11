namespace MiniPL
{
    using SymbolTable = Dictionary<string, TableEntry>;

    // Entry of symbol table which stores variable's type and optionally value
    public struct TableEntry
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
    // Singleton class context which stores the symbol table
    public class Context
    {
        public SymbolTable Table { get; protected set; }
        private static Context? _instance;
        private Context()
        {
            Table = new();
        }
        public static Context GetInstance()
        {
            if (_instance == null)
            {
                _instance = new();
            }
            return _instance;
        }
        // Clear table after program execution
        public void ClearTable()
        {
            Table.Clear();
        }
        // Declare a variable in symbol table
        public void Declare(string name, string type, object? value = null)
        {
            Table[name] = new(type);
            if (value != null) Assign(name, value);
        }
        // Assign a value to a variable
        public void Assign(string name, object value)
        {
            TableEntry entry = Table[name];
            entry.Value = value;
            Table[name] = entry;
        }
        // Check if variable is in symbol table
        public bool ContainsVariable(string name)
        {
            return Table.ContainsKey(name);
        }
        // Get variable's type
        public string GetVariableType(string name)
        {
            TableEntry entry;
            Table.TryGetValue(name, out entry);
            return entry.Type;
        }
        // Get variable's value
        public object? GetVariableValue(string name)
        {
            TableEntry entry;
            Table.TryGetValue(name, out entry);
            return entry.Value;
        }
    }
}

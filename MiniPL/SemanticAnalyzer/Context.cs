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
        public void Declare(string name, string type, object? value = null)
        {
            Table[name] = new(type);
            if (value != null) Assign(name, value);
        }
        public void Assign(string name, object value)
        {
            TableEntry entry = Table[name];
            entry.Value = value;
            Table[name] = entry;
        }
        public bool ContainsVariable(string name)
        {
            return Table.ContainsKey(name);
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

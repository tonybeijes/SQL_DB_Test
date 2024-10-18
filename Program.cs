using System.Reflection;
using System.Data.SQLite;


public class VisionData
{
    public required string Date {  get; set; }
    public required int Result { get; set; }
    public required double MeasuredValue { get; set; }
    public required double LocationX { get; set; }

}

public class SQLiteTable
{
    public required string fileName { get; set; }
    public required string name { get; set; }
    public required string path { get; set; }


}


class Program
{
    static void Main()
    {
        // Create an instance of VisionData
        var visionData = new VisionData
        {
            Date = "October 19",
            Result = 35,
            MeasuredValue = 91.5,
            LocationX = 29.35
        };

        // Create an object with all needed data for SQLite table
        var table = new SQLiteTable
        {
            fileName = "test.db",
            path = "C:\\Users\\Anthony.Beijes\\Downloads\\test.db",
            name = "Table1"
        };

        CreateTable(table, visionData);
        InsertDataIntoTable(table, visionData);
        ReadDataFromTable(table);
       
    }

    // Map C# types to SQLite types
    static string GetSQLiteType(Type type)
    {
        if (type == typeof(string)) return "TEXT";
        if (type == typeof(int)) return "INTEGER";
        if (type == typeof(double)) return "REAL";
        throw new Exception("Unsupported data type");
    }

    static void CreateTable(SQLiteTable table, VisionData data)
    {
        string connectionStr = $"Data Source={table.path};Version=3;New=True;";

        using (SQLiteConnection conn = new SQLiteConnection(connectionStr))
        {
            conn.Open();

            Type type = data.GetType();
            PropertyInfo[] properties = type.GetProperties();

            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {table.name} (";

            // Loop through the properties to build query
            foreach (var property in properties)
            {
                string columnName = property.Name;
                string columnType = GetSQLiteType(property.PropertyType);

                createTableQuery += $"{columnName} {columnType}, ";
            }

            // Remove trailing comma and space
            createTableQuery = createTableQuery.TrimEnd(',', ' ') + ");";

            // Create table
            using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, conn))
            {
                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }
    }

    // Insert data into the table
    static void InsertDataIntoTable(SQLiteTable table, VisionData data)
    {
        string connectionStr = $"Data Source={table.path};Version=3;New=True;";

        // Get properties
        Type type = data.GetType();
        PropertyInfo[] properties = type.GetProperties();

        // Initialize query
        string insertQuery = $"INSERT INTO {table.name} (";

        string columnNames = "";
        string columnValues = "";

        // Build query
        foreach (var property in properties)
        {
            string columnName = property.Name;
            object columnValue = property.GetValue(data);

            columnNames += $"{columnName}, ";
            columnValues += $"'{columnValue}', ";
        }

        // Remove trailing commas and spaces
        insertQuery += columnNames.TrimEnd(',', ' ') + ") VALUES (" + columnValues.TrimEnd(',', ' ') + ");";

        // Execute query
        using (SQLiteConnection conn = new SQLiteConnection(connectionStr))
        {
            conn.Open();

            using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
            {
                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }
        
    }

    static void ReadDataFromTable(SQLiteTable table)
    {
        string connectionStr = $"Data Source={table.path};Version=3;New=True;";

        string selectQuery = $"SELECT * FROM {table.name}";

        // Execute query
        using (SQLiteConnection conn = new SQLiteConnection(connectionStr))
        {
            conn.Open();

            using (SQLiteCommand cmd = new SQLiteCommand(selectQuery, conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("No data found.");
                        return;
                    }

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object value = reader.GetValue(i);
                            Console.WriteLine($"{columnName}: {value}");
                        }
                        Console.WriteLine();
                    }
                }
            }


        }
    }
}
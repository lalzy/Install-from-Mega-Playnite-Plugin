using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System.IO;
using System.Collections.Generic;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InstallFromMegaPlugin
{
    public class GameStatsManager
    {
        private string _connection;
        private static readonly System.Reflection.PropertyInfo[] _properties = typeof(GameStats).GetProperties();
        
        public GameStatsManager(Plugin plugin)
        {
            var dbPath = Path.Combine(plugin.GetPluginUserDataPath(), "plugin.db");
            _connection = $"Data Source={dbPath};Version=3;";

            using (var connection = new SQLiteConnection(_connection))
            {
                connection.Open();
                var command = connection.CreateCommand();
                CreateTable(command);
                SetupColums(command);
            }
        }

        /// <summary>Helper to convert from C# type to SQLite type</summary>
        private string GetSQLiteType(Type type){
            switch(type.Name){
                case "Boolean":
                case "UInt64":
                case "Int32":
                    return "INTEGER";
                case "Double":
                case "Single":
                    return "REAL";
                default:
                    return "TEXT";
            }
        }

        /// <summary>Create the GameStats table, with ID column if it doesn't exist</summary>
        private void CreateTable(SQLiteCommand command){
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {nameof(GameStats)} ({nameof(GameStats.GameID)} TEXT PRIMARY KEY)";
            command.ExecuteNonQuery();
        }

        /// <summary>Create or change tables of GameStats table</summary>
        private void SetupColums(SQLiteCommand command){
            var tableName = nameof(GameStats);

            command.CommandText = $"PRAGMA table_info({tableName})";
            var existing = new System.Collections.Generic.HashSet<string>();

            using(var reader = command.ExecuteReader()) while(reader.Read()) existing.Add(reader["name"].ToString());

            foreach(var property in _properties){
                if(property.Name == nameof(GameStats.GameID)) continue;
                else if(existing.Contains(property.Name)) continue;
                command.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {property.Name} {GetSQLiteType(property.PropertyType)}";
              
                command.ExecuteNonQuery();
            }
        }

        /// <summary>Create the Querry string to fetch a GameStats entry from our Database</summary>
        private string CreateReadString(){
            var columns = String.Join(", ", _properties.Select(p => p.Name));

            return $"SELECT {columns.ToString().TrimEnd(',', ' ')} FROM {nameof(GameStats)} WHERE {nameof(GameStats.GameID)} = @{nameof(GameStats.GameID)}";
        }

        /// <summary>Insert data into GameStats table</summary>
        private void InsertQuerry(SQLiteCommand command, GameStats gameStats){
            var columns = new System.Text.StringBuilder();
            var values = new System.Text.StringBuilder();
            
            foreach(var property in _properties){
                var name = property.Name;
                columns.Append(name).Append(", ");
                values.Append($"@{name}").Append(", ");
                command.Parameters.AddWithValue($"@{name}", property.GetValue(gameStats));
            }

            command.CommandText = $"INSERT OR REPLACE INTO {nameof(GameStats)} ({columns.ToString().TrimEnd(',', ' ')}) VALUES ({values.ToString().TrimEnd(',', ' ')})";

            command.ExecuteNonQuery();
        }

        /// <summary>Converts from SQLite to valid C# type</summary>
        private object ConvertToCSharpType(Type targetType, object value){
            return targetType == typeof(ulong) ? (object)Convert.ToUInt64(value) : Convert.ChangeType(value, targetType);
        }
        
        /// <summary>Helper to create a gamestats object from the database entry</summary>
        private GameStats CreateGameStatsObject(SQLiteDataReader reader){
            var result = new GameStats();
            foreach(var property in _properties){
                var value = reader[property.Name];
                if(value == DBNull.Value) continue;
                property.SetValue(result, ConvertToCSharpType(property.PropertyType, value));
            }
            return result;
        }
        
        /// <summary>Write current gamestats object to Database</summary>
        /// <returns></returns>
        public void Write(GameStats gameStats)
        {
            try{
                using (var connection = new SQLiteConnection(_connection))
                {
                    connection.Open();
                    InsertQuerry(connection.CreateCommand(), gameStats);
                }
            }catch(Exception e){
                Console.WriteLine($"Error Writing to Database: {e}");
            }
        }

        /// <summary>Read from the GameStats database</summary>
        /// <param name="gameID">The Database gameID</param>
        /// <returns>the GameStats object from the Database</returns>
        /// <throws> Generic pass-down exception</throws>
        public GameStats Read(Guid gameID)
        {
            try{
                using (var connection = new SQLiteConnection(_connection))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = CreateReadString();
                    command.Parameters.AddWithValue($"@{nameof(GameStats.GameID)}", gameID.ToString());
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        return CreateGameStatsObject(reader);
                    }
                }
            }catch(Exception e){
                Console.WriteLine($"Error Writing to Database: {e}");
            }
            return null;
        }
    }
}

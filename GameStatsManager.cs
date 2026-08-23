using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
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
        private IPlayniteAPI _api;
        private string _connection;
        private static readonly System.Reflection.PropertyInfo[] _properties = typeof(GameStats).GetProperties();

        public GameStatsManager(Plugin plugin, IPlayniteAPI api)
        {
            _api = api;
            var dbPath = Path.Combine(plugin.GetPluginUserDataPath(), "plugin.db");
            _connection = $"Data Source={dbPath};Version=3;";
            WithSQLiteCommand((command) =>
            {
                CreateTable(command);
                SetupColums(command);
            }, "Error initializing GameStatsManager");
        }
        
        private void WithSQLiteCommand(Action<SQLiteCommand> action, string commandText=null, string error=null){
            ErrorHandler.WithTryCatch(() => {
                using(var connection = new SQLiteConnection(_connection)){
                    connection.Open();
                    using(var command = connection.CreateCommand()){
                        if(commandText != null) command.CommandText = commandText;
                        action(command);
                    }
                }
            }, _api, error);
        }

        private T WithReturnsSQLiteCommand<T>(Func<SQLiteCommand, T> action, string commandText=null, string error=null){
            ErrorHandler.WithTryCatchReturn<T>(() => {
                using(var connection = new SQLiteConnection(_connection)){
                    connection.Open();
                    using(var command = connection.CreateCommand()){
                        if(commandText != null) command.CommandText = commandText;
                        return action(command);
                    }
                }
            }, _api);
            return default;
        }
        
        private string GetSQLiteType(Type type){
            if (type == typeof(Guid)) return "TEXT";
            if (type == typeof(bool)) return "INTEGER";
            if (type == typeof(ulong)) return "INTEGER";
            return "TEXT";
        }
        
        private HashSet<Guid> GetGuidHashSet(){
            var GuidSet = new HashSet<Guid>();

            WithSQLiteCommand((command) => {
                using(var reader = command.ExecuteReader()){
                    while(reader.Read()){
                        GuidSet.Add(reader.GetGuid(0));
                    }
                }
            }, $"SELECT {nameof(GameStats.GameID)} FROM {nameof(GameStats)}", "Error fetching Guids");

            return GuidSet;
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
        private GameStats CreateGameStatsObject(Dictionary<string, object> values){
            var result = new GameStats();
            foreach(var property in _properties){
                if(values[property.Name] == DBNull.Value) continue;
                property.SetValue(result, ConvertToCSharpType(property.PropertyType, values[property.Name]));
            }
            return result;
        }
        
        private void UpdatePlayniteObject(Playnite.SDK.Models.Game game, GameStats stats){
            if(stats == null)  stats = Write(new GameStats { GameID = game.Id, IsInstalled = false, Playtime = 0 });
            
            game.IsInstalled = stats.IsInstalled;
            game.Playtime = stats.Playtime;
            game.Version = stats.Version;
            _api.Database.Games.Update(game);
        }
        
        /// <summary>Write current gamestats object to Database</summary>
        /// <returns></returns>
        public GameStats Write(GameStats gameStats)
        {
            WithSQLiteCommand((command) => {
            InsertQuerry(command, gameStats);
            }, "Error writing to Database");
            return gameStats;
        }

        private Dictionary<string, object> convertReaderToObject(SQLiteDataReader reader){
            var values = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++){
                values[reader.GetName(i)] = reader.GetValue(i);
            }
            return values;
        }
        
        /// <summary>Read from the GameStats database</summary>
        /// <param name="gameID">The Database gameID</param>
        /// <returns>the GameStats object from the Database</returns>
        /// <throws> Generic pass-down exception</throws>
        public GameStats Read(Guid gameID)
        {
            return WithReturnsSQLiteCommand<GameStats>((SQLiteCommand command) => {
                command.CommandText = CreateReadString();
                command.Parameters.AddWithValue($"@{nameof(GameStats.GameID)}", gameID.ToString());
                using(var reader = command.ExecuteReader()){
                    if(!reader.Read()) return null;
                    return CreateGameStatsObject(convertReaderToObject(reader));

                }
            }, "Error writing to Database");
        }

        ///<summary>Get all GameStats objects from the Database</summary>
        ///<returns>All game objects as a dictionary with Guid as key</returns>
        /// <throws> Generic pass-down exception</throws>
        public Dictionary<Guid, GameStats> GetAllGameStats(){
            return WithReturnsSQLiteCommand<Dictionary<Guid, GameStats>>((command) => {
                command.CommandText = $"SELECT * FROM {nameof(GameStats)}";
                using (var reader = command.ExecuteReader()){
                    var map = new Dictionary<Guid, GameStats>();
                    while(reader.Read()){
                        var stats = CreateGameStatsObject(convertReaderToObject(reader));
                        map[stats.GameID] = stats;
                    }
                    return map;
                }
            }, "Error during GetAllGameStats");
        }
        
        /// <summary>Check if the GameStats database is empty or not (for initial run)</summary>
        ///<returns>true if empty</returns>
        /// <throws> Generic pass-down exception</throws>
        public bool IsEmpty(){
            return WithReturnsSQLiteCommand<bool>((command) => {
                command.CommandText = $"SELECT EXISTS(SELECT 1 FROM {nameof(GameStats)})";
                return (long)command.ExecuteScalar() == 0;
            }, "error Checking If Empty");
        }

        /// <summary>Sync the playnite Games.db entries to our playtime entry</summary>
        /// <throws> Generic pass-down exception</throws>
        public void SyncGamesToGameStats(){
            const string ERROR = "Error during syncing Games.DB to GameStats";
            ErrorHandler.WithTryCatch(() =>
            {
                var gameStats = GetAllGameStats();
                foreach(var game in _api.Database.Games){
                    gameStats.TryGetValue(game.Id, out var stats);
                    UpdatePlayniteObject(game, stats);
                }
            }, _api, ERROR);
        }
    }
}

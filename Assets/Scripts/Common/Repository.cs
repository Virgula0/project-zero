using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite; // Use this for SQLite via NuGet
using UnityEngine;

namespace Assets.Scripts.Common
{
    public class Repository
    {
        private readonly string sqliteFileName = "projectzero.sqlite3";
        private IDbConnection dbConnection;
        private string dbPath;

        public enum Tables
        {
            Stats = 0,
        }

        public Repository()
        {
            this.ConnectToDatabase();
            this.CreateStatsTable();
        }

        private void ConnectToDatabase()
        {
            this.dbPath = Path.Combine(Application.persistentDataPath, this.sqliteFileName);
            string connectionString = $"Data Source={this.dbPath};";

            dbConnection = new SqliteConnection(connectionString);
            dbConnection.Open();

            Debug.Log("Connected to SQLite database at: " + this.dbPath);
        }

        private void CreateStatsTable()
        {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Stats (ID INTEGER PRIMARY KEY AUTOINCREMENT, PlayerName TEXT, Score INTEGER);";
                command.ExecuteNonQuery();
            }
            Debug.Log("Table created successfully");
        }

        /*
                rep.InsertData(Repository.Tables.Stats.ToString(), new Dictionary<string, object>{
                    { "PlayerName", "test" },
                    { "Score",2}
                });
        */
        public void InsertData(string tableName, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
            {
                Debug.LogError("InsertData failed: No data provided.");
                return;
            }

            using (var command = dbConnection.CreateCommand())
            {
                // Ensure column names and placeholders are properly formatted
                string columns = string.Join(", ", data.Keys);
                string parameters = string.Join(", ", data.Keys.Select(k => $"@{k}"));

                command.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

                // Add parameters dynamically
                foreach (var pair in data)
                {
                    command.Parameters.Add(new SqliteParameter($"@{pair.Key}", pair.Value));
                }

                command.ExecuteNonQuery();
            }

            Debug.Log($"Inserted data into {tableName} successfully.");
        }


        /*
         * Use this for reading data
                List<Dictionary<string, object>> players = rep.ReadData(Repository.Tables.Stats.ToString());

                foreach (var player in players)
                {
                    Debug.Log($"ID: {player["ID"]}, Name: {player["PlayerName"]}, Age: {player["Score"]}");
                }
        */
        public List<Dictionary<string, object>> ReadData(string tableName)
        {
            List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM {tableName};";

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            row[columnName] = columnValue;
                        }

                        tableData.Add(row);
                    }
                }
            }
            return tableData;
        }

        void OnApplicationQuit()
        {
            dbConnection.Close();
            Debug.Log("Database connection closed.");
        }
    }
}
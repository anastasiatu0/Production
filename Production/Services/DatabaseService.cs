using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace Production.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ProdDB"].ConnectionString;
        }

        public DataTable GetData()
        {
            string query = @"SELECT t.Наименование, t.Описаниe, 
                                t.Цена, t.Скидка, t.Количество, t.Единица_Измерения AS [Единица Измерения], pt.Название AS [Поставщик], pr.Название AS [Производитель], c.Название AS [Категория], t.Путь_Фото 
                            FROM ((Товар AS t     
                                INNER JOIN Поставщик AS pt ON t.Поставщик_Код = pt.Код)
                                INNER JOIN Производитель AS pr ON t.Производитель_Код = pr.Код)
                                INNER JOIN Категория AS c ON t.Категория_Код = c.Код";
            var table = new DataTable();
            using (var connection = new OleDbConnection(_connectionString))
            {
                var command = new OleDbCommand(query, connection);
                var adapter = new OleDbDataAdapter(command);
                adapter.Fill(table);
            }

            table.Columns.Add("Цена_Итог", typeof(string));
            table.Columns.Add("Статус", typeof(string));

            foreach (DataRow row in table.Rows)
            {
                decimal price = row["Цена"] != DBNull.Value ? Convert.ToDecimal(row["Цена"]) : 0m;
                decimal discount = row["Скидка"] != DBNull.Value ? Convert.ToDecimal(row["Скидка"]) : 0m;

                row["Цена_Итог"] = discount > 0 ? $"Цена со скидкой: {price * (1 - discount / 100):C2}" : $"Цена без скидки: {price:C2}";

                int quantity = row["Количество"] != DBNull.Value ? Convert.ToInt32(row["Количество"]) : 0;

                row["Статус"] = quantity == 0 ? "Non" : quantity >= 15 ? "Big" : "Norm";
            }

            var colsToRemove = new[] { "Цена", "Скидка" };
            foreach (var col in colsToRemove)
            {
                if (table.Columns.Contains(col)) table.Columns.Remove(col);
            }

            return table;
        }

        public bool authUser(string email, string password)
        {
            string query = "SELECT COUNT(*) FROM Пользователи WHERE Логин = ? AND Пароль = ?";
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                using (var command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@p1", email.Trim().ToLower());
                    command.Parameters.AddWithValue("@p2", password);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public DataRow GetUserByEmail(string email)
        {
            string query = "SELECT * FROM Пользователи WHERE Логин = ?";
            var table = new DataTable();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                using (var command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@p1", email.Trim().ToLower());
                    var adapter = new OleDbDataAdapter(command);
                    adapter.Fill(table);
                }
            }
            return table.Rows.Count > 0 ? table.Rows[0] : null;

        }

        public DataTable GetUnits()
        {
            return GetDictionary("Единица_Измерения");
        }

        public DataTable GetSuppliers()
        {
            return GetDictionary("Поставщик");
        }

        public DataTable GetManufacturers()
        {
            return GetDictionary("Производитель");
        }

        public DataTable GetCategories()
        {
            return GetDictionary("Категория");
        }

        private DataTable GetDictionary(string tableName)
        {
            string query = $"SELECT Код, Название FROM [{tableName}]";
            var dt = new DataTable();
            using (var conn = new OleDbConnection(_connectionString))
            using (var cmd = new OleDbCommand(query, conn))
            {
                var adapter = new OleDbDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool InsertProduct(string name, string description, decimal price, decimal discount,
    int quantity, int unitId, int supplierId, int manufacturerId, int categoryId, string photoName)
        {
            string query = @"INSERT INTO Товар (Наименование, Описание, Цена, Скидка, Количество, 
                      Единица_Измерения, Поставщик, Производитель, Категория, Путь_Фото) 
                      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (var connection = new OleDbConnection(_connectionString))
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@p1", name);
                command.Parameters.AddWithValue("@p2", string.IsNullOrEmpty(description) ? DBNull.Value : (object)description);
                command.Parameters.AddWithValue("@p3", price);
                command.Parameters.AddWithValue("@p4", discount);
                command.Parameters.AddWithValue("@p5", quantity);
                command.Parameters.AddWithValue("@p6", unitId);
                command.Parameters.AddWithValue("@p7", supplierId);
                command.Parameters.AddWithValue("@p8", manufacturerId);
                command.Parameters.AddWithValue("@p9", categoryId);
                command.Parameters.AddWithValue("@p10", string.IsNullOrEmpty(photoName) ? DBNull.Value : (object)photoName);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}

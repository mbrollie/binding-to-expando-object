using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BindingToExpandoObject.Services
{
    public class EmployeeService
    {
        public static List<ExpandoObject> Data { get; set; } = new List<ExpandoObject>();
        public async Task<List<ExpandoObject>> GetEmployeesAsync()
        {
            if (!Data.Any())
            {
                for (int i = 0; i < 50; i++)
                {
                    ExpandoObject employee = new ExpandoObject();
                    employee.TryAdd("EmployeeId", i);
                    employee.TryAdd("FirstName", $"Employee {i}");
                    employee.TryAdd("LastName", $"Employee {i} Last name");
                    employee.TryAdd("IsActive", i % 2 == 0);
                    employee.TryAdd("HireDate", DateTime.Now.AddMonths(-i));
                    Data.Add(employee);
                }
            }
            return await Task.FromResult(Data);
        }

        public async Task<bool> Add(ExpandoObject model)
        {
            var dictionaryItem = (IDictionary<string, object>)model;
            var tableName = "Employee";
            var connString = "";

            var fields = string.Join(",", dictionaryItem.Keys.Select(field => $"[{field}]"));
            var values = string.Join(",", dictionaryItem.Values.Select(value => $"'{value}'"));
            var affectedRows = 0;
            string sql = $"insert into [{tableName}] ({fields}) values ({values})";

            try
            {
                using var connection = new SqlConnection(connString);
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();

                using var cmd = new SqlCommand(sql, connection);
                affectedRows = cmd.ExecuteNonQuery();

                return affectedRows > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                    return await Update(model);

                return false;
            }
        }

        public async Task<bool> Update(ExpandoObject model)
        {
            var dictionaryItem = (IDictionary<string, object>)model;
            var TableName = "Employee";
            var ColumnKey = "EmployeeId";
            var ColumnValue = 2;
            var connString = "";
            var affectedRows = 0;

            var fields = string.Join(",", dictionaryItem.Select(i => $"[{i.Key}]={i.Value}"));
            
            string sql = $"update [{TableName}] set {fields} where {ColumnKey} = {ColumnValue}";

            try
            {
                using var connection = new SqlConnection(connString);
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();

                using var cmd = new SqlCommand(sql, connection);
                affectedRows = cmd.ExecuteNonQuery();

                return affectedRows > 0;
            }
            catch (SqlException ex)
            {
                return false;
            }
        }

        private string ConcatItems(ICollection<object> items)
        {
            string strBuilder = string.Empty;
            int index = 0;

            foreach (var item in items)
            {
                strBuilder += index == 0 ? "(" : "";

                strBuilder += $"[{item}]";

                strBuilder += index < items.Count() - 1 ? "," : ")";

                index++;
            }

            return strBuilder;
        }
    }
}

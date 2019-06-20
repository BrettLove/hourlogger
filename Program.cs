using System;
using System.Data.SQLite;

namespace sqlite
{
    class Program
    {
        static void Main(string[] args)
        {

            //SQLiteConnection.CreateFile("myDatabase.sqlite");
            Console.WriteLine("Add hours? Hit Enter. Or type 'q' to quit.");

            Log log = new Log();

            while (Console.ReadLine().ToLower() != "q") {
                double hours = getPositiveDouble("Hours: ");
                DateTime input_date = getDateTime("Date: ");
                Day day = new Day(hours, input_date);
                log.Add(day);
                Console.WriteLine($"hour is {day.Hour}  date is {day.Date}");
                //InsertRows(day.Hour, day.Date);
                Console.WriteLine("Add hours? Hit Enter. Or type 'q' to quit.");
            }

            InsertRows(log);
            
            
            // if (!DateTime.TryParse(Console.ReadLine(), out input_date)) {
            //     Console.WriteLine("Date didn't convert.");
            // };

            //DateTime input_date = new DateTime(2016, 7, 15);

            
            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=myDatabase.sqlite;Version=3;");
            dbConnection.Open();

            string sql = "select * from hours order by date asc";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            double total_hours = 0;

            Console.WriteLine();
            Console.WriteLine("Date:\t\tHours:");
            while (reader.Read()) {
                Console.WriteLine(Convert.ToDateTime(reader["date"]).ToString("M/dd/yyyy")
                 + "\t" +
                reader["hours"]);
                total_hours += (double)reader["hours"];
            }

            Console.WriteLine($"Total hours: {total_hours}");

            sql = "select sum(hours) as total_hours from hours";
            command = new SQLiteCommand(sql, dbConnection);

            reader = command.ExecuteReader();

            reader.Read();

            Console.WriteLine("Total hours: {0}", reader["total_hours"]);


            dbConnection.Close();
        }

        static DateTime getDateTime(string message) {
            DateTime input_date;
            do {
                Console.Write(message);
            } while (!DateTime.TryParse(Console.ReadLine(), out input_date));
            return input_date;
        }

        static double getPositiveDouble(string message) {
            double number;
            do {
                Console.Write(message);
                Double.TryParse(Console.ReadLine(), out number);
            } while (number <= 0);
            return number;
        }

        static void InsertRows(Log log) {
            int rows = 0;
            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=myDatabase.sqlite;Version=3;");
            dbConnection.Open();

            foreach (Day day in log.days) {
                Console.WriteLine($"date {day.Date}  hour {day.Hour}");
            
                using (SQLiteTransaction tr = dbConnection.BeginTransaction()) {

                    using (SQLiteCommand command = dbConnection.CreateCommand()) {
                        command.Transaction = tr;
                        string sql = "insert into hours (hours, date) values (@hours, @date)";
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("@hours", day.Hour));
                        command.Parameters.Add(new SQLiteParameter("@date", day.Date.ToString("yyyy-MM-dd")));
                        rows += command.ExecuteNonQuery();
                    }
                    
                tr.Commit();
                }
            }

            
            
            //SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            
            Console.WriteLine($"Inserted {rows} rows.");
            dbConnection.Close();
            
            
        }
    }
}

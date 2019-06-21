using System;
using System.Data.SQLite;

namespace sqlite
{
    class Program
    {

        static private string year = "2019";

        static void Main(string[] args)
        {

            //SQLiteConnection.CreateFile("myDatabase.sqlite");


//          view log without adding hours OR 
//          add hours (in a loop) then either:
//              save and view log
//              quit without saving


            Console.WriteLine("Hour Logger");
            Console.WriteLine();
            Console.Write("View log (l), add hours (h), or quit (q)  ");

            string choice = Console.ReadLine().ToLower();

            while (choice != "q") {
                switch (choice) {
                    case "l":
                        // view log without adding hours
                        Console.WriteLine();
                        //Console.WriteLine("You are viewing the log without adding hours.");
                        viewlog();
                        Console.Write("View log (l), add hours (h), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;
                    
                    case "h":
                        // add hours (in a loop) then either:
                        // save and view log
                        // or quit without saving
                        //Console.WriteLine("You are adding hours.");
                        addhours();
                        Console.Write("View log (l), add hours (h), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;

                    default: 
                        Console.WriteLine("Could not understand your choice.");
                        Console.Write("View log (l), add hours (h), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;
                }
            }

            // Console.WriteLine("Add hours? Hit Enter. Or type 'q' to quit.");    

            
            // if (!DateTime.TryParse(Console.ReadLine(), out input_date)) {
            //     Console.WriteLine("Date didn't convert.");
            // };

            //DateTime input_date = new DateTime(2016, 7, 15);

        }

        static DateTime getDateTime(string message) {
            DateTime input_date;
            do {
                Console.Write(message);
            } while (!DateTime.TryParse(Console.ReadLine() + "/" + year, out input_date));
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

        static void viewlog() {
            Console.WriteLine("Log");
            Console.WriteLine("------------");

            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=myDatabase.sqlite;Version=3;");
            dbConnection.Open();

            string sql = "select date, hours from hours order by date asc";
            using (SQLiteCommand command = new SQLiteCommand(sql, dbConnection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {

                    double total_hours = 0;

                    //Console.WriteLine();
                    Console.WriteLine("Date:\t\tHours:");
                    while (reader.Read()) {
                        Console.WriteLine(Convert.ToDateTime(reader["date"]).ToString("M/dd/yyyy")
                        + "\t" +
                        reader["hours"]);
                        total_hours += (double)reader["hours"];
                    }

                    Console.WriteLine($"Total hours:\t{total_hours}");
                }
            }

            sql = "select sum(hours) as total_hours from hours";
            using (SQLiteCommand command = new SQLiteCommand(sql, dbConnection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    reader.Read();
                    Console.WriteLine("Total hours:\t{0}", reader["total_hours"]);
                }
            }
            
            //reader.Close();

            dbConnection.Close();

            Console.WriteLine();
        }

        static void addhours() {
            Console.WriteLine();
            Log log = new Log();
            
            do {
                double hours = getPositiveDouble("Hours: ");
                DateTime input_date = getDateTime("Date (mm/dd): ");
                Day day = new Day(hours, input_date);
                log.Add(day);
                //Console.WriteLine($"hour is {day.Hour}  date is {day.Date}");
                //InsertRows(day.Hour, day.Date);
                Console.WriteLine();
                Console.Write("Add more hours? Hit Enter. Or type 'q' to quit.  ");
            } while (Console.ReadLine().ToLower() != "q");

            InsertRows(log);
        }

        static void InsertRows(Log log) {
            int rows = 0;
            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=myDatabase.sqlite;Version=3;");
            dbConnection.Open();

                //Console.WriteLine($"date {day.Date}  hour {day.Hour}");
            
                using (SQLiteTransaction tr = dbConnection.BeginTransaction()) {

                    using (SQLiteCommand command = dbConnection.CreateCommand()) {
                        command.Transaction = tr;
                        string sql = "insert into hours (hours, date) values (@hours, @date)";
                        command.CommandText = sql;
                        foreach (Day day in log.days) {
                        command.Parameters.Add(new SQLiteParameter("@hours", day.Hour));
                        command.Parameters.Add(new SQLiteParameter("@date", day.Date.ToString("yyyy-MM-dd")));
                        rows += command.ExecuteNonQuery();
                        }
                    }
                    
                tr.Commit();
                }            
            
            //SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            
            Console.WriteLine($"Inserted {rows} rows.");
            dbConnection.Close();
            
            Console.WriteLine();
        }
    }
}

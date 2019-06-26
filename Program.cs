using System;
using System.Data.SQLite;
using System.IO;

namespace hourlogger
{
    class Program
    {
        static private string DatabaseFileName = "myDatabase.sqlite";
        static private string year = "2019";

        static void Main(string[] args)
        {

            // Add a way to load logs, create new log file, and perhaps delete
            // SQLiteConnection.    CreateFile(DatabaseFileName);

            DirectoryInfo dir = new DirectoryInfo(@".\sql");
            if (!dir.Exists) {
                dir.Create();
                Console.WriteLine("Directory successfully created.");
            }

            FileInfo[] sqlitefiles = dir.GetFiles("*.sqlite");
            
            if (sqlitefiles.Length == 0) {
                Console.WriteLine("No database files found. Creating a new one.  Give it a name.");
                Console.WriteLine("Don't include the .sqlite extension.");
                Console.Write("Name:  ");
                string filename = Console.ReadLine();
                    SQLiteConnection.CreateFile(@".\sql\" + filename + ".sqlite");
                DatabaseFileName = filename + ".sqlite";
            }

            sqlitefiles = dir.GetFiles("*.sqlite");
            
            Console.WriteLine("Total number of sqlite files: {0}", sqlitefiles.Length);

            Console.WriteLine();

            for (int i = 0; i < sqlitefiles.Length; i++) {
                Console.WriteLine($"{i}. {sqlitefiles[i].Name}");
            }

            // select a file by i or create a new one

/* 
            Console.WriteLine("Hour Logger");
            Console.WriteLine();
            Console.Write("View log (l), add hours (h), or quit (q)  ");

            string choice = Console.ReadLine().ToLower();

            while (choice != "q") {
                switch (choice) {
                    case "l":
                        // view log without adding hours
                        Console.WriteLine();
                        viewlog();
                        Console.Write("View log (l), add hours (h), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;
                    
                    case "h":
                        // add hours (in a loop) then either:
                        //      save and view log
                        //      or quit without saving
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

*/
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

            using (SQLiteConnection dbConnection = new SQLiteConnection("Data Source=" + DatabaseFileName + ";Version=3;")) {
                
                dbConnection.Open();

                using (SQLiteCommand command = new SQLiteCommand("select date, hours from hours order by date asc", dbConnection)) {
                    
                    using (SQLiteDataReader reader = command.ExecuteReader()) {

                        double total_hours = 0;

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

                using (SQLiteCommand command = new SQLiteCommand("select sum(hours) as total_hours from hours", dbConnection)) {
                    
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        reader.Read();
                        Console.WriteLine("Total hours:\t{0}", reader["total_hours"]);
                    }
                }
            
            }

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
                Console.WriteLine();
                Console.Write("Add more hours? Hit Enter. Or type 'q' to quit.  ");
            } while (Console.ReadLine().ToLower() != "q");

            InsertRows(log);
        }

        static void InsertRows(Log log) {
            int rows = 0;
            using (SQLiteConnection dbConnection = new SQLiteConnection("Data Source=" + DatabaseFileName + ";Version=3;")) {
                
                dbConnection.Open();
                
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
                
            }
            Console.WriteLine($"Inserted {rows} rows.");
            Console.WriteLine();
        }
    }
}

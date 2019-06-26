using System;
using System.Data.SQLite;
using System.IO;

namespace hourlogger
{
    class Program
    {
        static private string DatabaseFileName;
        static private string year = "2019";

        static void Main(string[] args)
        {

            Console.WriteLine();
            Console.WriteLine("Hour Logger");
            Console.WriteLine();

            // Add a way to load logs, create new log file, and perhaps delete
            // SQLiteConnection.    CreateFile(DatabaseFileName);

            string dbFileName = getDbFileName();
            DatabaseFileName = @".\sql\" + dbFileName;

            // debug
            //Console.WriteLine(DatabaseFileName);

            Console.Write("View log (l), add hours (h), change database (c), or quit (q)  ");

            string choice = Console.ReadLine().ToLower();

            while (choice != "q") {
                switch (choice) {
                    case "l":
                        // view log without adding hours
                        Console.WriteLine();
                        viewlog();
                        Console.Write("View log (l), add hours (h), change database (c), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;
                    
                    case "h":
                        // add hours (in a loop) then either:
                        //      save and view log
                        //      or quit without saving
                        addhours();
                        Console.Write("View log (l), add hours (h), change database (c), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;

                    case "c":
                        dbFileName = getDbFileName();
                        DatabaseFileName = @".\sql\" + dbFileName;
                        Console.WriteLine($"Database has been changed to {DatabaseFileName}");
                        Console.WriteLine();
                        Console.Write("View log (l), add hours (h), change database (c), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;

                    default: 
                        Console.WriteLine("Could not understand your choice.");
                        Console.WriteLine();
                        Console.Write("View log (l), add hours (h), change database (c), or quit (q)  ");
                        choice = Console.ReadLine().ToLower();
                        break;
                }
            }

        }

        static string getDbFileName() {
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
                DatabaseFileName = @".\sql\" + filename + ".sqlite";
                createDbSchema(DatabaseFileName);
            }

            sqlitefiles = dir.GetFiles("*.sqlite");
            
            // debug
            //Console.WriteLine("Total number of sqlite files: {0}", sqlitefiles.Length);

            //Console.WriteLine();

            for (int i = 0; i < sqlitefiles.Length; i++) {
                Console.WriteLine($"{i+1}. {sqlitefiles[i].Name}");
            }

            Console.WriteLine();
            
            int fileNum = getFileNum("Number:  ", sqlitefiles.Length);
            
            Console.WriteLine();
            
            return sqlitefiles[fileNum-1].Name;

            // select a file by i or create a new one
        }

        static int getFileNum(string message, int length) {
            int fileNum = 0;
            Console.WriteLine("Choose a file by number."); 
            do {
                Console.Write(message);
                Int32.TryParse(Console.ReadLine(), out fileNum);
                // debug
                //Console.WriteLine("File Chosen is {0}", fileNum);
            } while (fileNum <= 0 || fileNum > length);

            return fileNum;
        }

        static DateTime getDateTime(string message) {
            DateTime input_date = default(DateTime);
            string line = "";
            do {
                Console.Write(message);
                line = Console.ReadLine();
                if (line == "q") {
                    Console.WriteLine();
                    return input_date;
                }
                DateTime.TryParse(line + "/" + year, out input_date);
            } while (input_date == default(DateTime));
            return input_date;
        }

        static double getPositiveDouble(string message) {
            double number = 0;
            string line = "";
            do {
                Console.Write(message);
                line = Console.ReadLine();
                if (line == "q") {
                    Console.WriteLine();
                    return number;
                }
                Double.TryParse(line, out number);

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
                if (hours == 0) {
                    break;
                }
                DateTime input_date = getDateTime("Date (mm/dd): ");
                if (input_date == default(DateTime)) {
                    break;
                }
                Day day = new Day(hours, input_date);
                log.Add(day);
                Console.WriteLine();
                Console.Write("Add more hours? Hit Enter. Or type 'q' to quit.  ");
            } while (Console.ReadLine().ToLower() != "q");

            if (log.days.Count > 0) {
                InsertRows(log);
            }
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
            Console.WriteLine();
            Console.WriteLine($"Inserted {rows} rows.");
            Console.WriteLine();
        }

        static void createDbSchema (string dbFileName) {
            using (SQLiteConnection dbConnection = new SQLiteConnection("Data Source =" + dbFileName + ";Version=3;")) {
                
                dbConnection.Open();

                using (SQLiteCommand command = dbConnection.CreateCommand()) {

                    string sql = "create table hours (hours real, date date)";
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

            }

            Console.WriteLine("Created table");

        }

    }
}

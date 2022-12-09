using BenchmarkDotNet.Running;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;

namespace MakeMeFaster
{

    /// <summary>
    /// Steps: 
    /// 
    /// 1. Create a database with name "MakeMeFaster"
    /// 2. Run application Debug/Release mode for the first time. PopulateDbData method will get the script and populate
    /// created db.
    /// 3. Comment or delete PopulateDbData() call from Main method. 
    /// 4. Go to BenchmarkService.cs class
    /// 5. Start coding within GetAuthors_Optimized method
    /// GOOD LUCK! :D 
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            //Debugging 
            //BenchmarkService benchmarkService = new BenchmarkService();
            //var p = benchmarkService.GetAuthors_Optimized_Struct();
            //var d = benchmarkService.GetAuthors_Optimized_Struct1();

            //Comment me after first execution, please.
            //PopulateDbData();

            BenchmarkRunner.Run<BenchmarkService>();
        }

        public static void PopulateDbData()
        {
            string sqlConnectionString = @"Server=VESA\SQLEXPRESS;Encrypt=False;Database=MakeMeFaster;Trusted_Connection=True;Integrated Security=true;MultipleActiveResultSets=true";

            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.FullName, @"script.sql");
            string script = File.ReadAllText(path);

            SqlConnection conn = new SqlConnection(sqlConnectionString);

            Server server = new Server(new ServerConnection(conn));

            server.ConnectionContext.ExecuteNonQuery(script);
        }
    }
}

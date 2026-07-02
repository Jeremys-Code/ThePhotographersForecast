using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePhotographersForecast.Database
{
    public static class Constants
    {
        public const string DatabaseFilename = "dat.db";

        public const SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLiteOpenFlags.PrivateCache |
            // file is encrypted until opened
            SQLiteOpenFlags.ProtectionComplete;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        public static Guid DeviceId { get; set; }
        public static int SessionId { get; set; }
    }
}

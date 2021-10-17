using System;
using System.IO;
using App1;
using SQLite;
using Xamarin.Forms;
[assembly: Dependency(typeof(DB))]
namespace App1
{
    public class DB : ISQliteInterface
    {
        public SQLiteConnection GetConnection()
        {
            var sqliteFilename = "app1.db3";
            var libraryPath = "";
            var documentsPath = "";

            if (Device.RuntimePlatform == Device.iOS)
            {
                documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
                libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder instead
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }

            var path = Path.Combine(libraryPath, sqliteFilename);
            var db = new SQLiteConnection(path);

            return db;
        }

       
    }
}

using SQLite;

namespace App1
{
    public interface ISQliteInterface
    {

        SQLiteConnection GetConnection();
    }
}
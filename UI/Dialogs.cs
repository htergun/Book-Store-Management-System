namespace BookStoreManagement.UI;

// Short helpers for message boxes.
public static class Dialogs
{
    private const string Caption = "BookStoreManagement";

    public static void Info(string message)
    {
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void Error(string message)
    {
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static bool Confirm(string message)
    {
        return MessageBox.Show(message, Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    public static void DatabaseError(Exception ex)
    {
        Error("Database error:\n\n" + ex.Message +
              "\n\nCheck the connection string in Data/Database.cs and make sure Database/BookStoreDB.sql was executed.");
    }
}

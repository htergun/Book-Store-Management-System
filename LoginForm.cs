using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement;

// Holds the logged-in user's info (all forms read it from here)
public static class UserSession
{
    public static int UserId { get; set; }
    public static string Username { get; set; } = "";
    public static string Role { get; set; } = "";
    public static bool IsAdmin => Role == "Admin";
}

public class LoginForm : Form
{
    private const int MaxFailedAttempts = 3;

    private readonly TextBox txtUsername = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnLogin = new();
    private readonly Label lblMessage = new();

    public LoginForm()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        Text = "BookStoreManagement - Login";
        ClientSize = new Size(360, 280);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblTitle = new Label
        {
            Text = "Bookstore Management",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(40, 20)
        };
        var lblUser = new Label { Text = "Username", AutoSize = true, Location = new Point(40, 70) };
        var lblPass = new Label { Text = "Password", AutoSize = true, Location = new Point(40, 130) };

        txtUsername.Location = new Point(40, 92);
        txtUsername.Width = 280;

        txtPassword.Location = new Point(40, 152);
        txtPassword.Width = 280;
        txtPassword.UseSystemPasswordChar = true;   // hides the password with dots

        btnLogin.Text = "Log In";
        btnLogin.Location = new Point(40, 195);
        btnLogin.Size = new Size(280, 36);
        btnLogin.Click += (s, e) => TryLogin();

        lblMessage.AutoSize = true;
        lblMessage.ForeColor = Color.Firebrick;
        lblMessage.Location = new Point(40, 242);

        AcceptButton = btnLogin;   // pressing Enter also logs in

        Controls.AddRange(new Control[] { lblTitle, lblUser, txtUsername, lblPass, txtPassword, btnLogin, lblMessage });
    }

    private void TryLogin()
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (username.Length == 0 || password.Length == 0)
        {
            ShowError("Username and password cannot be empty.");
            return;
        }

        try
        {
            using var connection = DbHelper.GetConnection();
            connection.Open();

            bool found = false, isLocked = false;
            int userId = 0, failedAttempts = 0;
            string salt = "", role = "";
            byte[] storedHash = Array.Empty<byte>();

            // PARAMETERIZED QUERY: user input is never concatenated into the SQL text,
            // so SQL injection (e.g. ' OR 1=1 --) does not work.
            const string selectSql =
                "SELECT user_id, salt, password_hash, role, failed_attempts, is_locked " +
                "FROM Users WHERE username = @username";

            using (var cmd = new SqlCommand(selectSql, connection))
            {
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = username;
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    found = true;
                    userId = reader.GetInt32(0);
                    salt = reader.GetString(1);
                    storedHash = (byte[])reader[2];
                    role = reader.GetString(3);
                    failedAttempts = reader.GetInt32(4);
                    isLocked = reader.GetBoolean(5);
                }
            }

            if (!found)
            {
                ShowError("Invalid username or password.");
                return;
            }

            if (isLocked)
            {
                ShowError("Account is locked. Contact an administrator.");
                return;
            }

            // Hash the entered password the same way and compare it with the stored hash
            byte[] enteredHash = SHA256.HashData(Encoding.UTF8.GetBytes(salt + password));

            if (CryptographicOperations.FixedTimeEquals(enteredHash, storedHash))
            {
                // Successful login: reset the failed attempts counter
                using var reset = new SqlCommand(
                    "UPDATE Users SET failed_attempts = 0 WHERE user_id = @id", connection);
                reset.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                reset.ExecuteNonQuery();

                UserSession.UserId = userId;
                UserSession.Username = username;
                UserSession.Role = role;

                OpenMainForm();
            }
            else
            {
                // Wrong password: increase the counter and lock the account after 3 failures
                failedAttempts++;
                bool lockNow = failedAttempts >= MaxFailedAttempts;

                using var fail = new SqlCommand(
                    "UPDATE Users SET failed_attempts = @f, is_locked = @l WHERE user_id = @id", connection);
                fail.Parameters.Add("@f", SqlDbType.Int).Value = failedAttempts;
                fail.Parameters.Add("@l", SqlDbType.Bit).Value = lockNow;
                fail.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                fail.ExecuteNonQuery();

                ShowError(lockNow
                    ? "3 failed attempts! Account locked."
                    : $"Invalid username or password. Attempts left: {MaxFailedAttempts - failedAttempts}");
            }
        }
        catch (Exception ex)
        {
            ShowError("Database error: " + ex.Message);
        }
    }

    private void OpenMainForm()
    {
        Hide();
        var main = new MainForm();
        main.FormClosed += (s, e) => Close();   // closing the main menu ends the program
        main.Show();
    }

    private void ShowError(string message)
    {
        lblMessage.Text = message;
        txtPassword.Clear();
        txtPassword.Focus();
    }
}
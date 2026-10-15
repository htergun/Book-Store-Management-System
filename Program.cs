using BookStoreManagement;

ApplicationConfiguration.Initialize();

if (!DbHelper.TestConnection(out string error))
{
    MessageBox.Show(error, "Connection error");
    return;
}
MessageBox.Show("Connected to BookStoreDB!");

ApplicationConfiguration.Initialize();
Application.Run(new LoginForm());
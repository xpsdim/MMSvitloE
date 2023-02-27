using MySql.Data.MySqlClient;

namespace Data
{
	public class DBConnection
	{
		private DBConnection()
		{
		}

		public string ConnectionString { get; set; }

		public MySqlConnection Connection { get; set; }

		private static DBConnection _instance = null;
		public static DBConnection Instance()
		{
			if (_instance == null)
				_instance = new DBConnection();
			return _instance;
		}

		public bool IsConnect()
		{
			if (Connection == null)
			{
				if (String.IsNullOrEmpty(ConnectionString))
					return false;
				Connection = new MySqlConnection(ConnectionString);
				Connection.Open();
			}

			return true;
		}

		public void Close()
		{
			Connection.Close();
		}
	}
}
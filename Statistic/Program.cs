// See https://aka.ms/new-console-template for more information
using MySql.Data.MySqlClient;
using Statistic;

Console.WriteLine("statistic calculation running...");

//to keep connection string in safe just set it in "Environment Variables" of your windows computer
var connectionString = System.Environment.GetEnvironmentVariable("TBotConnStr", EnvironmentVariableTarget.Machine);
Console.WriteLine($"str: {connectionString}");

var dbCon = Data.DBConnection.Instance();
dbCon.ConnectionString = connectionString;
var startDateUtc = new DateTime(2022, 12, 1).ToUniversalTime();
var endDateUtc = new DateTime(2023, 3, 3).ToUniversalTime();

if (dbCon.IsConnect())
{
	//1. Total followers
	string query = "select count(*) from Followers";
	var cmd = new MySqlCommand(query, dbCon.Connection);
	var reader = cmd.ExecuteReader();
	while (reader.Read())
	{
		string followersCnt = reader.GetString(0);
		Console.WriteLine($"Folowers total: {followersCnt}");
	}

	//2. Active followers
	reader.Close();
	query = "select count(*) from Followers WHERE FollowingSvitloBot = 1;";
	cmd = new MySqlCommand(query, dbCon.Connection);
	reader = cmd.ExecuteReader();
	while (reader.Read())
	{
		string followersCnt = reader.GetString(0);
		Console.WriteLine($"Active folowers: {followersCnt}");
	}

	//3. Status Messages sent total
	reader.Close();
	query = "select count(*) from MessageLog where DateUtc >= @StartDate and DateUtc < @EndDate";
	cmd = new MySqlCommand(query, dbCon.Connection);
	cmd.Parameters.AddWithValue("@StartDate", startDateUtc);
	cmd.Parameters.AddWithValue("@EndDate", endDateUtc);
	reader = cmd.ExecuteReader();
	while (reader.Read())
	{
		string msgCnt = reader.GetString(0);
		Console.WriteLine($"status messages sent total: {msgCnt}");
	}

	//4. Status Messages sent December
	reader.Close();
	query = "select count(*) from MessageLog where DateUtc >= @StartDate and DateUtc < @EndDate";
	cmd = new MySqlCommand(query, dbCon.Connection);
	cmd.Parameters.AddWithValue("@StartDate", startDateUtc);
	cmd.Parameters.AddWithValue("@EndDate", new DateTime(2023, 1, 1).ToUniversalTime());
	reader = cmd.ExecuteReader();
	while (reader.Read())
	{
		string msgCnt = reader.GetString(0);
		Console.WriteLine($"	December: {msgCnt}");
	}

	//5. Status Messages sent January
	reader.Close();
	query = "select count(*) from MessageLog where DateUtc >= @StartDate and DateUtc < @EndDate";
	cmd = new MySqlCommand(query, dbCon.Connection);
	cmd.Parameters.AddWithValue("@StartDate", new DateTime(2023, 1, 1).ToUniversalTime());
	cmd.Parameters.AddWithValue("@EndDate", new DateTime(2023, 2, 1).ToUniversalTime());
	reader = cmd.ExecuteReader();
	while (reader.Read())
	{
		string msgCnt = reader.GetString(0);
		Console.WriteLine($"	January: {msgCnt}");
	}

	//6. Status Messages sent February
	reader.Close();
	query = "select count(*) from MessageLog where DateUtc >= @StartDate and DateUtc <= @EndDate";
	cmd = new MySqlCommand(query, dbCon.Connection);
	cmd.Parameters.AddWithValue("@StartDate", new DateTime(2023, 2, 1).ToUniversalTime());
	cmd.Parameters.AddWithValue("@EndDate", new DateTime(2023, 3, 3).ToUniversalTime());
	reader = cmd.ExecuteReader();
	while (reader.Read())
	{
		string msgCnt = reader.GetString(0);
		Console.WriteLine($"	Febryary: {msgCnt}");
	}

	//7. Power Outages number
	reader.Close();
	var outages = new List<OutageEvent>();
	query = "select * from Events order by DateUtc;";
	cmd = new MySqlCommand(query, dbCon.Connection);
	reader = cmd.ExecuteReader();
	while (reader.Read())
	{
		outages.Add(new OutageEvent()
		{
			EventType = reader.GetInt32(1),
			Date = reader.GetDateTime(2)
		});
	}
	Console.WriteLine($"Total outages: {outages.Where(o => o.EventType == 2 && o.KyivDate >= new DateTime(2022, 12, 1) && o.KyivDate < new DateTime(2023, 3, 1)).Count()}");
	Console.WriteLine($"	December: {outages.Where(o => o.EventType == 2 && o.KyivDate >= new DateTime(2022,12,1) && o.KyivDate < new DateTime(2023,1,1)).Count()}");
	Console.WriteLine($"	January: {outages.Where(o => o.EventType == 2 && o.KyivDate >= new DateTime(2023, 1, 1) && o.KyivDate < new DateTime(2023, 2, 1)).Count()}");
	Console.WriteLine($"	February: {outages.Where(o => o.EventType == 2 && o.KyivDate >= new DateTime(2023, 2, 1) && o.KyivDate < new DateTime(2023, 3, 1)).Count()}");

	//8. Powe outages time
	var startDate = new DateTime(2022, 12, 1);
	var endDate = new DateTime(2023, 3, 1);
	var decOutageTime = new TimeSpan();
	var janOutageTime = new TimeSpan();
	var febOutageTime = new TimeSpan();
	for(var i = 0; i < outages.Count; i++)
	{
		var o = outages[i];
		if (o.EventType != 2 || o.KyivDate < startDate || o.KyivDate >= endDate)
		{
			continue;
		}

		if (outages[i - 1].EventType != 1)
		{
			throw new Exception("The sequence of events is broken!");
		}

		if (o.KyivDate < new DateTime(2023, 1, 1))
		{
			decOutageTime = decOutageTime + (o.KyivDate - outages[i - 1].KyivDate);
		}
		else if (o.KyivDate < new DateTime(2023, 2, 1))
		{
			janOutageTime = decOutageTime + (o.KyivDate - outages[i - 1].KyivDate);
		}
		else if (o.KyivDate < new DateTime(2023, 3, 1))
		{
			febOutageTime = decOutageTime + (o.KyivDate - outages[i - 1].KyivDate);
		}
	}
	var totaloutage = decOutageTime + janOutageTime + febOutageTime;
	Console.WriteLine($"Total outage time: {totaloutage.Days} дн. {totaloutage.Hours} год. {totaloutage.Minutes} хв. ({totaloutage / (new DateTime(2023,3,1) - new DateTime(2022,12,1)) })");
	Console.WriteLine($"	December: {decOutageTime.Days} дн. {decOutageTime.Hours} год. {decOutageTime.Minutes} хв. ({decOutageTime / (new DateTime(2023, 1, 1) - new DateTime(2022, 12, 1))})");
	Console.WriteLine($"	January: {janOutageTime.Days} дн. {janOutageTime.Hours} год. {janOutageTime.Minutes} хв. ({janOutageTime / (new DateTime(2023, 2, 1) - new DateTime(2023, 1, 1))})");
	Console.WriteLine($"	February: {febOutageTime.Days} дн. {febOutageTime.Hours} год. {febOutageTime.Minutes} хв. ({febOutageTime / (new DateTime(2023, 3, 1) - new DateTime(2023, 2, 1))})");

	dbCon.Close();
}
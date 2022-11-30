using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using MMSvitloE.Db;
using MMSvitloE.ConfigurationService;

namespace MMSvitloE
{
	class Program
	{
		public static ITelegramBotClient bot = null;
		public static IConfigurationRoot configuration;
		public static DateTime? StatusChangedAtUtc = null;
		public static bool Status = true;
		public static TimeZoneInfo KyivTimezone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");

		public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
			{
				var message = update.Message;
				var messageTxt = message.Text?.ToLower();
				switch (messageTxt)
				{
					case "/start":
						await botClient.SendTextMessageAsync(message.Chat, $"Вітаю, {message.From.FirstName} {message.From.LastName}!{Environment.NewLine}Цей бот підтримує такі команди:{Environment.NewLine}/start - показує стартову сторінку з переліком команд{Environment.NewLine}/status - відображає поточний стан наявності світла в нашому ЖК{Environment.NewLine}/follow - підписатись на оновлення по світлу{Environment.NewLine}/unfollow - відписатись та припинити отримувати оновлення по світлу");
						return;
					case "/status":
						var timeMsgPart = string.Empty;
						if (StatusChangedAtUtc.HasValue)
						{
							var period = DateTime.UtcNow - StatusChangedAtUtc.Value;
							var periodStr = period.TimespanToReadableStr();
							timeMsgPart = $"з {TimeZoneInfo.ConvertTimeFromUtc(StatusChangedAtUtc.Value, KyivTimezone):HH:mm dd.MM.yyyy}{periodStr}";
						}
						var msg = $"Нема :( {timeMsgPart}";
						if (Status)
						{
							msg = $"Є! {timeMsgPart}";
						}

						Console.WriteLine($"{message.From.FirstName} {message.From.LastName} - {msg}");
						await botClient.SendTextMessageAsync(message.Chat, msg);
						await new Utils().SaveMessageToDBAsync(message, msg);
						return;
					case "/follow":
						await new Utils().UpdateFollower(message.From, follow: true);
						await botClient.SendTextMessageAsync(message.Chat, $"Вітаю, {message.From.FirstName} {message.From.LastName}! Ви тепер підписані на повідомлення щодо включення/відключення світла у нашому ЖК.");
						return;
					case "/unfollow":
						await new Utils().UpdateFollower(message.From, follow: false);
						await botClient.SendTextMessageAsync(message.Chat, "Вас відписано!");
						return;
					default:
						await botClient.SendTextMessageAsync(message.Chat, $"Команда {messageTxt} не підтримується ботом.");
						return;
				}
			}
		}

		public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			// Just write error to console
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
		}

		public static void InitConfiguration()
		{
			configuration = new ConfigurationServiceFactory().CreateInstance();
			bot = new TelegramBotClient(configuration["botToken"]);
			BotContextFactory.ConnectionString = configuration["connectionStrings"];
		}

		public static async Task<bool> ReadStatusAsync()
		{
			var utils = new Utils();

			//try to restore last status from DB
			var lastEvent = utils.ReadLastEvent();
			if (lastEvent != null)
			{
				Status = lastEvent.EventType == EventTypesEnum.PingSuccess;
				StatusChangedAtUtc = lastEvent.DateUtc;
			}

			var newStatus = new Utils().PingHost(configuration["ipToPing"]);
			var now = DateTime.UtcNow;
			if (newStatus != Status)
			{
				Status = newStatus;
				StatusChangedAtUtc = DateTime.UtcNow;
				await utils.SaveEvent(newStatus ? EventTypesEnum.PingSuccess : EventTypesEnum.PingTimeout);
				return true;
			}
			//TODO comment it after testing
			//Console.WriteLine($"new status: {newStatus}: {TimeZoneInfo.ConvertTimeFromUtc(now, KyivTimezone):HH:mm dd.MM.yyyy}");
			return false;
		}

		public static bool ReadStatus()
		{
			return ReadStatusAsync().Result;
		}

		static async Task Main(string[] args)
		{
			InitConfiguration();

			var timer = new Timer(
				e => ReadStatus(),
				null,
				TimeSpan.Zero,
				TimeSpan.FromMinutes(1));

			var cts = new CancellationTokenSource();
			var cancellationToken = cts.Token;
			var receiverOptions = new ReceiverOptions
			{
				AllowedUpdates = { },
			};
			bot.StartReceiving(
				HandleUpdateAsync,
				HandleErrorAsync,
				receiverOptions,
				cancellationToken
			);
			Console.WriteLine("Bot Started " + bot.GetMeAsync().Result.FirstName);
			Thread.Sleep(Timeout.Infinite);
		}
	}
}

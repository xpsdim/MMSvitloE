using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using MMSvitloE.Db;
using MMSvitloE.ConfigurationService;
using Telegram.Bot.Exceptions;

namespace MMSvitloE
{
	class Program
	{
		public static ITelegramBotClient bot = null;
		public static IConfigurationRoot configuration;
		public static DateTime? StatusChangedAtUtc = null;
		public static bool Status = true;
		public static TimeZoneInfo KyivTimezone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
		public static Utils utils;

		public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
			{
				var message = update.Message;
				var messageTxt = message.Text?.ToLower();
				switch (messageTxt)
				{
					case "/start":
						try
						{
							await botClient.SendTextMessageAsync(message.Chat, $"Вітаю, {message.From.FirstName} {message.From.LastName}!{Environment.NewLine}Цей бот підтримує такі команди:{Environment.NewLine}/start - показує стартову сторінку з переліком команд{Environment.NewLine}/status - відображає поточний стан наявності світла в нашому ЖК{Environment.NewLine}/follow - підписатись на оновлення по світлу{Environment.NewLine}/unfollow - відписатись та припинити отримувати оновлення по світлу");
						}
						catch (ApiRequestException ex)
						{
							Console.WriteLine(ex.Message);
							await utils.SaveMessageToDBAsync(message, $"{messageTxt}: {ex.Message}");
						}
						return;
					case "/status":
						var timeMsgPart = string.Empty;
						if (StatusChangedAtUtc.HasValue)
						{
							var period = DateTime.UtcNow - StatusChangedAtUtc.Value;
							var timeSpanStr = period.TimespanToReadableStr();
							var periodStr = string.Empty;
							if (!string.IsNullOrEmpty(timeSpanStr))
							{
								periodStr = $"{Environment.NewLine}вже {timeSpanStr}";
							}
							timeMsgPart = $"з {TimeZoneInfo.ConvertTimeFromUtc(StatusChangedAtUtc.Value, KyivTimezone):HH:mm dd.MM.yyyy}{periodStr}";
						}
						var msg = $"Нема 😕 {timeMsgPart}";
						if (Status)
						{
							msg = $"Є! 😀 {timeMsgPart}";
						}

						try
						{
							Console.WriteLine($"{message.From.FirstName} {message.From.LastName} - {msg}");
							await botClient.SendTextMessageAsync(message.Chat, msg);
							await utils.SaveMessageToDBAsync(message, msg);
						}
						catch (ApiRequestException ex)
						{
							Console.WriteLine(ex.Message);
							await utils.SaveMessageToDBAsync(message, $"{messageTxt}: {ex.Message}");
						}
						return;
					case "/follow":
						try
						{
							await utils.UpdateFollower(message.From, follow: true);
							await botClient.SendTextMessageAsync(message.Chat, $"Вітаю, {message.From.FirstName} {message.From.LastName}! Ви тепер підписані на повідомлення щодо включення/відключення світла у нашому ЖК.");
						}
						catch (ApiRequestException ex)
						{
							Console.WriteLine(ex.Message);
							await utils.SaveMessageToDBAsync(message, $"{messageTxt}: {ex.Message}");
						}
						return;
					case "/unfollow":
						try
						{
							await botClient.SendTextMessageAsync(message.Chat, "Вас відписано!");
							await utils.UpdateFollower(message.From, follow: false);
						}
						catch (ApiRequestException ex)
						{
							Console.WriteLine(ex.Message);
							await utils.SaveMessageToDBAsync(message, $"{messageTxt}: {ex.Message}");
						}
						return;
					default:
						try
						{
							await botClient.SendTextMessageAsync(message.Chat, $"Команда {messageTxt} не підтримується ботом.");
						}
						catch (ApiRequestException ex)
						{
							Console.WriteLine(ex.Message);
							await utils.SaveMessageToDBAsync(message, $"{messageTxt}: {ex.Message}");
						}
						return;
				}
			}
		}

		public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			// Just write error to console
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
		}

		public static async Task InitConfigurationAsync()
		{
			configuration = new ConfigurationServiceFactory().CreateInstance();
			bot = new TelegramBotClient(configuration["botToken"]);
			BotContextFactory.ConnectionString = configuration["connectionStrings"];
			utils = new Utils();
		}

		public static async Task<bool> ReadStatusAsync()
		{
			//try to restore last status from DB
			var lastEvent = utils.ReadLastEvent();
			if (lastEvent != null)
			{
				Status = lastEvent.EventType == EventTypesEnum.PingSuccess;
				StatusChangedAtUtc = lastEvent.DateUtc;
			}

			//try to ping IP first if it's configured
			var newStatus = true;
			var ip = configuration["ipToPing"];
			if (!string.IsNullOrEmpty(ip))
			{
				newStatus = utils.PingHost(ip);
			}

			//then try to get site if it configured and ping by IP was success or skipped
			var site = configuration["webSiteToGet"];
			if (newStatus && !string.IsNullOrEmpty(site))
			{
				newStatus = await utils.CheckWebSiteAsync(configuration["webSiteToGet"]);
			}

			//set new status if it changed and send notifications to followers
			if (newStatus != Status)
			{
				Status = newStatus;
				var prevStatusStartedAt = StatusChangedAtUtc;
				StatusChangedAtUtc = DateTime.UtcNow;
				await utils.SaveEvent(newStatus ? EventTypesEnum.PingSuccess : EventTypesEnum.PingTimeout);
				await utils.InformFollowersAboutStatusChangingAsync(bot, newStatus, (prevStatusStartedAt == null ? null : StatusChangedAtUtc - prevStatusStartedAt.Value));
				return true;
			}
			return false;
		}

		public static bool ReadStatus()
		{
			return ReadStatusAsync().Result;
		}

		static async Task Main(string[] args)
		{
			await InitConfigurationAsync();

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

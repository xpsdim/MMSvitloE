using Microsoft.Extensions.DependencyInjection;
using MMSvitloE.Db;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace MMSvitloE
{
	public class Utils
	{
		static IHttpClientFactory httpClientFactory;

		static Utils()
		{
			var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
			httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
		}

		public bool PingHost(string ip)
		{
			//set the ping options, TTL 128
			PingOptions pingOptions = new PingOptions(128, true);

			//create a new ping instance
			Ping ping = new Ping();

			//32 byte buffer (create empty)
			byte[] buffer = new byte[32];

			var successCnt = 0;

			//here we will ping the host 4 times (standard)
			for (int i = 0; i < 4; i++)
			{
				PingReply pingReply = ping.Send(ip, 1000, buffer, pingOptions);

				//make sure we dont have a null reply
				if (!(pingReply == null))
				{
					switch (pingReply.Status)
					{
						case IPStatus.Success:
							successCnt++;
							break;
						default:
							break;
					}
				}
			}

			return successCnt > 0;
		}

		public  async Task<bool> CheckWebSiteAsync(string siteUrl)
		{
			var client = httpClientFactory.CreateClient();
			try
			{
				var result = await client.GetAsync(siteUrl);
				return result.IsSuccessStatusCode;
			}
			catch
			{
				return false;
			}
		}

		public async Task SaveEvent(EventTypesEnum eventType)
		{
			var context = new BotContextFactory().CreateDbContext(null);
			var newEvent = new Event()
			{
				EventType = eventType,
				DateUtc = System.DateTime.UtcNow
			};

			context.Events.Add(newEvent);
			await context.SaveChangesAsync();
		}

		public Event ReadLastEvent()
		{
			var context = new BotContextFactory().CreateDbContext(null);
			return context.Events
				.OrderByDescending(p => p.DateUtc)
				.FirstOrDefault();
		}

		public async Task SaveMessageToDBAsync(Message msg, string reply)
		{
			await UpdateFollower(msg.From);
			var context = new BotContextFactory().CreateDbContext(null);
			var logMsg = new MessageLogItem();
			logMsg.FollowerId = msg.From.Id;
			logMsg.DateUtc = System.DateTime.UtcNow;
			logMsg.MessageSent = reply;
			context.MessageLog.Add(logMsg);
			await context.SaveChangesAsync();
		}

		public async Task UpdateFollower(User user, bool? follow = null)
		{
			if (user == null)
			{
				return;
			}
			var context = new BotContextFactory().CreateDbContext(null);
			var follower = context.Followers.FirstOrDefault(u => u.Id == user.Id);
			if (follower == null)
			{
				follower = new Follower();
				follower.Id = user.Id;
				context.Followers.Add(follower);
			}

			follower.Username = user.Username;
			follower.FirstName = user.FirstName;
			follower.LastName = user.LastName;
			follower.LanguageCode = user.LanguageCode;

			follower.IsBot = user.IsBot;
			follower.CanJoinGroups = user.CanJoinGroups;
			follower.CanReadAllGroupMessages = user.CanReadAllGroupMessages;
			follower.SupportsInlineQueries = user.SupportsInlineQueries;

			if (follow != null)
			{
				if (follow.Value)
				{
					follower.StartedUnfollowUtc = null;
					follower.StartedFollowUtc = System.DateTime.UtcNow;
					follower.FollowingSvitloBot = true;
				}
				else
				{
					follower.StartedUnfollowUtc = System.DateTime.UtcNow;
					follower.StartedFollowUtc = null;
					follower.FollowingSvitloBot = false;
				}
			}

			await context.SaveChangesAsync();
		}

		public async Task InformFollowersAboutStatusChangingAsync(ITelegramBotClient bot, bool newStstus)
		{
			var mesage = newStstus
				? "Щойно з'явилось світло!"
				: "Пропало світло :(";

			var context = new BotContextFactory().CreateDbContext(null);
			foreach(var follower in context.Followers.Where(f => f.FollowingSvitloBot))
			{
				try
				{
					await bot.SendTextMessageAsync(follower.Id, mesage);
				}
				catch(ApiRequestException ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
	}
}

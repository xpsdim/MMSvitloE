using System.Net.NetworkInformation;

namespace MMSvitloE
{
	public class Utils
	{
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
	}
}

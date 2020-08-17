using Discord;
using Discord_Custom_Status.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord_Custom_Status.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public string UpdateStatus(Microsoft.AspNetCore.Http.IFormCollection collection)
        {
            if (_cache.Get("DiscordActivityVersion") == null)
            {
                _cache.Set("DiscordActivityVersion", 0);
            }
            else
            {
                _cache.Set("DiscordActivityVersion", (int)_cache.Get("DiscordActivityVersion") + 1);
            }
            Thread.Sleep(100);
            Task.Run(() => UpdateActivity((int)_cache.Get("DiscordActivityVersion"), collection));

            return "Request sent!";
        }

        private void UpdateActivity(int VersionNumber, Microsoft.AspNetCore.Http.IFormCollection collection)
        {
            //var clientID = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
            string clientID = collection["ClientID"].ToString();
            if (clientID == "")
            {
                clientID = "418559331265675294";
                clientID = "636181032353136640";
            }
            var discord = new Discord.Discord(long.Parse(clientID), (ulong)Discord.CreateFlags.Default);
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                State = collection["State"],
                Details = collection["Details"],
                Assets =
                {
                LargeImage =collection["LargeImage"],
                LargeText = collection["LargeText"],
                SmallImage=collection["SmallImage"],
                SmallText = collection["SmallText"],
                },

                
                Instance = true,
            };
            if (collection["timeStart"] != "")
            {
                activity.Timestamps.Start = long.Parse(collection["timeStart"]);
            }
            if (collection["timeEnd"] != "")
            {
                activity.Timestamps.End = long.Parse(collection["timeEnd"]);
            }
          ;
            if (collection["EnableParty"].ToString() == "on")
            {
                activity.Party.Id = Guid.NewGuid().ToString();
                activity.Party.Size = new PartySize
                {
                    CurrentSize = collection["CurrentSize"] == "" ? 1 : int.Parse(collection["CurrentSize"]),
                    MaxSize = collection["MaxSize"] == "" ? 1 : int.Parse(collection["MaxSize"])
                };
                activity.Secrets = new Discord.ActivitySecrets
                {
                    Join = Guid.NewGuid().ToString(),
                    Match = Guid.NewGuid().ToString(),
                    Spectate = Guid.NewGuid().ToString(),
                };
            }
            Discord.Result result1 = new Discord.Result();
            activityManager.UpdateActivity(activity, result =>
            {
                Console.WriteLine("Update Activity {0}", result);
                result1 = result;
                // Send an invite to another user for this activity.
                // Receiver should see an invite in their DM.
                // Use a relationship user's ID for this.
                // activityManager
                //   .SendInvite(
                //       364843917537050624,
                //       Discord.ActivityActionType.Join,
                //       "",
                //       inviteResult =>
                //       {
                //           Console.WriteLine("Invite {0}", inviteResult);
                //       }
                //   );
            });
            try
            {
                while (VersionNumber == (int)_cache.Get("DiscordActivityVersion"))
                {
                    discord.RunCallbacks();
                    Thread.Sleep(1000 / 10);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                discord.Dispose();
                _cache.Set("DisposeVersionCode", VersionNumber);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
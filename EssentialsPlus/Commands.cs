using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EssentialsPlus.Db;
using EssentialsPlus.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Localization;
using StringExtensions = EssentialsPlus.Extensions.StringExtensions;

namespace EssentialsPlus
{
	public static class Commands
    {
        private static void FindHelp(TSPlayer plr)
        {
            plr.SendErrorMessage("Invalid syntax! Proper syntax: {0}find <switch> <name...> [page]",
                TShock.Config.Settings.CommandSpecifier);
            plr.SendSuccessMessage("Valid {0}find switches:", TShock.Config.Settings.CommandSpecifier);

            if (plr.RealPlayer)
            {
                plr.SendInfoMessage
                (
                    "[c/00FF00:-command], [c/00FF00:-c],  [c/00FF00:c]:  Finds a command.\n" +
                    "[c/00FF00:-item],       [c/00FF00:-i],   [c/00FF00:i]:   Finds an item.\n" +
                    "[c/00FF00:-npc],       [c/00FF00:-n],   [c/00FF00:n]:  Finds an NPC.\n" +
                    "[c/00FF00:-tile],        [c/00FF00:-t],   [c/00FF00:t]:   Finds a tile.\n" +
                    "[c/00FF00:-wall],       [c/00FF00:-w],  [c/00FF00:w]:  Finds a wall.\n" +
                    "[c/00FF00:-prefix]     [c/00FF00:-p],   [c/00FF00:p]:  Finds a prefix.\n" +
                    "[c/00FF00:-paint],      [c/00FF00:-pa], [c/00FF00:pa]: Finds a paint.\n" +
                    "[c/00FF00:-buff],      [c/00FF00:-b],   [c/00FF00:b]:  Finds a buff."
                );
            }
            else
            {
                plr.SendInfoMessage
                (
                    "-command, -c,  c:  Finds a command.\n" +
                    "-item,    -i,  i:  Finds an item.\n" +
                    "-npc,     -n,  n:  Finds an NPC.\n" +
                    "-tile,    -t,  t:  Finds a tile.\n" +
                    "-wall,    -w,  w:  Finds a wall.\n" +
                    "-prefix,  -p,  p:  Finds a prefix.\n" +
                    "-paint,   -pa, pa: Finds a paint.\n" +
                    "-buff,    -b,  b:  Finds a buff."
                );
            }
        }

        private static List<string> AllPaintsList = new List<string> { "Red", "Orange", "Yellow", "Lime", "Green", "Teal", "Cyan", "Sky Blue", "Blue", "Purple", "Violet", "Pink", "Deep Red", "Deep Orange", "Deep Yellow", "Deep Lime", "Deep Green", "Deep Teal", "Deep Cyan", "Deep Sky Blue", "Deep Blue", "Deep Purple", "Deep Violet", "Deep Pink", "Black", "White", "Gray", "Brown", "Shadow", "Negative" };

        public static async void Find(CommandArgs e)
        {
            if (e.Parameters.Count < 2)
            {
                FindHelp(e.Player);
                return;
            }
            
            string Search = ((int.TryParse(e.Parameters.Last(), out int Page))
                                ? string.Join(" ", e.Parameters.Skip(1).Take(e.Parameters.Count - 2))
                                : string.Join(" ", e.Parameters.Skip(1)));
            if (Page < 1) { Page = 1; }

            switch (e.Parameters[0].ToLower())
            {
                #region Command

                case "c":
                case "-c":
                case "-command":
                    {
                        var commands = new List<string>();
                        await Task.Run(() =>
                        {
                            foreach (Command command in TShockAPI.Commands.ChatCommands
                                .FindAll(c => c.Names.Any(s => s.ContainsInsensitive(Search))))
                            {
                                commands.Add(string.Format
                                (
                                    "{0} (Permission: {1})",
                                    command.Name,
                                    command.Permissions.FirstOrDefault())
                                );
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, commands,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found Commands ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No commands were found."
                            });
                        return;
                    }

                #endregion

                #region Item

                case "i":
                case "-i":
                case "-item":
                    {
                        var items = new List<string>();

                        await Task.Run(() =>
                        {
                            for (int i = -48; i < 0; i++)
                            {
                                var item = new Item();
                                item.netDefaults(i);
                                if (item.HoverName.ContainsInsensitive(Search))
                                {
                                    items.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        item.HoverName, i
                                    ));
                                }
                            }
                            for (int i = 0; i < ItemID.Count; i++)
                            {
                                if (Lang.GetItemNameValue(i).ContainsInsensitive(Search))
                                {
                                    items.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        Lang.GetItemNameValue(i), i
                                    ));
                                }
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, items,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found Items ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No items were found."
                            });
                        return;
                    }

                #endregion

                #region NPC

                case "n":
                case "-n":
                case "-npc":
                    {
                        var npcs = new List<string>();

                        await Task.Run(() =>
                        {
                            for (int i = -65; i < 0; i++)
                            {
                                var npc = new NPC();
                                npc.SetDefaults(i);
                                if (npc.FullName.ContainsInsensitive(Search))
                                {
                                    npcs.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        npc.FullName, i
                                    ));
                                }
                            }
                            for (int i = 0; i < NPCID.Count; i++)
                            {
                                if (Lang.GetNPCNameValue(i).ContainsInsensitive(Search))
                                {
                                    npcs.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        Lang.GetNPCNameValue(i), i
                                    ));
                                }
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, npcs,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found NPCs ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No NPCs were found.",
                            });
                        return;
                    }

                #endregion

                #region Tile

                case "t":
                case "-t":
                case "-tile":
                    {
                        var tiles = new List<string>();

                        await Task.Run(() =>
                        {
                            foreach (FieldInfo fi in typeof(TileID).GetFields())
                            {
                                var sb = new StringBuilder();
                                for (int i = 0; i < fi.Name.Length; i++)
                                {
                                    if (Char.IsUpper(fi.Name[i]) && i > 0)
                                    { sb.Append(" ").Append(fi.Name[i]); }
                                    else { sb.Append(fi.Name[i]); }
                                }

                                string name = sb.ToString();
                                if (name.ContainsInsensitive(Search))
                                {
                                    tiles.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        name, fi.GetValue(null)
                                    ));
                                }
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, tiles,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found Tiles ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No tiles were found.",
                            });
                        return;
                    }

                #endregion

                #region Wall

                case "w":
                case "-w":
                case "-wall":
                    {
                        var walls = new List<string>();

                        await Task.Run(() =>
                        {
                            foreach (FieldInfo fi in typeof(WallID).GetFields())
                            {
                                var sb = new StringBuilder();
                                for (int i = 0; i < fi.Name.Length; i++)
                                {
                                    if (Char.IsUpper(fi.Name[i]) && i > 0)
                                    { sb.Append(" ").Append(fi.Name[i]); }
                                    else { sb.Append(fi.Name[i]); }
                                }

                                string name = sb.ToString();
                                if (name.ContainsInsensitive(Search))
                                {
                                    walls.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        name, fi.GetValue(null)
                                    ));
                                }
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, walls,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found Walls ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No walls were found.",
                            });
                        return;
                    }

                #endregion

                #region Prefix

                case "p":
                case "-p":
                case "-prefix":
                    {
                        var prefixes = new List<string>();

                        await Task.Run(() =>
                        {
                            foreach (FieldInfo fi in typeof(PrefixID).GetFields())
                            {
                                var sb = new StringBuilder();
                                for (int i = 0; i < fi.Name.Length; i++)
                                {
                                    if (Char.IsUpper(fi.Name[i]) && i > 0)
                                    { sb.Append(" ").Append(fi.Name[i]); }
                                    else { sb.Append(fi.Name[i]); }
                                }

                                string name = sb.ToString();
                                if (name.ContainsInsensitive(Search))
                                {
                                    prefixes.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        name, fi.GetValue(null)
                                    ));
                                }
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, prefixes,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found Prefixes ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No prefixes were found.",
                            });
                        return;
                    }

                #endregion

                #region Paint

                case "pa":
                case "-pa":
                case "-paint":
                    {
                        var paints = new List<string>();

                        await Task.Run(() =>
                        {
                            for (int i = 0; i < AllPaintsList.Count; i++)
                            {
                                string paint = AllPaintsList[i];
                                if (paint.ContainsInsensitive(Search))
                                {
                                    paints.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        paint, (i + 1)
                                    ));
                                }
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, paints,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found Paint ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No paints were found.",
                            });
                        return;
                    }

                #endregion

                #region Buff

                case "b":
                case "-b":
                case "-buff":
                    {
                        var buffs = new List<string>();

                        await Task.Run(() =>
                        {
                            foreach (FieldInfo fi in typeof(BuffID).GetFields())
                            {
                                var sb = new StringBuilder();
                                for (int i = 0; i < fi.Name.Length; i++)
                                {
                                    if (Char.IsUpper(fi.Name[i]) && i > 0)
                                    { sb.Append(" ").Append(fi.Name[i]); }
                                    else { sb.Append(fi.Name[i]); }
                                }

                                string name = sb.ToString();
                                if (name.ContainsInsensitive(Search))
                                {
                                    buffs.Add(string.Format
                                    (
                                        "{0} (ID: {1})",
                                        name, fi.GetValue(null)
                                    ));
                                }
                            }
                        });

                        PaginationTools.SendPage(e.Player, Page, buffs,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Found Buffs ({0}/{1}):",
                                FooterFormat = string.Format
                                (
                                    "Type /find {0} {1} {{0}} for more",
                                    e.Parameters[0], Search
                                ),
                                NothingToDisplayString = "No buffs were found.",
                            });
                        return;
                    }

                #endregion

                default: { FindHelp(e.Player); return; }
            }
        }

		private static System.Timers.Timer FreezeTimer = new System.Timers.Timer(1000);

		public static void FreezeTime(CommandArgs e)
		{
			if (FreezeTimer.Enabled)
			{
				FreezeTimer.Stop();
				TSPlayer.All.SendInfoMessage("{0} unfroze the time.", e.Player.Name);
			}
			else
			{
				bool dayTime = Main.dayTime;
				double time = Main.time;

				FreezeTimer.Dispose();
				FreezeTimer = new System.Timers.Timer(1000);
				FreezeTimer.Elapsed += (o, ee) =>
				{
					Main.dayTime = dayTime;
					Main.time = time;
					TSPlayer.All.SendData(PacketTypes.TimeSet);
				};
				FreezeTimer.Start();
				TSPlayer.All.SendInfoMessage("{0} froze the time.", e.Player.Name);
			}
		}

		public static async void DeleteHome(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}delhome <home name>", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			string homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "home";
			Home home = await EssentialsPlus.Homes.GetAsync(e.Player, homeName);
			if (home != null)
			{
				if (await EssentialsPlus.Homes.DeleteAsync(e.Player, homeName))
				{
					e.Player.SendSuccessMessage("Deleted your home '{0}'.", homeName);
				}
				else
				{
					e.Player.SendErrorMessage("Could not delete home, check logs for more details.");
				}
			}
			else
			{
				e.Player.SendErrorMessage("Invalid home '{0}'!", homeName);
			}
		}

		public static async void MyHome(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}myhome <home name>", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			if (Regex.Match(e.Message, @"^\w+ -l(?:ist)?$").Success)
			{
				List<Home> homes = await EssentialsPlus.Homes.GetAllAsync(e.Player);
				e.Player.SendInfoMessage(homes.Count == 0 ? "You have no homes set." : "List of homes: {0}", string.Join(", ", homes.Select(h => h.Name)));
			}
			else
			{
				string homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "home";
				Home home = await EssentialsPlus.Homes.GetAsync(e.Player, homeName);
				if (home != null)
				{
					e.Player.Teleport(home.X, home.Y);
					e.Player.SendSuccessMessage("Teleported you to your home '{0}'.", homeName);
				}
				else
				{
					e.Player.SendErrorMessage("Invalid home '{0}'!", homeName);
				}
			}
		}
		public static async void SetHome(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}sethome <home name>", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			string homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "home";
			if (await EssentialsPlus.Homes.GetAsync(e.Player, homeName) != null)
			{
				if (await EssentialsPlus.Homes.UpdateAsync(e.Player, homeName, e.Player.X, e.Player.Y))
				{
					e.Player.SendSuccessMessage("Updated your home '{0}'.", homeName);
				}
				else
				{
					e.Player.SendErrorMessage("Could not update home, check logs for more details.");
				}
				return;
			}

			if ((await EssentialsPlus.Homes.GetAllAsync(e.Player)).Count >= e.Player.Group.GetDynamicPermission(Permissions.HomeSet))
			{
				e.Player.SendErrorMessage("You have reached your home limit!");
				return;
			}

			if (await EssentialsPlus.Homes.AddAsync(e.Player, homeName, e.Player.X, e.Player.Y))
			{
				e.Player.SendSuccessMessage("Set your home '{0}'.", homeName);
			}
			else
			{
				e.Player.SendErrorMessage("Could not set home, check logs for more details.");
			}
		}

		public static async void KickAll(CommandArgs e)
		{
			var regex = new Regex(@"^\w+(?: -(\w+))* ?(.*)$");
			Match match = regex.Match(e.Message);
			bool noSave = false;
			foreach (Capture capture in match.Groups[1].Captures)
			{
				switch (capture.Value.ToLowerInvariant())
				{
					case "nosave":
						noSave = true;
						continue;
					default:
						e.Player.SendSuccessMessage("Valid {0}kickall switches:", TShock.Config.Settings.CommandSpecifier);
						e.Player.SendInfoMessage("-nosave: Kicks without saving SSC data.");
						return;
				}
			}

			int kickLevel = e.Player.Group.GetDynamicPermission(Permissions.KickAll);
			string reason = String.IsNullOrWhiteSpace(match.Groups[2].Value) ? "No reason." : match.Groups[2].Value;
			await Task.WhenAll(TShock.Players.Where(p => p != null && p.Group.GetDynamicPermission(Permissions.KickAll) < kickLevel).Select(p => Task.Run(() =>
			{
				if (!noSave && p.IsLoggedIn)
				{
					p.SaveServerCharacter();
				}
				p.Disconnect("Kicked: " + reason);
			})));
			e.Player.SendSuccessMessage("Kicked everyone for '{0}'.", reason);
        }

        public static async void RepeatLast(CommandArgs e)
        {
            string arg0 = e.Parameters.FirstOrDefault() ?? "1";
            List<string> lastCommands = e.Player.GetPlayerInfo().LastCommands;
            if (lastCommands.Count == 0)
            {
                e.Player.SendErrorMessage("You don't have last commands!");
                return;
            }

            if ((arg0.ToLower() == "list") || (arg0.ToLower() == "l"))
            {
                List<string> formated = new List<string>();
                for (int i = 1; i <= lastCommands.Count; i++)
                {
                    if (e.Player.RealPlayer)
                    {
                        formated.Insert(0, string.Format
                        (
                            "[c/808080:({0})] {1}{2}",
                            i, TShock.Config.Settings.CommandSpecifier,
                            lastCommands[i - 1]
                        ));
                    }
                    else
                    {
                        formated.Insert(0, string.Format
                        (
                            "[{0}] {1}{2}",
                            i, TShock.Config.Settings.CommandSpecifier,
                            lastCommands[i - 1]
                        ));
                    }
                }
                e.Player.SendInfoMessage(string.Join("\n", formated));
                return;
            }

            int Index = 1;
            if ((e.Parameters.Count > 0)
                && (!int.TryParse(arg0, out Index)
                    || (Index < 1) || (Index > lastCommands.Count)))
            {
                e.Player.SendErrorMessage("Invalid command index!");
                return;
            }

            e.Player.SendSuccessMessage("Repeated {0} command '{1}'!", StringExtensions.GetIndex(Index), lastCommands[Index - 1]);
            await Task.Run(() => TShockAPI.Commands.HandleCommand(e.Player, e.Message[0] + lastCommands[Index - 1]));
        }

		public static async void Mute(CommandArgs e)
		{
			string subCmd = e.Parameters.FirstOrDefault() ?? "help";
			switch (subCmd.ToLowerInvariant())
			{
				#region Add

				case "add":
					{
						var regex = new Regex(@"^\w+ \w+ (?:""(.+?)""|([^\s]+?))(?: (.+))?$");
						Match match = regex.Match(e.Message);
						if (!match.Success)
						{
							e.Player.SendErrorMessage("Invalid syntax! Proper syntax: /mute add <name> [time]");
							return;
						}

						int seconds = Int32.MaxValue / 1000;
						if (!String.IsNullOrWhiteSpace(match.Groups[3].Value) &&
							(!TShock.Utils.TryParseTime(match.Groups[3].Value, out seconds) || seconds <= 0 ||
							 seconds > Int32.MaxValue / 1000))
						{
							e.Player.SendErrorMessage("Invalid time '{0}'!", match.Groups[3].Value);
							return;
						}

						string playerName = String.IsNullOrWhiteSpace(match.Groups[2].Value)
							? match.Groups[1].Value
							: match.Groups[2].Value;
						List<TSPlayer> players = TSPlayer.FindByNameOrID(playerName);
						if (players.Count == 0)
						{
							UserAccount user = TShock.UserAccounts.GetUserAccountByName(playerName);
							if (user == null)
								e.Player.SendErrorMessage("Invalid player or account '{0}'!", playerName);
							else
							{
								if (TShock.Groups.GetGroupByName(user.Group).GetDynamicPermission(Permissions.Mute) >=
									e.Player.Group.GetDynamicPermission(Permissions.Mute))
								{
									e.Player.SendErrorMessage("You can't mute {0}!", user.Name);
									return;
								}

								if (await EssentialsPlus.Mutes.AddAsync(user, DateTime.UtcNow.AddSeconds(seconds)))
								{
									TSPlayer.All.SendInfoMessage("{0} muted {1}.", e.Player.Name, user.Name);
								}
								else
								{
									e.Player.SendErrorMessage("Could not mute, check logs for details.");
								}
							}
						}
						else if (players.Count > 1)
						{
							e.Player.SendErrorMessage("More than one player matched: {0}", String.Join(", ", players.Select(p => p.Name)));
						}
						else
						{
							if (players[0].Group.GetDynamicPermission(Permissions.Mute) >=
								e.Player.Group.GetDynamicPermission(Permissions.Mute))
							{
								e.Player.SendErrorMessage("You can't mute {0}!", players[0].Name);
								return;
							}

							if (await EssentialsPlus.Mutes.AddAsync(players[0], DateTime.UtcNow.AddSeconds(seconds)))
							{
								TSPlayer.All.SendInfoMessage("{0} muted {1}.", e.Player.Name, players[0].Name);

								players[0].mute = true;
								try
								{
									await Task.Delay(TimeSpan.FromSeconds(seconds), players[0].GetPlayerInfo().MuteToken);
									players[0].mute = false;
									players[0].SendInfoMessage("You have been unmuted.");
								}
								catch (TaskCanceledException)
								{
								}
							}
							else
								e.Player.SendErrorMessage("Could not mute, check logs for details.");
						}
					}
					return;

				#endregion

				#region Delete

				case "del":
				case "delete":
					{
						var regex = new Regex(@"^\w+ \w+ (?:""(.+?)""|([^\s]*?))$");
						Match match = regex.Match(e.Message);
						if (!match.Success)
						{
							e.Player.SendErrorMessage("Invalid syntax! Proper syntax: /mute del <name>");
							return;
						}

						string playerName = String.IsNullOrWhiteSpace(match.Groups[2].Value)
							? match.Groups[1].Value
							: match.Groups[2].Value;
						List<TSPlayer> players = TSPlayer.FindByNameOrID(playerName);
						if (players.Count == 0)
						{
							UserAccount user = TShock.UserAccounts.GetUserAccountByName(playerName);
							if (user == null)
								e.Player.SendErrorMessage("Invalid player or account '{0}'!", playerName);
							else
							{
								if (await EssentialsPlus.Mutes.DeleteAsync(user))
									TSPlayer.All.SendInfoMessage("{0} unmuted {1}.", e.Player.Name, user.Name);
								else
									e.Player.SendErrorMessage("Could not unmute, check logs for details.");
							}
						}
						else if (players.Count > 1)
							e.Player.SendErrorMessage("More than one player matched: {0}", String.Join(", ", players.Select(p => p.Name)));
						else
						{
							if (await EssentialsPlus.Mutes.DeleteAsync(players[0]))
							{
								players[0].mute = false;
								TSPlayer.All.SendInfoMessage("{0} unmuted {1}.", e.Player.Name, players[0].Name);
							}
							else
								e.Player.SendErrorMessage("Could not unmute, check logs for details.");
						}
					}
					return;

				#endregion

				#region Help

				default:
					e.Player.SendSuccessMessage("Mute Sub-Commands:");
					e.Player.SendInfoMessage("add <name> [time] - Mutes a player or account.");
					e.Player.SendInfoMessage("del <name> - Unmutes a player or account.");
					return;

				#endregion
			}
		}

		public static void PvP(CommandArgs e)
		{
			e.TPlayer.hostile = !e.TPlayer.hostile;
			string hostile = Language.GetTextValue(e.TPlayer.hostile ? "LegacyMultiplayer.11" : "LegacyMultiplayer.12", e.Player.Name);
			TSPlayer.All.SendData(PacketTypes.TogglePvp, "", e.Player.Index);
			TSPlayer.All.SendMessage(hostile, Main.teamColor[e.Player.Team]);
		}

		public static void Ruler(CommandArgs e)
		{
			if (e.Parameters.Count == 0)
			{
				if (e.Player.TempPoints.Any(p => p == Point.Zero))
				{
					e.Player.SendErrorMessage("Ruler points are not set up!");
					return;
				}

				Point p1 = e.Player.TempPoints[0];
				Point p2 = e.Player.TempPoints[1];
				int x = Math.Abs(p1.X - p2.X) + 1;
				int y = Math.Abs(p1.Y - p2.Y) + 1;
				double cartesian = Math.Sqrt(x * x + y * y);
				e.Player.SendInfoMessage("Distances: X: {0}, Y: {1}, Cartesian: {2:N3}", x, y, cartesian);
			}
			else if (e.Parameters.Count == 1)
			{
				if (e.Parameters[0] == "1")
				{
					e.Player.AwaitingTempPoint = 1;
					e.Player.SendInfoMessage("Modify a block to set the first ruler point.");
				}
				else if (e.Parameters[0] == "2")
				{
					e.Player.AwaitingTempPoint = 2;
					e.Player.SendInfoMessage("Modify a block to set the second ruler point.");
				}
				else
					e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}ruler [1/2]", TShock.Config.Settings.CommandSpecifier);
			}
			else
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}ruler [1/2]", TShock.Config.Settings.CommandSpecifier);
		}

		public static void Send(CommandArgs e)
		{
			var regex = new Regex(@"^\w+(?: (\d+),(\d+),(\d+))? (.+)$");
			Match match = regex.Match(e.Message);
			if (!match.Success)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}send [r,g,b] <text...>", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			byte r = e.Player.Group.R;
			byte g = e.Player.Group.G;
			byte b = e.Player.Group.B;
			if (!String.IsNullOrWhiteSpace(match.Groups[1].Value) && !String.IsNullOrWhiteSpace(match.Groups[2].Value) && !String.IsNullOrWhiteSpace(match.Groups[3].Value) &&
				(!byte.TryParse(match.Groups[1].Value, out r) || !byte.TryParse(match.Groups[2].Value, out g) || !byte.TryParse(match.Groups[3].Value, out b)))
			{
				e.Player.SendErrorMessage("Invalid color!");
				return;
			}
			TSPlayer.All.SendMessage(match.Groups[4].Value, new Color(r, g, b));
        }

        public static void Sudo(CommandArgs e)
        {
            var regex = new Regex(string.Format(@"^\w+(?: -(\w+))* (?:""(.+?)""|([^\s]*?)) (.+)$"));
            Match match = regex.Match(e.Message);
            if (!match.Success)
            {
                e.Player.SendErrorMessage("Invalid syntax! Proper syntax: /sudo [-force] <player> <command...>");
                e.Player.SendInfoMessage("-f, -force: Force sudo, ignoring permissions.");
                return;
            }

            bool force = false;
            foreach (Capture capture in match.Groups[1].Captures)
            {
                switch (capture.Value.ToLowerInvariant())
                {
                    case "f":
                    case "force":
                        if (!e.Player.Group.HasPermission(Permissions.SudoForce))
                        {
                            e.Player.SendErrorMessage("You do not have access to the switch '-{0}'!", capture.Value);
                            return;
                        }
                        force = true;
                        continue;
                    default:
                        e.Player.SendSuccessMessage("Valid {0}sudo switches:", TShock.Config.Settings.CommandSpecifier);
                        e.Player.SendInfoMessage("-f, -force: Force sudo, ignoring permissions.");
                        return;
                }
            }

            string playerName = string.IsNullOrWhiteSpace(match.Groups[3].Value)
                                    ? match.Groups[2].Value
                                    : match.Groups[3].Value;
            string command = match.Groups[4].Value;
            if (!command.StartsWith(TShock.Config.Settings.CommandSpecifier)
                && !command.StartsWith(TShock.Config.Settings.CommandSilentSpecifier))
            { command = TShock.Config.Settings.CommandSpecifier + command; }

            List<TSPlayer> players = TSPlayer.FindByNameOrID(playerName);
            if (players.Count == 0)
            { e.Player.SendErrorMessage("Invalid player '{0}'!", playerName); }
            else if (players.Count > 1)
            {
                e.Player.SendErrorMessage("More than one player matched: {0}",
                    string.Join(", ", players.Select(p => p.Name)));
            }
            else
            {
                if ((e.Player.Group.GetDynamicPermission(Permissions.Sudo)
                    <= players[0].Group.GetDynamicPermission(Permissions.Sudo))
                    && !e.Player.Group.HasPermission(Permissions.SudoSuper))
                {
                    e.Player.SendErrorMessage("You cannot force {0} to execute {1}!",
                        players[0].Name, command);
                    return;
                }

                string cmd = command.Split(' ')[0].Substring(1).ToLower();
                if (string.IsNullOrWhiteSpace(cmd))
                {
                    e.Player.SendErrorMessage("Invalid command.");
                    return;
                }

                List<Command> cmds = TShockAPI.Commands.ChatCommands.Where(c => c.Names.Contains(cmd)).ToList();
                if (cmds.Count == 0)
                {
                    e.Player.SendErrorMessage("Invalid command.");
                    return;
                }
                else if (cmds.Any(c => c.Permissions.Any(p => !e.Player.HasPermission(p))))
                {
                    e.Player.SendErrorMessage("You do not have permission to use this command.");
                    return;
                }

                e.Player.SendSuccessMessage("Forced {0} to execute {1}.", players[0].Name, command);
                if (!e.Player.Group.HasPermission(Permissions.SudoInvisible))
                { players[0].SendInfoMessage("{0} forced you to execute {1}.", e.Player.Name, command); }
                players[0].ExecuteCommand(command, force);
            }
        }

        public static async void TimeCmd(CommandArgs e)
		{
			var regex = new Regex(String.Format(@"^\w+(?: -(\w+))* (\w+) (?:{0})?(.+)$", TShock.Config.Settings.CommandSpecifier));
			Match match = regex.Match(e.Message);
			if (!match.Success)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}timecmd [-switches...] <time> <command...>", TShock.Config.Settings.CommandSpecifier);
				e.Player.SendSuccessMessage("Valid {0}timecmd switches:", TShock.Config.Settings.CommandSpecifier);
				e.Player.SendInfoMessage("-r, -repeat: Repeats the time command indefinitely.");
				return;
			}

			bool repeat = false;
			foreach (Capture capture in match.Groups[1].Captures)
			{
				switch (capture.Value.ToLowerInvariant())
				{
					case "r":
					case "repeat":
						repeat = true;
						break;
					default:
						e.Player.SendSuccessMessage("Valid {0}timecmd switches:", TShock.Config.Settings.CommandSpecifier);
						e.Player.SendInfoMessage("-r, -repeat: Repeats the time command indefinitely.");
						return;
				}
			}

			if (!TShock.Utils.TryParseTime(match.Groups[2].Value, out int seconds) || seconds <= 0 || seconds > Int32.MaxValue / 1000)
			{
				e.Player.SendErrorMessage("Invalid time '{0}'!", match.Groups[2].Value);
				return;
			}

			if (repeat)
				e.Player.SendSuccessMessage("Queued command '{0}{1}' indefinitely. Use /cancel to cancel!", TShock.Config.Settings.CommandSpecifier, match.Groups[3].Value);
			else
				e.Player.SendSuccessMessage("Queued command '{0}{1}'. Use /cancel to cancel!", TShock.Config.Settings.CommandSpecifier, match.Groups[3].Value);
			e.Player.AddResponse("cancel", o =>
			{
				e.Player.GetPlayerInfo().CancelTimeCmd();
				e.Player.SendSuccessMessage("Cancelled all time commands!");
			});

			CancellationToken token = e.Player.GetPlayerInfo().TimeCmdToken;
			try
			{
				await Task.Run(async () =>
				{
					do
					{
						await Task.Delay(TimeSpan.FromSeconds(seconds), token);
						TShockAPI.Commands.HandleCommand(e.Player, TShock.Config.Settings.CommandSpecifier + match.Groups[3].Value);
					}
					while (repeat);
				}, token);
			}
			catch (TaskCanceledException)
			{
			}
		}

		public static void Back(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}back [steps]", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			int steps = 1;
			if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out steps) || steps <= 0))
			{
				e.Player.SendErrorMessage("Invalid number of steps '{0}'!", e.Parameters[0]);
				return;
			}

			PlayerInfo info = e.Player.GetPlayerInfo();
			if (info.BackHistoryCount == 0)
			{
				e.Player.SendErrorMessage("Could not teleport back!");
				return;
			}

			steps = Math.Min(steps, info.BackHistoryCount);
			e.Player.SendSuccessMessage("Teleported back {0} step{1}.", steps, steps == 1 ? "" : "s");
			Vector2 vector = info.PopBackHistory(steps);
			e.Player.Teleport(vector.X, vector.Y);
		}
		public static async void Down(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Correct syntax: {0}down [levels]", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			int levels = 1;
			if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
			{
				e.Player.SendErrorMessage("Invalid number of levels '{0}'!", levels);
				return;
			}

			int currentLevel = 0;
			bool empty = false;
			int x = Math.Max(0, Math.Min(e.Player.TileX, Main.maxTilesX - 2));
			int y = Math.Max(0, Math.Min(e.Player.TileY + 3, Main.maxTilesY - 3));

			await Task.Run(() =>
			{
				for (int j = y; currentLevel < levels && j < Main.maxTilesY - 2; j++)
				{
					if (Main.tile[x, j].IsEmpty() && Main.tile[x + 1, j].IsEmpty() &&
						Main.tile[x, j + 1].IsEmpty() && Main.tile[x + 1, j + 1].IsEmpty() &&
						Main.tile[x, j + 2].IsEmpty() && Main.tile[x + 1, j + 2].IsEmpty())
					{
						empty = true;
					}
					else if (empty)
					{
						empty = false;
						currentLevel++;
						y = j;
					}
				}
			});

			if (currentLevel == 0)
				e.Player.SendErrorMessage("Could not teleport down!");
			else
			{
				if (e.Player.Group.HasPermission(Permissions.TpBack))
					e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
				e.Player.Teleport(16 * x, 16 * y - 10);
				e.Player.SendSuccessMessage("Teleported down {0} level{1}.", currentLevel, currentLevel == 1 ? "" : "s");
			}
		}
		public static async void Left(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Correct syntax: {0}left [levels]", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			int levels = 1;
			if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
			{
				e.Player.SendErrorMessage("Invalid number of levels '{0}'!", levels);
				return;
			}

			int currentLevel = 0;
			bool solid = false;
			int x = Math.Max(0, Math.Min(e.Player.TileX, Main.maxTilesX - 2));
			int y = Math.Max(0, Math.Min(e.Player.TileY, Main.maxTilesY - 3));

			await Task.Run(() =>
			{
				for (int i = x; currentLevel < levels && i >= 0; i--)
				{
					if (Main.tile[i, y].IsEmpty() && Main.tile[i + 1, y].IsEmpty() &&
						Main.tile[i, y + 1].IsEmpty() && Main.tile[i + 1, y + 1].IsEmpty() &&
						Main.tile[i, y + 2].IsEmpty() && Main.tile[i + 1, y + 2].IsEmpty())
					{
						if (solid)
						{
							solid = false;
							currentLevel++;
							x = i;
						}
					}
					else
						solid = true;
				}
			});

			if (currentLevel == 0)
				e.Player.SendErrorMessage("Could not teleport left!");
			else
			{
				if (e.Player.Group.HasPermission(Permissions.TpBack))
					e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
				e.Player.Teleport(16 * x + 12, 16 * y);
				e.Player.SendSuccessMessage("Teleported left {0} level{1}.", currentLevel, currentLevel == 1 ? "" : "s");
			}
		}
		public static async void Right(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Correct syntax: {0}right [levels]", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			int levels = 1;
			if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
			{
				e.Player.SendErrorMessage("Invalid number of levels '{0}'!", levels);
				return;
			}

			int currentLevel = 0;
			bool solid = false;
			int x = Math.Max(0, Math.Min(e.Player.TileX + 1, Main.maxTilesX - 2));
			int y = Math.Max(0, Math.Min(e.Player.TileY, Main.maxTilesY - 3));

			await Task.Run(() =>
			{
				for (int i = x; currentLevel < levels && i < Main.maxTilesX - 1; i++)
				{
					if (Main.tile[i, y].IsEmpty() && Main.tile[i + 1, y].IsEmpty() &&
						Main.tile[i, y + 1].IsEmpty() && Main.tile[i + 1, y + 1].IsEmpty() &&
						Main.tile[i, y + 2].IsEmpty() && Main.tile[i + 1, y + 2].IsEmpty())
					{
						if (solid)
						{
							solid = false;
							currentLevel++;
							x = i;
						}
					}
					else
						solid = true;
				}
			});

			if (currentLevel == 0)
				e.Player.SendErrorMessage("Could not teleport right!");
			else
			{
				if (e.Player.Group.HasPermission(Permissions.TpBack))
					e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
				e.Player.Teleport(16 * x, 16 * y);
				e.Player.SendSuccessMessage("Teleported right {0} level{1}.", currentLevel, currentLevel == 1 ? "" : "s");
			}
		}
		public static async void Up(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Correct syntax: {0}up [levels]", TShock.Config.Settings.CommandSpecifier);
				return;
			}

			int levels = 1;
			if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
			{
				e.Player.SendErrorMessage("Invalid number of levels '{0}'!", levels);
				return;
			}

			int currentLevel = 0;
			bool solid = false;
			int x = Math.Max(0, Math.Min(e.Player.TileX, Main.maxTilesX - 2));
			int y = Math.Max(0, Math.Min(e.Player.TileY, Main.maxTilesY - 3));

			await Task.Run(() =>
			{
				for (int j = y; currentLevel < levels && j >= 0; j--)
				{
					if (Main.tile[x, j].IsEmpty() && Main.tile[x + 1, j].IsEmpty() &&
						Main.tile[x, j + 1].IsEmpty() && Main.tile[x + 1, j + 1].IsEmpty() &&
						Main.tile[x, j + 2].IsEmpty() && Main.tile[x + 1, j + 2].IsEmpty())
					{
						if (solid)
						{
							solid = false;
							currentLevel++;
							y = j;
						}
					}
					else
						solid = true;
				}
			});

			if (currentLevel == 0)
				e.Player.SendErrorMessage("Could not teleport up!");
			else
			{
				if (e.Player.Group.HasPermission(Permissions.TpBack))
					e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
				e.Player.Teleport(16 * x, 16 * y + 6);
				e.Player.SendSuccessMessage("Teleported up {0} level{1}.", currentLevel, currentLevel == 1 ? "" : "s");
			}
		}
	}
}
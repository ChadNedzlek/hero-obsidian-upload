using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WatiN.Core;
using Settings = ObsidianUpload.Properties.Settings;

namespace ObsidianUpload
{
	public partial class MainWindow
	{
		public static readonly DependencyProperty AllCampaignsProperty = DependencyProperty.Register(
			"AllCampaigns",
			typeof(ObservableCollection<CampaignInfo>),
			typeof(MainWindow),
			new PropertyMetadata(default(ObservableCollection<CampaignInfo>)));

		public static readonly DependencyProperty CurrentCampaignProperty = DependencyProperty.Register(
			"CurrentCampaign",
			typeof(CampaignInfo),
			typeof(MainWindow),
			new PropertyMetadata(default(CampaignInfo), OnCampaignChanged));

		public static readonly DependencyProperty CurrentCharacterProperty = DependencyProperty.Register(
			"CurrentCharacter",
			typeof(CharacterInfo),
			typeof(MainWindow),
			new PropertyMetadata(default(CharacterInfo), OnCharacterChanged));

		private static readonly DependencyPropertyKey _sourceCharacterPropertyKey = DependencyProperty.RegisterReadOnly(
			"SourceCharacter",
			typeof(CharacterSummary),
			typeof(MainWindow),
			new PropertyMetadata(default(CharacterSummary)));

		public static readonly DependencyProperty SourceCharacterProperty =
			_sourceCharacterPropertyKey.DependencyProperty;

		private static readonly DependencyPropertyKey _targetCharacterPropertyKey = DependencyProperty.RegisterReadOnly(
			"TargetCharacter",
			typeof(CharacterSummary),
			typeof(MainWindow),
			new PropertyMetadata(default(CharacterSummary)));

		public static readonly DependencyProperty TargetCharacterProperty =
			_targetCharacterPropertyKey.DependencyProperty;

		private static readonly DependencyPropertyKey _userNamePropertyKey = DependencyProperty.RegisterReadOnly(
			"UserName",
			typeof(string),
			typeof(MainWindow),
			new PropertyMetadata(default(string)));

		public static readonly DependencyProperty UserNameProperty = _userNamePropertyKey.DependencyProperty;
		private readonly Settings _settings;

		private JObject _sourceCharacterObject;

		public MainWindow()
		{
			_settings = Settings.Default;
			InitializeComponent();
			UserName = _settings.UserName;
			if (!string.IsNullOrEmpty(_settings.OauthConsumerKey))
			{
#pragma warning disable 4014
				LoadInformation();
#pragma warning restore 4014
			}

			string[] args = Environment.GetCommandLineArgs();
			if (args.Length == 2)
			{
#pragma warning disable 4014
				LoadSourceCharacter(args[1]);
#pragma warning restore 4014
			}
		}

		public ObservableCollection<CampaignInfo> AllCampaigns
		{
			get { return (ObservableCollection<CampaignInfo>)GetValue(AllCampaignsProperty); }
			set { SetValue(AllCampaignsProperty, value); }
		}

		public CampaignInfo CurrentCampaign
		{
			get { return (CampaignInfo)GetValue(CurrentCampaignProperty); }
			set { SetValue(CurrentCampaignProperty, value); }
		}

		public CharacterInfo CurrentCharacter
		{
			get { return (CharacterInfo)GetValue(CurrentCharacterProperty); }
			set { SetValue(CurrentCharacterProperty, value); }
		}

		public CharacterSummary SourceCharacter
		{
			get { return (CharacterSummary)GetValue(SourceCharacterProperty); }
			private set { SetValue(_sourceCharacterPropertyKey, value); }
		}

		public CharacterSummary TargetCharacter
		{
			get { return (CharacterSummary)GetValue(TargetCharacterProperty); }
			private set { SetValue(_targetCharacterPropertyKey, value); }
		}

		public string UserName
		{
			get { return (string)GetValue(UserNameProperty); }
			private set { SetValue(_userNamePropertyKey, value); }
		}

		private void LoadSourceCharacter(string fileName)
		{
			XDocument charXml;
			using (Stream input = File.OpenRead(fileName))
				charXml = XDocument.Load(input);

			XElement elem = charXml.Element("document").Element("public").Element("character");

			Func<string, string, string, JProperty> a =
				(name, path, attr) => new JProperty(name, elem.XPathSelectElement(path).Attribute(attr).Value);
			Func<string, JProperty> n =
				name => new JProperty(name, elem.XPathSelectElement(name).Attribute("name").Value);
			Func<string, object, JProperty> v =
				(name, value) => new JProperty(name, value);

			var attackSize = "0";
			var cmbSize = "0";

			switch (elem.Element("size").Attribute("name").Value)
			{
				case "Large":
					attackSize = "-1";
					cmbSize = "+1";
					break;
				case "Small":
					attackSize = "+1";
					cmbSize = "11";
					break;
			}

			var sheet = new JObject(
				a("race", "race", "racetext"),
				new JProperty("class", (elem.XPathSelectElement("favoredclasses/favoredclass") ?? elem.XPathSelectElement("classes/class")).Attribute("name").Value),
				a("gender", "personal", "gender"),
				a("age", "personal", "age"),
				n("size"),
				n("alignment"),
				n("deity"),
				a("level", "classes", "summary"),
				a("experience_points", "xp", "total"),
				v(
					"languages",
					string.Join(
						", ",
						elem.Element("languages").Elements("language").Select(l => l.Attribute("name").Value))),
				a("str", "attributes/attribute[@name='Strength']/attrvalue", "text"),
				a("str_mod", "attributes/attribute[@name='Strength']/attrbonus", "text"),
				a("dex", "attributes/attribute[@name='Dexterity']/attrvalue", "text"),
				a("dex_mod", "attributes/attribute[@name='Dexterity']/attrbonus", "text"),
				a("con", "attributes/attribute[@name='Constitution']/attrvalue", "text"),
				a("con_mod", "attributes/attribute[@name='Constitution']/attrbonus", "text"),
				a("int", "attributes/attribute[@name='Intelligence']/attrvalue", "text"),
				a("int_mod", "attributes/attribute[@name='Intelligence']/attrbonus", "text"),
				a("wis", "attributes/attribute[@name='Wisdom']/attrvalue", "text"),
				a("wis_mod", "attributes/attribute[@name='Wisdom']/attrbonus", "text"),
				a("cha", "attributes/attribute[@name='Charisma']/attrvalue", "text"),
				a("cha_mod", "attributes/attribute[@name='Charisma']/attrbonus", "text"),
				a("hp", "health", "hitpoints"),
				a("initative_total", "initiative", "total"),
				a("misc_init_mod", "initiative", "misctext"),
				a("speed", "movement/speed", "text"),
				a("attack_melee_total", "attack", "meleeattack"),
				a("attack_ranged_total", "attack", "rangedattack"),
				a("bab", "attack", "baseattack"),
				v("size_mode", attackSize),
				a("fortitude_total", "saves/save[@abbr='Fort']", "save"),
				a("base_fort_save", "saves/save[@abbr='Fort']", "base"),
				a("magic_fort_mod", "saves/save[@abbr='Fort']", "fromresist"),
				a("misc_fort_mod", "saves/save[@abbr='Fort']", "frommisc"),
				a("reflex_total", "saves/save[@abbr='Ref']", "save"),
				a("base_reflex_save", "saves/save[@abbr='Ref']", "base"),
				a("magic_reflex_mod", "saves/save[@abbr='Ref']", "fromresist"),
				a("misc_reflex_mod", "saves/save[@abbr='Ref']", "frommisc"),
				a("willpower_total", "saves/save[@abbr='Will']", "save"),
				a("base_willpower_save", "saves/save[@abbr='Will']", "base"),
				a("magic_willpower_mod", "saves/save[@abbr='Will']", "fromresist"),
				a("misc_willpower_mod", "saves/save[@abbr='Will']", "frommisc"),
				a("cmb_total", "maneuvers", "cmb"),
				a("cmb_stat_mod", "attributes/attribute[@name='Strength']/attrbonus", "text"),
				v("size_mod_cmb", cmbSize),
				v("cmd_stat", "Str"),
				a("cmd_total", "maneuvers", "cmd"),
				a("ac", "armorclass", "ac"),
				a("touch_ac", "armorclass", "touch"),
				a("flat_footed_ac", "armorclass", "flatfooted"),
				a("armor_bonus", "armorclass", "fromarmor"),
				a("shield_bonus", "armorclass", "fromshield"),
				a("dex_mod_armor", "armorclass", "fromdexterity"),
				a("natural_armor", "armorclass", "fromnatural"),
				a("deflection_mod", "armorclass", "fromdeflect"),
				a("dodge_mod", "armorclass", "fromdodge"),
				a("misc_mod", "armorclass", "frommisc"),
				a("light_load", "encumbrance", "light"),
				a("medium_load", "encumbrance", "medium"),
				a("heavy_load", "encumbrance", "heavy"),
				a("wealth_total", "money", "total"),
				a("pp", "money", "pp"),
				a("gp", "money", "gp"),
				a("sp", "money", "sp"),
				a("cp", "money", "cp"),
				a("height", "personal/charheight", "text"),
				a("weight", "personal/charweight", "text"),
				a("eyes", "personal", "eyes"),
				a("hair", "personal", "hair"),
				a("skin", "personal", "skin")
			);

			_sourceCharacterObject =
				new JObject(
					new JProperty("name", elem.Attribute("name").Value),
					v(
						"dynamic_sheet_template",
						new JObject(
							v("id", "f38b253c447f11e1858f4040ff56340d"),
							v("slug", "kpfrpg"),
							v("name", "Pathfinder RPG Universal Sheet")
						)),
					v("dynamic_sheet", sheet));
			{
				var aCounter = 0;
				foreach (XElement armor in elem.Element("defenses").Elements("armor"))
				{
					if (armor.Attribute("useradded")?.Value == "no")
					{
						continue;
					}

					aCounter++;

					sheet[$"armor{aCounter}"] = armor.Attribute("name").Value;
					sheet[$"armor{aCounter}_ac_bonus"] = armor.Attribute("ac").Value;

					if (armor.Attribute("equipped")?.Value == "yes")
					{
						sheet[$"armor{aCounter}_max_dex"] =
							elem.XPathSelectElement("penalties/penalty[@name='Max Dex Bonus']")?.Attribute("text").Value ??
							"";
						sheet[$"armor{aCounter}_penalty"] =
							elem.XPathSelectElement("penalties/penalty[@name='Armor Check Penalty']")?
								.Attribute("text")
								.Value ?? "";
					}
				}
			}
			{
				var wCounter = 0;
				foreach (XElement weapon in elem.Element("melee").Elements("weapon"))
				{
					wCounter++;

					sheet[$"weapon{wCounter}"] = weapon.Attribute("name").Value;
					sheet[$"weapon{wCounter}_attack_bonus"] = weapon.Attribute("attack").Value;
					sheet[$"weapon{wCounter}_damage"] = weapon.Attribute("damage").Value;
					sheet[$"weapon{wCounter}_critical"] = weapon.Attribute("crit").Value;
					sheet[$"weapon{wCounter}_range"] = "";
					sheet[$"weapon{wCounter}_type"] = weapon.Attribute("typetext").Value;
					sheet[$"weapon{wCounter}_notes"] = string.Join(
						", ",
						weapon.Attribute("categorytext")
							.Value.Split(',')
							.Select(c => c.Trim())
							.Where(c => c != "Melee Weapon"));
				}

				foreach (XElement weapon in elem.Element("ranged").Elements("weapon"))
				{
					wCounter++;

					sheet[$"weapon{wCounter}"] = weapon.Attribute("name").Value;
					sheet[$"weapon{wCounter}_attack_bonus"] = weapon.Attribute("attack").Value;
					sheet[$"weapon{wCounter}_damage"] = weapon.Attribute("damage").Value;
					sheet[$"weapon{wCounter}_critical"] = weapon.Attribute("crit").Value;
					sheet[$"weapon{wCounter}_range"] = weapon.Element("rangedattack").Attribute("rangeinctext").Value;
					sheet[$"weapon{wCounter}_type"] = weapon.Attribute("typetext").Value;
					sheet[$"weapon{wCounter}_notes"] = weapon.Attribute("categorytext").Value;
				}
				sheet["ctlWeaponCount"] = wCounter.ToString();
			}
			{
				var skillCount = 0;
				foreach (XElement skill in elem.Element("skills").Elements("skill"))
				{
					if (skill.Attribute("usable")?.Value == "no")
					{
						continue;
					}

					skillCount++;
					sheet[$"skill{skillCount}"] = skill.Attribute("name").Value;
					sheet[$"skill{skillCount}_cs"] = skill.Attribute("classskill")?.Value == "yes" ? "1" : "0";
					sheet[$"skill{skillCount}_ability"] = skill.Attribute("attrname").Value;
					sheet[$"skill{skillCount}_mod"] = skill.Attribute("value").Value;
					sheet[$"skill{skillCount}_ability_mod"] = skill.Attribute("attrbonus").Value;
					sheet[$"skill{skillCount}_ranks"] = skill.Attribute("ranks").Value;
					sheet[$"skill{skillCount}_misc_mod"] = "";
				}
				sheet["ctlSkillCount"] = skillCount.ToString();
			}

			{
				var featCount = 0;
				foreach (XElement feat in elem.Element("feats").Elements("feat"))
				{
					featCount++;
					sheet[$"feat{featCount}"] = feat.Attribute("name").Value;
				}
				sheet["ctlFeatCount"] = featCount.ToString();
			}
			{
				var traitCount = 0;
				foreach (XElement trait in elem.Element("traits").Elements("trait"))
				{
					traitCount++;
					sheet[$"trait{traitCount}"] = trait.Attribute("name").Value;
				}
				sheet["ctlTraitCount"] = traitCount.ToString();
			}
			{
				var abilityCount = 0;
				foreach (XElement special in elem.Element("otherspecials").Elements("special"))
				{
					abilityCount++;
					sheet[$"special_ability{abilityCount}"] = special.Attribute("name").Value;
				}
				sheet["ctlAbilityCount"] = abilityCount.ToString();
			}

			{
				var spellSectionCount = 0;
				IEnumerable<XElement> known = elem.Element("spellsknown").Elements("spell");
				IEnumerable<XElement> memorized = elem.Element("spellsmemorized").Elements("spell");
				IEnumerable<IGrouping<string, XElement>> knownByClass = known.GroupBy(e => e.Attribute("class").Value);
				IEnumerable<IGrouping<string, XElement>> memorizedByClass =
					memorized.GroupBy(e => e.Attribute("class").Value);
				foreach (XElement spellClass in elem.Element("spellclasses").Elements("spellclass"))
				{
					string className = spellClass.Attribute("name").Value;
					XElement classElem = elem.XPathSelectElement($"classes/class[@name='{className}']");
					sheet[$"spsSec{spellSectionCount}_Spontaneous"] = spellClass.Attribute("spells")?.Value ==
						"Spontaneous";
					sheet[$"spsSec{spellSectionCount}_Spellbook"] = spellClass.Attribute("spells")?.Value == "Spellbook";
					sheet[$"spsSec{spellSectionCount}_Domains"] = "0";
					sheet[$"spsSec{spellSectionCount}_MaxLevel"] = spellClass.Attribute("maxspelllevel").Value;
					sheet[$"spsSec{spellSectionCount}_Minlevel"] =
						spellClass.Elements("spelllevel")
							.Select(l => int.Parse(l.Attribute("level").Value))
							.Min()
							.ToString();
					sheet[$"spsSec{spellSectionCount}_Name"] = className;

					int baseDc = int.Parse(classElem.Attribute("basespelldc").Value);

					IGrouping<string, XElement> knownForClass =
						knownByClass.FirstOrDefault(
							g => className.StartsWith(g.Key, StringComparison.OrdinalIgnoreCase));
					ILookup<string, XElement> knownByLevel = knownForClass?.ToLookup(k => k.Attribute("level").Value);
					IGrouping<string, XElement> memForClass =
						memorizedByClass.FirstOrDefault(
							g => className.StartsWith(g.Key, StringComparison.OrdinalIgnoreCase));
					ILookup<string, XElement> memByLevel = memForClass?.ToLookup(k => k.Attribute("level").Value);
					foreach (XElement spellLevel in spellClass.Elements("spelllevel"))
					{
						int level = int.Parse(spellLevel.Attribute("level").Value);
						sheet[$"spsSec{spellSectionCount}_DC{level}"] = (baseDc + level).ToString();

						XAttribute casts = spellLevel.Attribute("maxcasts");
						if (casts != null)
						{
							sheet[$"spsSec{spellSectionCount}_PerDayNumber{level}"] = casts.Value;
						}

						if (memByLevel != null)
						{
							List<XElement> memLevel = memByLevel[level.ToString()].ToList();
							sheet[$"spsSec{spellSectionCount}_Prepared{level}"] = string.Join(
								", ",
								memLevel.Select(l => l.Attribute("name").Value));
						}
						else if (knownByLevel != null)
						{
							List<XElement> levelList = knownByLevel[level.ToString()].ToList();
							sheet[$"spsSec{spellSectionCount}_KnownNumber{level}"] = levelList.Count.ToString();
							sheet[$"spsSec{spellSectionCount}_Prepared{level}"] = string.Join(
								", ",
								levelList.Select(l => l.Attribute("name").Value));
						}
					}

					if (classElem.Element("arcanespellfailure") != null)
					{
						sheet[$"spsSec{spellSectionCount}_SpellFailure"] =
							classElem.Element("arcanespellfailure").Attribute("text").Value;
					}
					else
					{
						sheet[$"spsSec{spellSectionCount}_SpellFailure"] = "-";
					}

					spellSectionCount++;
				}
				sheet["ctlSpellSectionCount"] = spellSectionCount.ToString();
			}

			{
				var itemMap = new[]
				{
					new {Name = "Belt", Index = 1},
					new {Name = "Neck", Index = 2},
					new {Name = "Body", Index = 3},
					new {Name = "Ring", Index = 4},
					new {Name = "Chest", Index = 5},
					new {Name = "Ring", Index = 6},
					new {Name = "Eyes", Index = 7},
					new {Name = "Shoulders", Index = 8},
					new {Name = "Feet", Index = 9},
					new {Name = "Wrist", Index = 10},
					new {Name = "Hands", Index = 11},
					new {Name = (string) null, Index = 12},
					new {Name = "Head", Index = 13},
					new {Name = (string) null, Index = 14},
					new {Name = "Headband", Index = 15},
					new {Name = (string) null, Index = 16},
					new {Name = "Armor", Index = 17},
					new {Name = "Shield", Index = 18}
				}.ToList();

				var extraCount = 0;
				foreach (XElement item in elem.Element("magicitems").Elements("item"))
				{
					string slotName = item.Element("itemslot")?.Value;
					string itemName = item.Attribute("name").Value;

					// The sheild slot is sort of weird, in that it's not a "slot" as HL understands it
					// Assume any "armor" things not in the "Armor" slot are shields
					if (slotName == null &&
						elem.Element("defenses")
							.Elements("armor")
							.Any(
								ar => string.Equals(
									ar.Attribute("name").Value,
									itemName,
									StringComparison.OrdinalIgnoreCase)))
					{
						slotName = "Shield";
					}

					int slotIndex = itemMap.FindIndex(s => s.Name == slotName);
					if (slotIndex == -1)
					{
						extraCount++;
						sheet[$"misc_magic_item{extraCount}"] = itemName;
					}
					else
					{
						var slot = itemMap[slotIndex];
						itemMap.RemoveAt(slotIndex);
						sheet[$"protitem{slot.Index}"] = itemName;
					}
				}
				sheet["ctlMagicItemCount"] = extraCount.ToString();
			}

			{
				var gearCount = 0;
				foreach (XElement gear in elem.Element("gear").Elements("item"))
				{
					gearCount++;
					if ((gear.Attribute("quantity")?.Value ?? "1") != "1")
					{
						sheet[$"item{gearCount}"] =
							$"{gear.Attribute("name").Value} ({gear.Attribute("quantity").Value})";
					}
					else
					{
						sheet[$"item{gearCount}"] = gear.Attribute("name").Value;
					}
					sheet[$"item{gearCount}_weight"] = gear.Element("weight").Attribute("value").Value;
				}
				sheet["ctlInventoryCount"] = gearCount.ToString();
			}

			SourceCharacter = ToSummary(_sourceCharacterObject);
		}

		private async Task LoadInformation()
		{
			OAuthSession newSession = CreateOAuthSession();

			await FillCampaigns(newSession);

			CharacterInfo foundCurrentCharacter = null;

			foreach (CampaignInfo campaign in AllCampaigns)
			{
				HttpWebRequest request =
					newSession.Request()
						.ForUrl($"http://api.obsidianportal.com/v1/campaigns/{campaign.Id}/characters.json")
						.Get()
						.ToWebRequest();
				JArray charactersObject;
				using (
					WebResponse response = await Task.Factory.FromAsync(
						request.BeginGetResponse,
						request.EndGetResponse,
						null))
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				using (var jsonReader = new JsonTextReader(streamReader))
					charactersObject = JArray.Load(jsonReader);

				campaign.Characters = new ObservableCollection<CharacterInfo>(
					charactersObject.Select(
						c => new CharacterInfo
						{
							Id = (string)c["id"],
							Name = (string)c["name"],
							Player = (string)c["author"]["username"],
							CampaignId = campaign.Id
						}));

				campaign.Characters.Add(
					new CharacterInfo { Id = null, CampaignId = campaign.Id, Name = "<New>", Player = "<New>" });

				foundCurrentCharacter = foundCurrentCharacter ??
					campaign.Characters.FirstOrDefault(c => c.Id == _settings.LastCharacterId);
			}

			CurrentCharacter = foundCurrentCharacter;
		}

		private OAuthSession CreateOAuthSession()
		{
			var newSession = new OAuthSession(
				new OAuthConsumerContext
				{
					ConsumerKey = _settings.OauthConsumerKey,
					ConsumerSecret = _settings.OauthConsumerSecret,
					SignatureMethod = SignatureMethod.HmacSha1
				}
				,
				"https://www.obsidianportal.com/oauth/request_token",
				"https://www.obsidianportal.com/oauth/authorize",
				"https://www.obsidianportal.com/oauth/access_token")
			{
				AccessToken = new TokenBase
				{
					Token = _settings.OauthToken,
					TokenSecret = _settings.OauthTokenSecret
				}
			};
			return newSession;
		}

		private async Task FillCampaigns(OAuthSession newSession)
		{
			HttpWebRequest request =
				newSession.Request().Get().ForUrl("https://api.obsidianportal.com/v1/users/me.json").ToWebRequest();

			JObject meObject;
			using (
				WebResponse response = await Task.Factory.FromAsync(
					request.BeginGetResponse,
					request.EndGetResponse,
					null))
			using (var streamReader = new StreamReader(response.GetResponseStream()))
			using (var jsonReader = new JsonTextReader(streamReader))
				meObject = JObject.Load(jsonReader);
			var campaigns = (JArray)meObject["campaigns"];
			_settings.UserName = (string)meObject["username"];
			UserName = _settings.UserName;
			_settings.Save();

			AllCampaigns = new ObservableCollection<CampaignInfo>(
				campaigns.Select(
					c => new CampaignInfo
					{
						Name = (string)c["name"],
						Id = (string)c["id"]
					}));

			CurrentCampaign = AllCampaigns.FirstOrDefault(c => c.Id == _settings.LastCampaignId);
		}

		private static void OnCampaignChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var that = d as MainWindow;
			if (that == null)
			{
				return;
			}
			var newCampaign = e.NewValue as CampaignInfo;
			that._settings.LastCampaignId = newCampaign?.Id;
			that._settings.LastCampaignName = newCampaign?.Name;
			that._settings.Save();
		}

		private static void OnCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var that = d as MainWindow;
			if (that == null)
			{
				return;
			}
			var newCharacter = e.NewValue as CharacterInfo;

			that._settings.LastCharacterName = newCharacter?.Name;
			that._settings.LastCharacterId = newCharacter?.Id;
			that._settings.Save();

			that.TargetCharacter = null;

			that.LoadCharacter(newCharacter);
		}

		private async void LoadCharacter(CharacterInfo character)
		{
			if (character == null)
			{
				return;
			}

			if (character.Id == null)
			{
				TargetCharacter = new CharacterSummary { Name = "<New Character>" };
				return;
			}

			OAuthSession newSession = CreateOAuthSession();
			HttpWebRequest request = newSession.Request()
				.ForUrl(
					$"https://api.obsidianportal.com/v1/campaigns/{character.CampaignId}/characters/{character.Id}.json")
				.Get().
				ToWebRequest();

			JObject charObject;
			using (
				WebResponse response = await Task.Factory.FromAsync(
					request.BeginGetResponse,
					request.EndGetResponse,
					null))
			using (var streamReader = new StreamReader(response.GetResponseStream()))
			using (var jsonReader = new JsonTextReader(streamReader))
				charObject = JObject.Load(jsonReader);

			TargetCharacter = ToSummary(charObject);
		}

		private static CharacterSummary ToSummary(JObject charObject)
		{
			JToken jToken = charObject["dynamic_sheet"];
			if (jToken.Type != JTokenType.Object)
			{
				return null;
			}

			var sheet = (JObject)jToken;

			var characterSummary = new CharacterSummary
			{
				Name = (string)charObject["name"],
				Alignment = (string)sheet["alignment"],
				ClassLevels = (string)sheet["level"],
				Gender = (string)sheet["gender"],
				Race = (string)sheet["race"]
			};
			return characterSummary;
		}

		private async void Login(object sender, RoutedEventArgs e)
		{
			var ctx = new OAuthConsumerContext
			{
				ConsumerKey = _settings.OauthConsumerKey,
				ConsumerSecret = _settings.OauthConsumerSecret,
				SignatureMethod = SignatureMethod.HmacSha1
			};

			var session = new OAuthSession(
				ctx,
				"https://www.obsidianportal.com/oauth/request_token",
				"https://www.obsidianportal.com/oauth/authorize",
				"https://www.obsidianportal.com/oauth/access_token");

			IToken token = session.GetRequestToken();

			string authLink = session.GetUserAuthorizationUrlForToken(token, "oob");
			Task<IToken> getTokenTask = CallbackAsyncThread.RunAsync(
				() =>
				{
					string verifier = null;
					using (var browser = new IE(authLink) { AutoClose = true })
					{
						while (
							!string.Equals(
								browser.Url,
								"https://www.obsidianportal.com/oauth/authorize",
								StringComparison.OrdinalIgnoreCase))
						{
							NameValueCollection queryString = HttpUtility.ParseQueryString(browser.Uri.Query);
							if (queryString["oauth_verifier"] != null)
							{
								verifier = queryString["oauth_verifier"];
								break;
							}
							Thread.Sleep(500);
						}

						if (verifier == null)
						{
							Element element = browser.Element("oauth-verifier");
							if (!element.Exists)
							{
								return null;
							}
							verifier = element.Text;
						}
					}

					return session.ExchangeRequestTokenForAccessToken(token, verifier);
				},
				ApartmentState.STA);

			IToken accessToken;
			try
			{
				accessToken = await getTokenTask;
			}
			catch (Exception)
			{
				MessageBox.Show(this, "Failed to log in", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			_settings.OauthToken = accessToken.Token;
			_settings.OauthTokenSecret = accessToken.TokenSecret;
			_settings.Save();

			await LoadInformation();
		}

		private async void UploadCharacter(object sender, RoutedEventArgs e)
		{
			CharacterInfo character = CurrentCharacter;

			if (character == null)
			{
				return;
			}

			if (TargetCharacter == null)
			{
				return;
			}

			OAuthSession newSession = CreateOAuthSession();
			newSession.ConsumerContext.UseHeaderForOAuthParameters = true;

			HttpWebRequest request;

			if (character.Id != null)
			{
				request = newSession.Request()
					.ForUrl(
						$"https://api.obsidianportal.com/v1/campaigns/{character.CampaignId}/characters/{character.Id}.json")
					.Put()
					.ToWebRequest();
			}
			else
			{
				request = newSession.Request()
					.ForUrl(
						$"https://api.obsidianportal.com/v1/campaigns/{character.CampaignId}/characters.json")
					.Post()
					.ToWebRequest();
			}

			JObject uploadingCharacter = (JObject)_sourceCharacterObject.DeepClone();
			uploadingCharacter["is_player_character"] = true;
			uploadingCharacter["is_game_master_only"] = false;

			using (
				Stream requestStream = await Task.Factory.FromAsync(
					request.BeginGetRequestStream,
					request.EndGetRequestStream,
					null))
			using (var textWriter = new StreamWriter(requestStream))
			using (var jsonWriter = new JsonTextWriter(textWriter))
			{
				var wrapped = new JObject(new JProperty("character", uploadingCharacter));
				await wrapped.WriteToAsync(jsonWriter);
			}

			try
			{
				using (
					var response =
						(HttpWebResponse)
						await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null))
				{
					if (response.StatusCode == HttpStatusCode.OK)
					{
						using (var streamReader = new StreamReader(response.GetResponseStream()))
						using (var jsonReader = new JsonTextReader(streamReader))
						{
							JObject charObj = JObject.Load(jsonReader);
							var charInfo = new CharacterInfo
							{
								Id = (string)charObj["id"],
								Name = (string)charObj["name"],
								Player = (string)charObj["author"]["username"],
								CampaignId = character.CampaignId,
							};

							if (CurrentCampaign.Characters.Any(c => c.Id == charInfo.Id))
							{
								CurrentCampaign.Characters.Insert(CurrentCampaign.Characters.Count - 1, charInfo);
								CurrentCharacter = charInfo;
							}

							TargetCharacter = ToSummary(charObj);
						}
						MessageBox.Show(this, "Character upload complete", "Complete");
					}
					else
					{
						MessageBox.Show(
							this,
							"Character upload failed!",
							"Failed",
							MessageBoxButton.OK,
							MessageBoxImage.Error);
					}
				}
			}
			catch (Exception)
			{
				MessageBox.Show(
					this,
					"Character upload failed!",
					"Failed",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		private void OpenNewCharacter(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog
			{
				Title = "Load HeroLab XML",
				CheckFileExists = true,
				DefaultExt = ".xml",
				Filter = "All Files|*.*|HeroLab XML|*.xml",
				FilterIndex = 2,
				Multiselect = false
			};

			if (dlg.ShowDialog() == true)
			{
				LoadSourceCharacter(dlg.FileName);
			}
		}
	}
}

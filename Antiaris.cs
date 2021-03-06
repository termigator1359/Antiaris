﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Antiaris.NPCs.Town;
using Antiaris.VEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Antiaris.UIs;
using Terraria.UI;

namespace Antiaris
{
    public class Antiaris : Mod
    {
        public static Mod Thorium;
		public static Mod kRPG;
		public static Mod RockosARPG;
		public static Mod TerrariaOverhaul;
        public static Mod Unleveled;
        public static Texture2D cQuestTexture;
        public static Mod Instance;
        public static int coin;
        public static ModHotKey adventurerKey;
        public static ModHotKey hideTracker;
        internal QuestTrackerUI questTracker;
        private UserInterface questInterface;
        internal CurrentQuestUI cQuestUI;
        private UserInterface questLog;
        public static Texture2D trackerTexture;
        public static ModHotKey stand;

        private static float lifePerHeart = 20f;
        private Color rb = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

        public bool Tracker = true;

        public Antiaris()
        {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true,
                AutoloadBackgrounds = true
            };
        }

        public static string ConfigFileRelativePath {
			get { return "Mod Configs/Antiaris.json"; }
		}

        private Mod mod
        {
            get
            {
                return ModLoader.GetMod("Antiaris");
            }
        }

        public static void ReloadConfigFromFile() {
			// Define implementation to reload your mod's config data from file
		}

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte msgType = reader.ReadByte();
            switch (msgType)
            {
                // id 1 = transform
                case 1:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        return;
                    int transformedNPC = reader.ReadInt32();
                    Main.npc[transformedNPC].Transform(mod.NPCType("BrokenMirror"));
                    break;
                case 2:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        return;
                    int transformedNPC2 = reader.ReadInt32();
                    Main.npc[transformedNPC2].Transform(mod.NPCType("Adventurer"));
                    break;
                case 3:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        return;
					int player = reader.ReadInt32();
                    int currentQuest = reader.ReadInt32();
                    Main.player[player].GetModPlayer<QuestSystem>(mod).CurrentQuest = currentQuest;
                    break;
				case 4:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        return;
					int player2 = reader.ReadInt32();
                    int currentPirateQuest = reader.ReadInt32();
                    Main.player[player2].GetModPlayer<Pirate.PirateQuestSystem>(mod).CurrentPirateQuest = currentPirateQuest;
                    break;
            }
            if (Main.netMode != 2)
                return;
            NetMessage.SendData(7, -1, -1, (NetworkText)null, 0, 0.0f, 0.0f, 0.0f, 0, 0, 0);
        }

        public override void PostSetupContent()
        {
			Thorium = ModLoader.GetMod("ThoriumMod");
			kRPG = ModLoader.GetMod("kRPG");
			RockosARPG = ModLoader.GetMod("RockosARPG");
            kRPG = ModLoader.GetMod("kRPG");
            Unleveled = ModLoader.GetMod("Unleveled");
            TerrariaOverhaul = ModLoader.GetMod("TerrariaOverhaul");
            var bossChecklist = ModLoader.GetMod("BossChecklist");
            if (bossChecklist != null)
            {
				//SlimeKing = 1f;
				//EyeOfCthulhu = 2f;
				//EaterOfWorlds = 3f;
				//QueenBee = 4f;
				//Skeletron = 5f;
				//WallOfFlesh = 6f;
				//TheTwins = 7f;
				//TheDestroyer = 8f;
				//SkeletronPrime = 9f;
				//Plantera = 10f;
				//Golem = 11f;
				//DukeFishron = 12f;
				//LunaticCultist = 13f;
				//Moonlord = 14f;
                bossChecklist.Call("AddBossWithInfo", "Antlion Queen", 4.5f, (Func<bool>)(() => AntiarisWorld.DownedAntlionQueen), "Use a [i:" + ItemType("AntlionDoll") + "] in Desert after beating Queen Bee");
				bossChecklist.Call("AddBossWithInfo", "Tower Keeper", 6.2f, (Func<bool>)(() => AntiarisWorld.DownedTowerKeeper), "Break the Mirror in the Cursed Tower in Corruption or Crimson using [i:" + ItemType("StoneHammer1") +"] or [i:" + ItemType("StoneHammer2") +"]. Can be also summoned by using [i:" + ItemType("PocketCursedMirror") +"] or [i:" + ItemType("PocketCursedMirror2") +"] in Corruption or Crimson.");
            }
        }

        public override void Load()
        {
            ///UI codo
            #region Load UI
            cQuestTexture = ModLoader.GetTexture("Antiaris/Miscellaneous/NoteBackground");
            trackerTexture = ModLoader.GetTexture("Antiaris/Miscellaneous/QuestTracker");
            if (!Main.dedServ)
            {
                questTracker = new QuestTrackerUI();
                questTracker.Activate();
                questInterface = new UserInterface();
                questInterface.SetState(questTracker);

                cQuestUI = new CurrentQuestUI();
                cQuestUI.Activate();
                questLog = new UserInterface();
                questLog.SetState(cQuestUI);
            }
            #endregion
            ModExplorer._initialize();
            AntiarisGlowMasks.Load();
            adventurerKey = RegisterHotKey("Special Ability", "L");
			hideTracker = RegisterHotKey("Enable/Disable Quest Tracker", "Q");
            GameShaders.Armor.BindShader(ItemType("GooDye"), new ArmorShaderData(Main.PixelShaderRef, "ArmorSolar")).UseColor(0.1f, 0.3f, 0.2f).UseSecondaryColor(0.1f, 0.3f, 0.2f);
            coin = CustomCurrencyManager.RegisterCurrency(new CustomCoin(mod.ItemType("IronCoin"), 999L));
            if (!Main.dedServ)
            {
                LightningBolt.SegmentTexture = GetTexture("VEffects/LightningSegment");
                LightningBolt.EndTexture = GetTexture("VEffects/LightningEnd");
                Filters.Scene["Antiaris:AntlionQueen"] = new Filter(new Data("FilterMiniTower").UseColor(0.9f, 0.5f, 0.2f).UseOpacity(0.3f), EffectPriority.VeryHigh);
                SkyManager.Instance["Antiaris:AntlionQueen"] = new Sky();
				Filters.Scene["Antiaris:Corruption"] = new Filter(new Data("FilterMiniTower").UseColor(0.1f, 0.1f, 0.1f).UseOpacity(0.5f), EffectPriority.VeryHigh);
                SkyManager.Instance["Antiaris:Corruption"] = new Sky();
				Filters.Scene["Antiaris:TimeSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.6f, 0.6f), EffectPriority.VeryHigh);
                SkyManager.Instance["Antiaris:TimeSky"] = new TimeSky();
                Filters.Scene["Antiaris:TimeSky2"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(Main.DiscoR, Main.DiscoG, Main.DiscoB), EffectPriority.VeryHigh);
                SkyManager.Instance["Antiaris:TimeSky2"] = new TimeSky();
                AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/AntlionQueen"), ItemType("AntlionQueenMusicBox"), TileType("AntlionQueenMusicBox"));
				if(WorldGen.crimson)
				{
					AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/TowerKeeper"), ItemType("TowerKeeperMusicBox1"), TileType("TowerKeeperMusicBox1"));
				}
				else if (!WorldGen.crimson)
				{
					AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/TowerKeeper"), ItemType("TowerKeeperMusicBox2"), TileType("TowerKeeperMusicBox2"));
				}
				
            }
            #region Translations
            ModTranslation text = CreateTranslation("UnconsciousGuide");
            text.SetDefault("Ugh... My head hurts so much... Oh, hello, {0}! Someone bursted into my house and pretty much destroyed it... If you could repair it I'd live in it again! You should look into my chest, it has some things that can help you with progression. By the way, if you need some help, just talk to me!");
            text.AddTranslation(GameCulture.Chinese, "呃啊…我的头…哦，哈喽，{0}!之前有人来到这里洗劫了我的家，还把它给烧毁了...你能不能修复它？求你了，否则我将无家可归...你可以翻一下我家里的箱子，也许有对你有用的东西。顺带一提，有问题尽管找我！");
            text.AddTranslation(GameCulture.Russian, "Угх... Моя голова так болит... Оу, здравствуй, {0}! Кто-то ворвался в мой дом и почти разрушил его... Если бы ты смог его починить, я бы мог там снова жить! В моем сундуке есть немного вещей, которые помогут тебе с продвижением. Кстати, если тебе нужна помощь, просто поговори со мной!");
            AddTranslation(text);
			
            text = CreateTranslation("EnchantedSetBonus");
            text.SetDefault("17% reduced mana usage\nIncreases maximum mana by 20\nHitting an enemy may cause a dagger to pierce them");
            text.AddTranslation(GameCulture.Chinese, "1、减少 17% 的魔力消耗\n2、魔力最大值增加 20\n3、攻击敌人可能会有光之刃为你刺杀它们");
            text.AddTranslation(GameCulture.Russian, "Снижает использование маны на 17%\nУвеличивает максимальное количество маны на 20\nПри ударе по врагу может появиться клинок, который пронзит его");
			AddTranslation(text);

            text = CreateTranslation("DiscipleSetBonus");
            text.SetDefault("Increases maximum mana by 20\nEach third damage dealt creates a magical energy that restores mana");
            text.AddTranslation(GameCulture.Chinese, "1、增加 20 点魔力最大值\n2、每第三次施予攻击将召唤能够恢复魔力的魔法能量");
            text.AddTranslation(GameCulture.Russian, "Увеличивает максимальное количество маны на 20\nКаждое третье попадание создаст магическую энергию, восстанавливающую ману");
            AddTranslation(text);
            
            text = CreateTranslation("SorcererSetBonus");
            text.SetDefault("Increases maximum mana by 30\n10% reduced mana usage\nWhen the player is moving, he receives mana every 2 seconds");
            text.AddTranslation(GameCulture.Chinese, "1、增加 30 点魔力最大值\n2、减少 10% 魔力消耗\\n3、当玩家移动时，每 2 秒将接收魔力");
            text.AddTranslation(GameCulture.Russian, "Увеличивает максимальное количество маны на 30\nСнижает использование маны на 10%\nКогда игрок двигается, он восстанавливает ману каждые 2 секунды");
            AddTranslation(text);
			
			text = CreateTranslation("GooSetBonus");
            text.SetDefault("Grants 5 defense if there are slimes nearby");
            text.AddTranslation(GameCulture.Chinese, "如果附近有史莱姆将增加 5 防御力");
            text.AddTranslation(GameCulture.Russian, "Дает 5 защиты, если рядом есть слизни");
			AddTranslation(text);
			
			text = CreateTranslation("AntlionSetBonus");
            text.SetDefault("Periodically summons friendly Antlion Swarmers (Maximum amount is 3)\nEach Swarmer increases minion damage by 10%, grants 2 defense and decreases damage taken by 5%\nSwarmers disappear after sometime but then spawn again");
            text.AddTranslation(GameCulture.Chinese, "定期召唤蚁狮蜂（最大数量为 3 ）\n每个蚁狮蜂会为你增加 10% 召唤伤害，2 防御力且减少 5% 所承受的伤害伤害\n蚁狮蜂会在一定时间后消失，然后再次被召唤");
            text.AddTranslation(GameCulture.Russian, "Периодически призывает дружественных Взрослых муравьиных львов (Максимальное количество - 3)\nКаждый Муравьиный лев увеличивает урон миньонов на 10%, даёт 2 защиты и снижает получаемый урон на 5%\nМуравьиные львы исчезают через некоторое время, но затем появляются снова");
			AddTranslation(text);
			
			text = CreateTranslation("NecromancerSetBonus");
            text.SetDefault("Minions have 33% chance restore player's health in amount of 20% of damage dealt to enemy");
            text.AddTranslation(GameCulture.Chinese, "召唤物有 33% 的概率为玩家回复召唤物对敌人造成的 20% 的伤害数值的生命值");
            text.AddTranslation(GameCulture.Russian, "Миньоны имеют 33% шанс восстановить здоровье игроку в размере 20% от нанесенного урона врагу");
			AddTranslation(text);
			
			text = CreateTranslation("LeafRoll");
            text.SetDefault("Uses: ");
            text.AddTranslation(GameCulture.Chinese, "使用次数：");
            text.AddTranslation(GameCulture.Russian, "Использований: ");
			AddTranslation(text);

            text = CreateTranslation("Note1");
            text.SetDefault("   If you don't give us\n  the information we need,\n   we will slaughter you.\n Don't pretend you don't\n     know where the thing\n we look for is.");
            text.AddTranslation(GameCulture.Chinese, "      如果你不提供\n   我们所需要的证据，\n   我们会宰了你。\n      不要假装你不知道那些，\n   我们在暗中观察你。");
            text.AddTranslation(GameCulture.Russian, "Если ты не дашь нам ин-\n     формацию, которую\nумалчиваешь, мы тебя\nуничтожим. Не притво-\n  ряйся, что не знаешь,\n   где то, что мы ищем.");
			AddTranslation(text);

            text = CreateTranslation("PirateHelp1");
            text.SetDefault("Help me, stranger!");
            text.AddTranslation(GameCulture.Chinese, "能不能来救救我？");
            text.AddTranslation(GameCulture.Russian, "Помоги мне, незнакомец!");
			AddTranslation(text);
			
			text = CreateTranslation("PirateHelp1F");
			text.SetDefault("Help me, stranger!");
			text.AddTranslation(GameCulture.Chinese, "能不能来救救我？");
			text.AddTranslation(GameCulture.Russian, "Помоги мне, незнакомка!");
			AddTranslation(text);
			
			text = CreateTranslation("PirateHelp2");
            text.SetDefault("Help me and I will reward you!");
            text.AddTranslation(GameCulture.Chinese, "救救我！我会回报你的!");
            text.AddTranslation(GameCulture.Russian, "Помоги мне и я дам тебе награду!");
			AddTranslation(text);
			
			text = CreateTranslation("PirateHelp3");
            text.SetDefault("You're not going to leave an old pirate, right?");
            text.AddTranslation(GameCulture.Chinese, "你不会丢下个老爷子不管的，对吧？");
            text.AddTranslation(GameCulture.Russian, "Ты же не бросишь старого пирата, да?");
			AddTranslation(text);

            text = CreateTranslation("FrozenAdventurer1");
            text.SetDefault("Looks like this man was snowed in. He's unconscious but he can be helped by digging him out. A shovel I've seen near the door would be useful...");
            text.AddTranslation(GameCulture.Chinese, "看来这个人被雪埋住并昏迷了，但是仍然可以铲掉他身上的雪救他，门旁边的铁锹也许有点用处...");
            text.AddTranslation(GameCulture.Russian, "Похоже, что этого человека завалило снегом. Он без сознания, но ему можно помочь, выкопав его. Лопата, замеченная мною около двери, тут бы пригодилась...");
			AddTranslation(text);
			
			text = CreateTranslation("FrozenAdventurer2");
            text.SetDefault("Oh, thanks for saving me! That blizzard was so hard, so I decided to hide in this house. Unluckily, icicles have shattered the windows and... You've seen what happened. Couldn't get out without your help, thanks again!");
            text.AddTranslation(GameCulture.Chinese, "哦，谢谢你救了我！那场暴风雪太大了，所以我决定躲在这个房子里。不幸的是，冰锥打碎了窗户以及……我猜你已经看到发生了什么。没有你的帮助，我可能真的就死在这里了，再次感谢！");
            text.AddTranslation(GameCulture.Russian, "Ох, благодарю за спасение! Та буря была такой сильной, что я решил спрятаться в этом доме. К несчастью, сосульки разбили окна и... Вы видели, что произошло. Не смог выбраться без вашей помощи, вновь благодарю!");
			AddTranslation(text);

            text = CreateTranslation("Adventurer1");
            text.SetDefault("Somebody once told me that I will never become an great adventurer. And look at me - I've achieved a lot!");
            text.AddTranslation(GameCulture.Chinese, "有很多人曾经告诉我，我的理想就是一个笑话，不过你看看，对于理想，我现在已经接近了很多！");
            text.AddTranslation(GameCulture.Russian, "Однажды мне кто-то сказал, что я никогда не буду великим путешественником. И взгляни на меня - я достиг очень многого!");
			AddTranslation(text);

            text = CreateTranslation("Adventurer2");
            text.SetDefault("When you're going on an adventure, make sure to take some kind of knife with you to cut vines. One time I didn't take it and I had to tear vines with bare hands!");
            text.AddTranslation(GameCulture.Chinese, "当你去探险的时候，一定要拿一把匕首割开藤蔓，上次我忘记拿它了，只好用手撕开它...");
            text.AddTranslation(GameCulture.Russian, "Когда отправляешься в путешествие, убедись, что у тебя есть какой-то нож, чтобы резать лианы. Один раз я не взял его и мне пришлось рвать лианы голыми руками!");
			AddTranslation(text);

            text = CreateTranslation("Adventurer3");
            text.SetDefault("You know who am I? I am professor of archeology, expert on the occult, and how does one say it? Obtainer of rare antiquities.");
            text.AddTranslation(GameCulture.Chinese, "你知道我是谁吗？该怎么说呢…？我是一个挖掘珍贵文物的考古学家和神秘学家。");
            text.AddTranslation(GameCulture.Russian, "Ты знаешь, кто я? Я профессор археологии, эксперт по оккультным наукам и, как там говорят? Добыватель редких древних вещей.");
			AddTranslation(text);

            text = CreateTranslation("Adventurer4");
            text.SetDefault("I know that hornets can be pretty big but when I've entered the jungle and saw a hornet that was bigger then me... To put it mildly, I was shocked and never returned to the jungle again.");
            text.AddTranslation(GameCulture.Chinese, "我知道蜂类生物可能会很大，但是我进入丛林，发现了一堆比我自身还要巨大的马蜂…咳咳，说的好听一点，我很吃惊，从那以后我就再也没有回到丛林。");
            text.AddTranslation(GameCulture.Russian, "Я знаю, что шершни могут быть достаточно крупными, но когда я вошёл в джунгли и увидел шершня больше меня... Мягко говоря, я был шокирован и никогда больше не возвращался в джунгли");
			AddTranslation(text);

            text = CreateTranslation("Adventurer5");
            text.SetDefault("I can travel anywhere without a map! I never get lost. Wanna know how I'm doing this? I... Don't know.");
            text.AddTranslation(GameCulture.Chinese, "我可以没有地图在任何的地方旅行！而且从来不迷路！你想知道我是怎么做到的吗？我...不知道。");
            text.AddTranslation(GameCulture.Russian, "Я могу путешествовать где угодно без карты! Я даже никогда не теряюсь. Хочешь знать, как я это делаю? Я... Не знаю.");
			AddTranslation(text);

            text = CreateTranslation("Adventurer6");
            text.SetDefault("During my adventures I often was close to death, but you know what do we say to death? Not today.");
            text.AddTranslation(GameCulture.Chinese, "在我的冒险生涯里，我经常与死神擦肩而过，但你知道我们应该对死神说什么吗？不是今天。");
            text.AddTranslation(GameCulture.Russian, "Во время моих путешествий я часто был близок к смерти, но ты знаешь, что мы говорим смерти? Не сегодня.");
			AddTranslation(text);

			string anglerName = NPC.GetFirstNPCNameOrNull(NPCID.Angler);
			text = CreateTranslation("Adventurer7");
			text.SetDefault("Do you help this kid, " + anglerName + ", with his needs? You know, I need help more then him and I also provide pretty better rewards for you.");
            text.AddTranslation(GameCulture.Chinese, "你帮这个熊孩子，" + anglerName + "，满足需求吗？你懂得，我更需要你的帮助，我也会给你带来更好的回报。"); 
            text.AddTranslation(GameCulture.Russian, "Ты ведь помогаешь этому парнишке, " + anglerName + ", с тем, что ему нужно? Знаешь, мне помощь нужна больше, чем ему, а я ведь еще предоставляю награды получше.");
			AddTranslation(text);
			
            text = CreateTranslation("Adventurer8");
            text.SetDefault("Some people are afraid of night, since many monsters appear during it. These people say that night is dark and full of terrors. But I'm not afraid of it, I'm brave!");
            text.AddTranslation(GameCulture.Chinese, "这些人总是说夜晚是充满了恐惧和黑暗的，因为许多怪物都在这个时候出现。但是我有胆子，才不怕那些东西！");
            text.AddTranslation(GameCulture.Russian, "Некоторые люди боятся ночи, потому что в ночное время появляются много монстров. Эти люди говорят, что ночь темна и полна ужасов. Но я её не боюсь, я храбрый!");
			AddTranslation(text);

            text = CreateTranslation("Thanks");
            text.SetDefault("Thank you for your help! I really appreciate it!");
            text.AddTranslation(GameCulture.Chinese, "十分感谢，我真的感激不尽！");
            text.AddTranslation(GameCulture.Russian, "Спасибо за твою помощь! Я очень ценю это!");
			AddTranslation(text);

            text = CreateTranslation("NoQuest1");
            text.SetDefault("Come back later if you wanna help me again!");
            text.AddTranslation(GameCulture.Chinese, "如果你想再帮我一次以后再过来！");
            text.AddTranslation(GameCulture.Russian, "Приходи попозже, если хочешь снова помочь мне!");
			AddTranslation(text);

            text = CreateTranslation("NoQuest2");
            text.SetDefault("Right now I don't have any quests for you, come back later!");
            text.AddTranslation(GameCulture.Chinese, "现在我没什么事情拜托你，以后再来!");
            text.AddTranslation(GameCulture.Russian, "Сейчас у меня нет для тебя никаких заданий, приходи попозже!");
			AddTranslation(text);

            text = CreateTranslation("NoQuest3");
            text.SetDefault("I think I can give you another quest soon if you need it!");
            text.AddTranslation(GameCulture.Chinese, "我想如果你有需要的话，我以后可以给你另一个任务！");
            text.AddTranslation(GameCulture.Russian, "Я думаю, что смогу скоро дать тебе еще задание, если оно тебе нужно!");
			AddTranslation(text);

            text = CreateTranslation("Quest0");
            text.SetDefault("One day when our crew was crossing the sea, we got into a dreadful storm. We've lost a lot of people, our ship was damaged, but the most terrible loss was my compass. My favourite compass with which I had so many adventures now lies at the bottom of the ocean. Please, try fishing it out in the ocean, I really miss it!");
            text.AddTranslation(GameCulture.Chinese, "有一天，我们横渡大海时，遭遇了一场可怕的暴风雨。我们失去了很多的船员，而且我们的船也损坏了，但对我而言最可怕的是我的罗盘已沉入海底。我最喜欢的罗盘和这么多的奇遇现在静静的躺在海底，你能不能试着帮我找回来，哪怕是用一个鱼钩如同大海捞针那样，我真的很想念它！");
            text.AddTranslation(GameCulture.Russian, "Однажды, когда я со своей командой пересекал море, мы попали в ужасный шторм. Мы потеряли много людей, наш корабль был поврежден, но самой ужасной потерей был мой компас. Мой любимый компас, с которым у меня было столько приключений, теперь лежит на дне океана. Пожалуйста, попробуй выудить его в океане, мне он ужасно дорог!");
			AddTranslation(text);
			
			text = CreateTranslation("Name0");
			text.SetDefault("'Lost in the Sea'");
			text.AddTranslation(GameCulture.Chinese, "“迷失深海”");
			text.AddTranslation(GameCulture.Russian, "'Утерянный в океане'");
			AddTranslation(text);
			
            text = CreateTranslation("Quest1");
            text.SetDefault("My friend once told me about an interesting artifact. We were in Egypt when he told me about this artifact and it was very hot there. This artifact is a some kind of ice crystal that can cool things. I thought that it would be awesome if I could cool down by using it when it's too hot outside. I only know that different ice creatures contain this crystal inside of them. Can you please find it and bring it to me?");
            text.AddTranslation(GameCulture.Chinese, "我的朋友曾经告诉我一些有趣的东西。我们在埃及时，他告诉了我这个东西，这个东西是一种能冷却物体的冰晶，我想如果外面太热了的话我可以用它来降温避免中暑。我只知道不同的冰元素生物体内含有这种冰晶，你能找到它并把它捎给我吗？");
            text.AddTranslation(GameCulture.Russian, "Мой друг однажды рассказал мне об интересном артефакте. В тот день, когда он рассказал мне о нем, мы были в Египте, а там было очень жарко. Этот артефакт это какой-то кристалл, который может охлаждать вещи. Я подумал, что было бы круто, если бы я смог охлаждаться с его помощью, если на улице слишком жарко. Я только лишь знаю, что разные ледяные существа содержат этот кристалл в себе. Можешь ли ты, пожалуйста, найти его и принести мне?");
			AddTranslation(text);
			
			text = CreateTranslation("Name1");
			text.SetDefault("'Fighting the heat'");
			text.AddTranslation(GameCulture.Chinese, "“防暑");
			text.AddTranslation(GameCulture.Russian, "'Борьба с жарой'");
			AddTranslation(text);

            text = CreateTranslation("Quest2");
            text.SetDefault("My beautiful hat is a bit damaged! I really need to fix it but I need some leather for this. Can you bring me... Let's say... 12 leather, so I can fix it?");
            text.AddTranslation(GameCulture.Chinese, "你看看我漂亮的帽子，已经有好几个破损的地方了！我真的需要修理它，但我需要一些皮革，你能捎给我吗？打个比方...12块山猪皮，这样我可以修复它？");
			text.AddTranslation(GameCulture.Russian, "Моя красивая шляпа немного повреждена! Я очень хочу починить её, но мне нужно немного кожи, чтобы сделать это. Ты можешь принести мне... Скажем... 12 кожи, чтобы я смог починить её?");
			AddTranslation(text);
			
			text = CreateTranslation("Name2");
			text.SetDefault("'Save the hat!'");
			text.AddTranslation(GameCulture.Chinese, "“修理帽子”");
			text.AddTranslation(GameCulture.Russian, "'Спасите шляпу!'");
			AddTranslation(text);

            text = CreateTranslation("Quest3");
            text.SetDefault("I've heard legends about creatures that were living in deserts. Unfortunately, all of them died during a massive cataclysm. Bones of those creatures are probably lost in the sands. I really want to know more about those creatures but the only way to do it is to find at least a little bone of them. Maybe you can dig in the sand and find any remains of those creatures?");
            text.AddTranslation(GameCulture.Chinese, "我听说过关于生活在沙漠里的生物的传说。不幸的是，它们都在一场大灾变中丧生了。这些生物的骸骨应该是被沙漠埋葬了，我真的很想知道更多关于这些生物的事情。但目前唯一的办法就是找到至少一点的骸骨，也许你可以挖开那些沙子找到这些远古生物的骸骨？");
            text.AddTranslation(GameCulture.Russian, "Я слышал легенды о существах, живших в пустынях. К несчастью, они все погибли из-за огромного катаклизма. Кости тех существ, вероятнее, затеряны в песках. Я очень хочу узнать побольше о тех существах, но единственный способ сделать это, это найти хотя бы маленькую их кость. Может, ты покопаешься в песке и найдешь какие-нибудь останки тех существ?");
			AddTranslation(text);
			
			text = CreateTranslation("Name3");
			text.SetDefault("'Ancient legends'");
			text.AddTranslation(GameCulture.Chinese, "“远古传说”");
			text.AddTranslation(GameCulture.Russian, "'Древние легенды'");
			AddTranslation(text);

            text = CreateTranslation("Quest4");
            text.SetDefault("Have you ever tried an omelette made of harpy eggs? No? Me too, but I really want to try it. Maybe you can bring me a harpy egg so I can cook the omelette of it? I can share it with you!");
            text.AddTranslation(GameCulture.Chinese, "你有没有试过用鹰身女妖的蛋来煎蛋？没有？我也是，但我真的想知道那是什么味道的，也许你可以给我一个鹰身女妖的蛋，这样我可以做它的鸡蛋卷？而且我可以和你一起分享！");
			text.AddTranslation(GameCulture.Russian, "Ты когда-нибудь пробовал яичницу из яиц гарпий? Нет? Я тоже, но я очень хочу попробовать её. Может ты можешь принести мне яйцо гарпии, чтобы я приготовил эту яичницу? Я могу с тобой ей поделиться!");
			AddTranslation(text);
			
			text = CreateTranslation("Name4");
			text.SetDefault("'Tasty omelette'");
			text.AddTranslation(GameCulture.Chinese, "“美味煎蛋”");
			text.AddTranslation(GameCulture.Russian, "'Вкусная яичница'");
			AddTranslation(text);
			
			text = CreateTranslation("Quest5");
            text.SetDefault("I think I just had a brilliant idea... I'll need an apple that's covered in pure gold. But I can't just go and get that kind of thing in a shop! Can you help me out with this? Don't ask why I need the apple, you'll see later.");
            text.AddTranslation(GameCulture.Chinese, "我有一个有趣的主意...我需要一个纯金的苹果。但我不能在商店买到那种东西！你能帮我这个忙吗？不要问我为什么需要它，以后你会明白的。");
            text.AddTranslation(GameCulture.Russian, "Хм, кажется, у меня появилась гениальная идея... Мне понадобится яблоко, покрытое чистым золотом. Но ведь такое в магазине не купишь! Может, ты мне поможешь? Не спрашивай, зачем мне это яблоко, потом узнаешь.");
			AddTranslation(text);
			
			text = CreateTranslation("Name5");
			text.SetDefault("'Strange experiment'");
			text.AddTranslation(GameCulture.Chinese, "“古怪试验”");
			text.AddTranslation(GameCulture.Russian, "'Странный эксперимент'");
			AddTranslation(text);

            text = CreateTranslation("ThanksApple1");
            text.SetDefault("Yes! Yes! Finally! Behold my creation! This is a golden apple mask! A true masterpiece! Do you like it? Here you go, try it on. I poked holes in it so that you can eat and breathe. Oh, and one more thing: I melted the excess gold and turned it back into coins. But they are worthless compared to this wonderful mask.");
            text.AddTranslation(GameCulture.Chinese, "耶！就是这样！最后！看看我的杰作！这是一个金苹果面具！你喜欢吗？给你，试试看。我在里面戳了一个洞，这样你就可以吃和呼吸了。哦，还有一件事，我把多余的金子熔炼成硬币。但是和这个漂亮的面具比起来，它们毫无价值。");
            text.AddTranslation(GameCulture.Russian, "Да! Да! Наконец-то! Узри моё творение! Это маска золотого яблока! Это же шедевр! Тебе нравится? На, померяй. Я вырезал в ней дыры, чтобы ты мог есть и дышать. А, и еще одно: остатки золота я перековал обратно в монеты. Но по сравнению с этой великолепной маской, они ничего не стоят.");
			AddTranslation(text);
			
			text = CreateTranslation("Quest6");
            text.SetDefault("Do you know that skeletons from that dark dungeon can revive fallen allies? I really want to be able to revive dead creatures too! I've gathered some information and found out that they use Necronomicon to do this. Can you go to that spooky place and bring me that magical book?");
            text.AddTranslation(GameCulture.Chinese, "你知道黑暗地牢的那些骷髅可以复活死去的生物吗？我真的希望借此来复活死去的动物们！我收集了一些信息，它们使用一种被叫做“死灵之书”做这个。你能去那个鬼地方给我捎来这本神奇的书吗？");
            text.AddTranslation(GameCulture.Russian, "Ты знаешь, что скелеты из того темного подземелья могут воскрешать мертвых союзников? Я тоже очень хочу воскрешать мертвых существ! Я собрал немного информации и узнал, что они используют Некрономикон чтобы делать это. Ты можешь сходить в это страшное место и принести мне эту магическую книгу?");
			AddTranslation(text);
			
			text = CreateTranslation("Name6");
			text.SetDefault("'Dark Magic'");
			text.AddTranslation(GameCulture.Chinese, "“黑魔法”");
			text.AddTranslation(GameCulture.Russian, "'Тёмная магия'");
			AddTranslation(text);
			
			text = CreateTranslation("Quest7");
            text.SetDefault("I'm so tired of these slimes! Whenever I go outside, they immediately attack me! I really want to kill them all but I'm a bad warrior! Can you kill, let's say 25 slimes so I could go outside without any troubles?");
            text.AddTranslation(GameCulture.Chinese, "我真讨厌这些史莱姆！每当我出门时，它们总是立刻攻击我！我真的干死它们，但我却是个很差劲的战士...你能干掉，比方说25个史莱姆吗？让我们出门在外没有任何烦心事！");
            text.AddTranslation(GameCulture.Russian, "Я так устал от этих слизней! Каждый раз, когда я выхожу на улицу, они сразу же меня атакуют! Я очень хочу убить их всех, но из меня ужасный воин. Можешь ли ты убить, скажем, 25 слизней, чтобы я смог выйти на улицу без всяких проблем?");
			AddTranslation(text);
			
			text = CreateTranslation("Name7");
			text.SetDefault("'Annoying creatures'");
			text.AddTranslation(GameCulture.Chinese, "“恼人粘液”");
			text.AddTranslation(GameCulture.Russian, "'Надоедливые существа'");
			AddTranslation(text);
			
			text = CreateTranslation("Quest8");
            text.SetDefault("When we once lost our way in caverns, one of our group members told me about a skeleton with a gold hat. We have met one and it captured our mapmaker! Soon we found him dead, I must take revenge on that skeleton! Can you kill it?");
            text.AddTranslation(GameCulture.Chinese, "我们迷失在洞穴的时候，队伍里有一个人告诉我发现了一个戴着黄金矿工头盔的骷髅。我们遇见了那样的骷髅，但是它抓走了我们的制图师，然后很快我们就发现他已经死了。我必须要为死去的队友报仇，你能帮我杀了它吗？");
            text.AddTranslation(GameCulture.Russian, "Когда мы один раз заблудились в подземельях, один из участников нашей команды рассказал мне о скелете с золотой каской. Мы встретили такого, и он схватил нашего картографа! Вскоре мы нашли его мертвым, я должен отомстить тому скелету! Не мог бы ты уничтожить его?");
			AddTranslation(text);
			
			text = CreateTranslation("Name8");
			text.SetDefault("'Vengeance'");
			text.AddTranslation(GameCulture.Chinese, "“偿还血债”");
			text.AddTranslation(GameCulture.Russian, "'Отомщение'");
			AddTranslation(text);
			
			text = CreateTranslation("Quest9");
            text.SetDefault("Yesterday I got to a sky island using magic. Everything was peaceful at the beginning but then I got attacked by giant birds! I fell off the island, luckily I haven't broken any bones. Can you kill these birds so I will not get attacked next time?");
            text.AddTranslation(GameCulture.Chinese, "昨天我登上了空岛施法，就在我觉得一切安全时，一些蓝绿色巨禽用它的爪子抓住了我把我从空岛扔了下来，幸好我掉进了水里，没被摔惨。你能干一票它们吗？这样我去空岛也许不会再体验一次蹦极");
            text.AddTranslation(GameCulture.Russian, "Вчера я попал на летающий остров при помощи магии. Сначала всё было мирно, но потом меня атаковали огромные птицы! Я упал с острова, к счастью, я не сломал ни одной кости. Ты можешь убить этих птиц, чтобы в следующий раз на меня не напали?");
			AddTranslation(text);
			
			text = CreateTranslation("Name9");
			text.SetDefault("'Mutated birds'");
			text.AddTranslation(GameCulture.Chinese, "“突变猛禽”");
			text.AddTranslation(GameCulture.Russian, "'Мутировавшие птицы'");
			AddTranslation(text);

            text = CreateTranslation("Quest10");
            text.SetDefault("Let's toss a coin! The loser will bring something to the winner. Ugh... Our coins seems to not have heads and tails... Guess I've won! You have to bring me a silk scarf!");
            text.AddTranslation(GameCulture.Chinese, "我们投硬币吧！赌输了的要给赌赢了的一些东西。诶等等…我们的硬币好像没有正面和反面…我猜我赢了！你得给我一条丝绸围巾！");
            text.AddTranslation(GameCulture.Russian, "Давай подбросим монетку! Проигравший что-нибудь принесёт победителю. Блин, на наших монетах нет орла и решки... Пожалуй, я победил! Ты должен принести мне шёлковый шарфик!");
			AddTranslation(text);
			
			text = CreateTranslation("Name10");
			text.SetDefault("'Prize for the winner'");
			text.AddTranslation(GameCulture.Chinese, "“获胜奖品”");
			text.AddTranslation(GameCulture.Russian, "'Приз победившему'");
			AddTranslation(text);

            text = CreateTranslation("Quest11");
            text.SetDefault("The idea of going fishing during night turned out to be bad! These disgusting zombies snatched fishing rod from my hands and broke it! How am I supposed to fish without fishing rod!? Can you please gather the pieces and repair it?");
            text.AddTranslation(GameCulture.Chinese, "在晚上钓鱼真是个馊主意。那些恶心的僵尸从我的手中夺走并打碎了鱼竿。没鱼竿我怎么钓鱼？你能帮我找回它的残骸并修理吗？");
            text.AddTranslation(GameCulture.Russian, "Идея сходить порыбачить ночью оказалась ужасной! Эти отвратительные зомби вырвали удочку из моих рук и сломали! Как я должен рыбачить без удочки!? Можешь ли ты, пожалуйста, собрать части удочки и починить её?");
			AddTranslation(text);
			
			text = CreateTranslation("Name11");
			text.SetDefault("'The failed fishing'");
			text.AddTranslation(GameCulture.Chinese, "“摸鱼失败”");
			text.AddTranslation(GameCulture.Russian, "'Неудавшаяся рыбалка'");
			AddTranslation(text);

			text = CreateTranslation("Quest12");
            text.SetDefault("I just noticed that this gunslinger is selling something that looks like a blueprint of a weapon! Unfortunately, I don't have enough money to buy it and even if I had - I'm not that smart to understand all these schemes. Think you can buy that blueprint and craft the weapon for me?");
            text.AddTranslation(GameCulture.Chinese, "我只是注意到那个军火商看起来正在卖一个武器的蓝图，糟糕的是我根本没有钱能够买它，就算我有，我也难以理解蓝图所写的设计方案。我猜，你也许能给我买到那个蓝图并制作出武器？");
            text.AddTranslation(GameCulture.Russian, "Я только что заметил, что этот стрелок продаёт что-то похожее на чертёж оружия! К сожалению, у меня нет нужного количества денег, чтобы купить его, а даже если было - я не настолько умный, чтобы понять все эти схемы. Может ты сможешь купить чертёж и создать для меня это оружие?");
			AddTranslation(text);
			
			text = CreateTranslation("Name12");
			text.SetDefault("'Making a powerful weapon'");
			text.AddTranslation(GameCulture.Chinese, "“锻造重武”");
			text.AddTranslation(GameCulture.Russian, "'Создание мощного оружия'");
			AddTranslation(text);
			
			text = CreateTranslation("ThanksBonebardier");
            text.SetDefault("Ohh, would you look at this beauty! Glad you helped me out! ... Actually, I don't think I'll ever use this gun so guess you can take it as your reward.");
            text.AddTranslation(GameCulture.Chinese, "哇哦，这家伙看起来真棒！很高兴你帮了我！…其实，我并不知道如何使用这把枪，所以你可以把它当作你的奖励。");
            text.AddTranslation(GameCulture.Russian, "Охх, только взгляни на эту красоту! Рад, что ты помог мне! ... На самом деле, я не думаю, что я когда-нибудь буду использовать эту пушку, так что я думаю, что ты можешь взять её в качестве своей награды.");
			AddTranslation(text);
			
			text = CreateTranslation("Quest13");
            text.SetDefault("Making wings is a really hard process! You need a strong material so the wings won't tear apart when you're flying. I think pieces of demon wings should be suitable. Please, bring me 12 pieces so I can make good wings!");
            text.AddTranslation(GameCulture.Chinese, "制造翅膀的过程是非常困难的！你需要一个强大的材料制作它，这样在你飞行时翅膀才不会断裂。我想恶魔翅膀的碎片应该是合适的，请给我12块碎片让我做一个不错的翅膀！");
            text.AddTranslation(GameCulture.Russian, "Создание крыльев это тяжелый процесс! Нужно подобрать такой крепкий материал, чтобы крылья не порвались при полёте. Я думаю, что части крыльев демона подойдут. Пожалуйста, принеси мне 12 частей, чтобы я смог сделать хорошие крылья!");
			AddTranslation(text);
			
			text = CreateTranslation("Name13");
			text.SetDefault("'How to make wings'");
			text.AddTranslation(GameCulture.Chinese, "“想入飞飞”");
			text.AddTranslation(GameCulture.Russian, "'Как создать крылья'");
			AddTranslation(text);
			
			text = CreateTranslation("Quest14");
            text.SetDefault("Where's my chest!? Please, don't tell me that I've lost it... There were so many useful things in it! Wait a second... I didn't lose it! It was a shark who attacked my boat! Yeah, right, it ate my chest! Please, kill some sharks until you find the one who ate my chest and then bring the chest back to me!");
            text.AddTranslation(GameCulture.Chinese, "我的箱子在哪里？别告诉我我把它弄丢了！里面有这么多有用的东西…等等…我没有丢！是一条鲨鱼袭击了我的船后吞下了它。你能在海里杀掉一些鲨鱼，以找到那个吞下我的箱子的那条吗？然后把箱子还给我！");
            text.AddTranslation(GameCulture.Russian, "Где мой сундучок!? Пожалуйста, не говорите, что я потерял его... В нём было столько полезных вещей! Секундочку... Я не потерял его! Это всё акула, которая напала на мой корабль! Да, всё верно, именно она съела мой сундучок! Пожалуйста, убей немного акул, пока не найдешь ту, которая съела мой сундучок и принеси его мне обратно!");
			AddTranslation(text);
			
			text = CreateTranslation("Name14");
			text.SetDefault("'In a shark's stomach'");
			text.AddTranslation(GameCulture.Chinese, "“在鲨鱼的肚子里”");
			text.AddTranslation(GameCulture.Russian, "'В желудке акулы'");
			AddTranslation(text);
			
			text = CreateTranslation("ThanksChest");
            text.SetDefault("Oh, I can't find words to thank you! You really helped me out! Here, take this book from my chest. Somebody gave it to me long time ago and I don't think I'll use it.");
            text.AddTranslation(GameCulture.Chinese, "哇哦…我想我已经找不到能感谢你的话了！你真的帮了我个大忙！来，从我的箱子里把这本书拿走吧。很久以前有人给我的，我想我不会用它。");
            text.AddTranslation(GameCulture.Russian, "Ох, я не могу найти слов, чтобы отблагодарить тебя! Ты очень сильно помог мне! Вот, возьми эту книгу из сундучка. Кто-то дал её мне давным-давно, и я не думаю, что буду её использовать.");
			AddTranslation(text);	
			
			text = CreateTranslation("Quest15");
            text.SetDefault("I have tried different food during my life but I've never eaten a coconut! It may sound oddly but it's true. I know that there're some palms growing near the ocean and I've even seen coconuts on them! The problem is that I'm too short to get them. Please, bring me 16 coconuts and then I can find out if coconuts are that tasty as many people say!");
            text.AddTranslation(GameCulture.Chinese, "我一生中品尝过诸多食物，但我仍然没有吃过椰子！听起来很奇怪，但这是真的。我知道海边生长着一些棕榈树，然后看到上面长了很多的椰子！问题是我太矮了，而且我没有斧子所以拿不到。能给我16个椰子吗？我想知道椰子是不是像许多人说的那样美味！");
            text.AddTranslation(GameCulture.Russian, "В течение своей жизни я пробовал разную еду, но я никогда не ел кокос! Это звучит странно, но это так. Я знаю, что рядом с океаном растёт несколько пальм и я даже видел на них кокосы! Проблема в том, что я не настолько высокий чтобы достать их. Пожалуйста, принеси мне 16 кокосов, чтобы я смог понять, действительно ли они такие вкусные, как многие говорят!");
			AddTranslation(text);
			
			text = CreateTranslation("Name15");
			text.SetDefault("'Delicious food'");
			text.AddTranslation(GameCulture.Chinese, "“美味佳肴”");
			text.AddTranslation(GameCulture.Russian, "'Изысканная еда'");
			AddTranslation(text);			

			text = CreateTranslation("Quest16");
            text.SetDefault("I'm currently trying to make a potion that will allow one to climb on walls. A potion like this would be very useful for my adventures! The problem is that I need some spider samples to make it and I am... Afraid of spiders. Can you gather 12 spider masses for me? Just go to a spider nest, kill some baby creepers and then gather the mass.");
            text.AddTranslation(GameCulture.Chinese, "我正在尝试制作一种可以让人进行攀爬的药水。这样的药水对我的冒险而言非常有用！问题是我需要一些蜘蛛样本来制作，但是…我怕蜘蛛…你能帮我收集12个蜘蛛分泌物吗？只需要去蜘蛛洞杀掉一些爬行者幼体来采集分泌物。");
            text.AddTranslation(GameCulture.Russian, "Сейчас я пытаюсь сделать зелье, которое позволит ползать по стенам. Такое зелье было бы очень полезным для моих приключений! Но проблема в том, что мне нужно немного образцов пауков, чтобы сделать его, а я... Боюсь пауков. Можешь ли ты собрать 12 паучих масс для меня? Просто иди в гнездо пауков, убей немного маленьких паучков и затем собери массу.");
			AddTranslation(text);
			
			text = CreateTranslation("Name16");
			text.SetDefault("'Arachnophobia'");
			text.AddTranslation(GameCulture.Chinese, "“蜘蛛恐惧症”");
			text.AddTranslation(GameCulture.Russian, "'Арахнофобия'");
			AddTranslation(text);

			text = CreateTranslation("Quest17");
            text.SetDefault("I'm really-really upset right now! Wanna know what happened? I was making a presents for my friends. When I've made 20 of them, a monster whose name is Krampus came and stole the presents! I really don't want my friends to be left without presents from me this year! Please, find that monster and bring my presents back!");
            text.AddTranslation(GameCulture.Chinese, "我现在真的很难过！想知道发生了什么事了吗？我在给朋友做礼物，当我做了第20件时，一个叫 Krampus 的怪物把礼物全偷走了！我真的不想让我的朋友今年不给我送礼物！请找到那个怪物，把我的礼物夺回来！");
            text.AddTranslation(GameCulture.Russian, "Я очень-очень расстроен! Хочешь знать, что произошло? Я делал подарки для моих друзей. Когда я сделал 20, монстр, чьё имя Крампус, пришёл и украл подарки! Я очень не хочу, чтоб мои друзья остались без подарков от меня в этом году! Пожалуйста, найди этого монстра и верни мои подарки!");
			AddTranslation(text);		
			
			text = CreateTranslation("Name17");
			text.SetDefault("'Stolen Christmas'");
			text.AddTranslation(GameCulture.Chinese, "被盗的圣诞节");
			text.AddTranslation(GameCulture.Russian, "'Украденное Рожедство'");
			AddTranslation(text);

			text = CreateTranslation("Quest18");
            text.SetDefault("There're rumors about very strange slimes living deep in the caves. You probably wonder why strange, right? That's because those chunks of gel eat emeralds! That's why they're covered with emerald shards like with a shell. Bring me some of these shards and I'll create something. Now go!");
            text.AddTranslation(GameCulture.Chinese, "有传言说在地下深处生存着非常古怪的史莱姆。你肯定想知道它为什么古怪，对吧？那是因为这些凝胶居然吃翡翠！这就是为什么它们被像是翡翠的东西包裹住，给我点它们的碎片，我会做点有趣的东西，出发吧！");
            text.AddTranslation(GameCulture.Russian, "Ходят слухи об очень странных слизнях, живущих глубоко в пещерах. Ты наверное думаешь, почему о странных, да? Всё потому что эти куски геля едят изумруды! Именно поэтому они покрыты изумрудными осколками, как будто панцирем. Принеси мне немного этих осколков и я кое-что сделаю. Иди же!");
			AddTranslation(text);

			text = CreateTranslation("Name18");
			text.SetDefault("'Slimes that eat emeralds'");
			text.AddTranslation(GameCulture.Chinese, "“吃翡翠的史莱姆”");
			text.AddTranslation(GameCulture.Russian, "'Слизни, что едят изумруды'");
			AddTranslation(text);	

			text = CreateTranslation("Quest19");
            text.SetDefault("Did you know that if you cut down burnt trees you will get charcoal? I bet you did not. Well, since now you have this information, can you please get me 25 charcoal? I really need it to make some torches!");
            text.AddTranslation(GameCulture.Chinese, "你知道如果砍伐烧焦的树木会得到木炭吗？ 我赌五毛你肯定不知道。 那么，既然你了解到了这些，能给我25个木炭吗？ 我真的需要它来制作一些火把！");
            text.AddTranslation(GameCulture.Russian, "А ты знал, что если срубить сгоревшие дерева, то получишь древесный уголь? Спорю, что не знал. Что же, раз теперь ты владеешь такой информацией, можешь ли ты принести мне 25 древесного угля? Он очень нужен мне для создания факелов!");
			AddTranslation(text);

			text = CreateTranslation("Name19");
			text.SetDefault("'Hot to the touch'");
			text.AddTranslation(GameCulture.Chinese, "“触手可及”");
			text.AddTranslation(GameCulture.Russian, "'Горячий на ощупь");
			AddTranslation(text);			

			text = CreateTranslation("ThanksShards");
            text.SetDefault("You've managed to get them? Amazing! These shards look like they're alive, they're probably got affected by those slimes impact. If I'll try using a magic enchantment on them, you will get a new familiar... It worked! Take it, it will light up your way wherever you go!");
            text.AddTranslation(GameCulture.Chinese, "你设法弄到了？太棒了！这些碎片看起来仍然活着，我断定是因为受到史莱姆的影响，如果我试着用魔法附魔，你会得到一个新的…它能用了！拿着它，无论到哪里，它都可以照亮你前进的路！");
            text.AddTranslation(GameCulture.Russian, "Ты добыл его? Невероятно! Эти осколки выглядят как живые, наверное, на них сказалось необычное взаимодействие от тех слизней. Если я попробую использовать на них кое-какое магическое зачарование, то у тебя появится новый питомец... Получилось! Держи, он будеть освещать тебе путь, куда бы ты не направился!");
			AddTranslation(text);			

            text = CreateTranslation("PirateChat");
            text.SetDefault("Yarr, I really need ya help to leave this appalin' place!");
            text.AddTranslation(GameCulture.Chinese, "啊！我真的需要你的帮助来离开这个鬼地方!");
            text.AddTranslation(GameCulture.Russian, "Йаррр, мне очень нужна твоя помощь чтобы покинуть это ужасное место!");
			AddTranslation(text);
			
			text = CreateTranslation("PirateQuest");
            text.SetDefault("Yarr! I need your help... I got robbed by dirty mongrels and they took away my magical gimmick, moreover they haven't left a drop of rum aboard. These mongrels were talking about three beasts, they want to break my amulet and feed them it's parts! Of course I'm a sailor but I also want visit the land but I can't do it without my amulet. Get this amulet and kill them all for me!");
            text.AddTranslation(GameCulture.Chinese, "啊！我需要你的帮助...我被三个大怪物抢劫了，它们抢走了我神奇的道具，而且没留下一滴朗姆酒。它们甚至打碎我的护身符，抢走了其它的碎片妄图充当食物！我是个水手，但是我也想游览陆地，不过我不能没有我的护身符。去帮我拿回护身符...等等，它们其中有一个是眼球，一个是一大块粘液，还有一个我想不起来了，总之，把这些怪物都宰了！");
            text.AddTranslation(GameCulture.Russian, "Йарр! Мне нужна твоя помощь... Меня обокрали гадкие черти и унесли мою магическую диковинку, да еще и ни капли рома на борту не оставили. Эти гады болтали о каких-то трех тварях, они хотят разбить мой амулет и скормить им его части! Я конечно моряк, но и на суше побывать охота, а без моего амулета я этого сделать не могу. Достань этот амулет и прикончи всех за меня!");
			AddTranslation(text);
			
			text = CreateTranslation("PirateThanks");
            text.SetDefault("Yarr, thankee so much! At last I can leave this world! Maybe we will meet again! Take this as my reward!");
            text.AddTranslation(GameCulture.Chinese, "啊！十分感谢！我终于可以离开这里了！也许我们以后会再见面，把这个当做我的报答！");
            text.AddTranslation(GameCulture.Russian, "Йаррр, спасибо тебе! Наконец-то я могу покинуть этот мир! Быть может, мы встретимся вновь! Возьми это в качестве награды!");
			AddTranslation(text);
			
			text = CreateTranslation("BoundPirate");
            text.SetDefault("Thank ya for rescuin' me!");
            text.AddTranslation(GameCulture.Chinese, "谢谢你救了我！");
            text.AddTranslation(GameCulture.Russian, "Спасибо тебе за то, что спас меня!");
			AddTranslation(text);
			
			text = CreateTranslation("PirateCompleted");
            text.SetDefault("Ya already helped me and I appreciate it!");
            text.AddTranslation(GameCulture.Chinese, "你已经帮助了我，我感激不尽！");
            text.AddTranslation(GameCulture.Russian, "Ты уже помог мне и я ценю это!");
			AddTranslation(text);
			
			text = CreateTranslation("AmuletDeath");
            text.SetDefault("{0} was torn to pieces by dark forces...");
            text.AddTranslation(GameCulture.Chinese, "{0} 被黑暗力量撕成碎片...");
            text.AddTranslation(GameCulture.Russian, "{0} был разорван куски тёмными силами...");
			AddTranslation(text);
			
			text = CreateTranslation("AmuletDeathF");
            text.SetDefault("{0} was torn to pieces by dark forces...");
            text.AddTranslation(GameCulture.Chinese, "{0} 被黑暗力量撕成碎片...");
            text.AddTranslation(GameCulture.Russian, "{0} была разорвана куски тёмными силами...");
			AddTranslation(text);
			
            text = CreateTranslation("PirateBoatGen");
            text.SetDefault("The Pirate is sailing to the world...");
            text.AddTranslation(GameCulture.Chinese, "船长正在向世界航行...");
            text.AddTranslation(GameCulture.Russian, "Пират приплывает в мир...");
			AddTranslation(text);
			
			text = CreateTranslation("GuideHouseGen");
            text.SetDefault("Someone is building a house for the Guide...");
            text.AddTranslation(GameCulture.Chinese, "有人正在为向导修建房屋...");
            text.AddTranslation(GameCulture.Russian, "Кто-то строит дом Гиду...");
			AddTranslation(text);
			
			text = CreateTranslation("PyramideGen");
            text.SetDefault("Creating pyramids in the sand...");
            text.AddTranslation(GameCulture.Chinese, "在沙漠上创建金字塔...");
            text.AddTranslation(GameCulture.Russian, "Идёт создание пирамид в песках...");
			AddTranslation(text);

            text = CreateTranslation("SubmarineGen");
            text.SetDefault("A submarine sinks in the ocean...");
            text.AddTranslation(GameCulture.Chinese, "潜水艇在深海沉没...");
            text.AddTranslation(GameCulture.Russian, "Субмарина тонет в океане...");
			AddTranslation(text);

            text = CreateTranslation("EnchantedGen");
            text.SetDefault("Enchanted stones are appearing in caves...");
            text.AddTranslation(GameCulture.Chinese, "附魔石在洞穴缓慢生长...");
            text.AddTranslation(GameCulture.Russian, "В пещерах появляются зачарованные камни...");
			AddTranslation(text);

            text = CreateTranslation("CrystalGen");
            text.SetDefault("Nature crystals are appearing on the grass...");
            text.AddTranslation(GameCulture.Chinese, "自然水晶在草木之中生长...");
            text.AddTranslation(GameCulture.Russian, "На траве появляются кристаллы природы...");
			AddTranslation(text);

            text = CreateTranslation("CampGen");
            text.SetDefault("Robbers are making their camp...");
            text.AddTranslation(GameCulture.Chinese, "土匪们正在建造营地...");
            text.AddTranslation(GameCulture.Russian, "Разбойники создают свой лагерь...");
			AddTranslation(text);
			
			text = CreateTranslation("TowerGen");
            text.SetDefault("A strange tower appears in the dangerous forests...");
            text.AddTranslation(GameCulture.Chinese, "诡异的石塔出现于危机四伏的森林...");
            text.AddTranslation(GameCulture.Russian, "Странная башня появляется в опасных лесах...");
			AddTranslation(text);
			
			text = CreateTranslation("FortressGen");
            text.SetDefault("Something ancient and forbidden appears underground...");
            text.AddTranslation(GameCulture.Chinese, "...");
            text.AddTranslation(GameCulture.Russian, "Что-то древнее и запретное появляется в подземельях...");
			AddTranslation(text);

            text = CreateTranslation("QuestKilled");
            text.SetDefault("\n \nKilled: ");
            text.AddTranslation(GameCulture.Chinese, "\n \n已击杀：");
            text.AddTranslation(GameCulture.Russian, "\n \nУбито: ");
			AddTranslation(text);
			
			text = CreateTranslation("QuestKilled2");
            text.SetDefault(" out of ");
            text.AddTranslation(GameCulture.Chinese, " 需要击杀：");
            text.AddTranslation(GameCulture.Russian, " из ");
			AddTranslation(text);

            text = CreateTranslation("IronCoin");
            text.SetDefault("iron coin");
            text.AddTranslation(GameCulture.Chinese, "铁币");
            text.AddTranslation(GameCulture.Russian, "мон. железа");
			AddTranslation(text);
			
			text = CreateTranslation("Mirror1");
            text.SetDefault("You look in the mirror. You see your reflection but there's also something moving behind you.");
            text.AddTranslation(GameCulture.Chinese, "你凝视着镜子，看着另一个自己。但是“自己”的身后似乎有什么东西。");
            text.AddTranslation(GameCulture.Russian, "Вы смотрите в зеркало. Вы видите своё отражение, но ещё позади вас что-то движется.");
			AddTranslation(text);
			
			text = CreateTranslation("Mirror3");
            text.SetDefault("The mirror seems to be broken.");
            text.AddTranslation(GameCulture.Chinese, "镜子已经碎了");
            text.AddTranslation(GameCulture.Russian, "Похоже, что зеркало разбито.");
			AddTranslation(text);
			
			text = CreateTranslation("CurrentQuest");
			text.SetDefault("Current Quest");
            text.AddTranslation(GameCulture.Chinese, "当前任务");
			text.AddTranslation(GameCulture.Russian, "Текущий квест");
			AddTranslation(text);
			
			text = CreateTranslation("TurnIn");
            text.SetDefault("Ready for turn-in");
            text.AddTranslation(GameCulture.Chinese, "任务完成");
            text.AddTranslation(GameCulture.Russian, "Можно сдать");
			AddTranslation(text);
			
			text = CreateTranslation("Information1");
			text.SetDefault("Thank you for playing with Antiaris!");
			text.AddTranslation(GameCulture.Chinese, "感谢你游玩Antiaris！我们的QQ官方群号码:669341455!");
			text.AddTranslation(GameCulture.Russian, "Спасибо, что играете с Antiaris!");
			AddTranslation(text);

			text = CreateTranslation("TimeStop1");
            text.SetDefault("<{0}> Time, stop!");
            text.AddTranslation(GameCulture.Chinese, "<{0}> 时停！");
            text.AddTranslation(GameCulture.Russian, "<{0}> Время, остановись!");
            AddTranslation(text);
			
			text = CreateTranslation("TimeStop2");
            text.SetDefault("<{0}> Time has resumed.");
            text.AddTranslation(GameCulture.Chinese, "<{0}> 时间恢复运转");
            text.AddTranslation(GameCulture.Russian, "<{0}> Время возобновило свой ход.");
            AddTranslation(text);
			
			text = CreateTranslation("PixieLampCollect");
            text.SetDefault("Collect");
            text.AddTranslation(GameCulture.Chinese, "收集");
            text.AddTranslation(GameCulture.Russian, "Собрать");
            AddTranslation(text);
			
			text = CreateTranslation("PixieLamp");
            text.SetDefault("Pixie lamp is slowly floating in the air, attracting pixies with it's look.");
            text.AddTranslation(GameCulture.Chinese, "精灵之灯在空中缓缓飘动，看样子吸引了诸多精灵");
            text.AddTranslation(GameCulture.Russian, "Лампа пикси медленно парит в воздухе, привлекая пикси своим видом.");
            AddTranslation(text);
			
			text = CreateTranslation("BitesTheDust");
			text.SetDefault("{0} bites the dust...");
			text.AddTranslation(GameCulture.Chinese, "{0} 尘埃落定...");
			text.AddTranslation(GameCulture.Russian, "{0} глотает пыль...");
			AddTranslation(text);

			text = CreateTranslation("SnowHouseGen");
			text.SetDefault("A cozy house appears in snow-capped lands...");
			text.AddTranslation(GameCulture.Russian, "Уютный домик появляется в заснеженных землях...");
			text.AddTranslation(GameCulture.Chinese, "一个舒适的房子出现在积雪覆盖的土地上…");
            AddTranslation(text);
			
			text = CreateTranslation("InjuredDeath");
            text.SetDefault("{0} couldn't stop the bleeding.");
            text.AddTranslation(GameCulture.Russian, "{0} не смог остановить кровотечение.");
            text.AddTranslation(GameCulture.Chinese, "{0} 无法止住流血...");
            AddTranslation(text);

            text = CreateTranslation("AdventurerSaid");
            text.SetDefault("I've written down\nAdventurer's words, he said: \n");
            text.AddTranslation(GameCulture.Russian, "Я записал слова\nПутешественника, он сказал: \n");
            text.AddTranslation(GameCulture.Chinese, "我记下了冒险家的话，他说：\n");
            AddTranslation(text);

            text = CreateTranslation("AdventurerHelp");
            text.SetDefault("\n\nI need to help him if I want to get a reward.");
            text.AddTranslation(GameCulture.Russian, "\n\nЯ должен помочь ему, если хочу получить награду.");
            text.AddTranslation(GameCulture.Chinese, "\n\n看来如果我想得到报酬，我需要帮助他。");
            AddTranslation(text);

            text = CreateTranslation("NoTask");
            text.SetDefault("I don't have any tasks from Adventurer.\nMaybe I should ask him if he has one for me?");
            text.AddTranslation(GameCulture.Russian, "У меня нету никаких заданий от\nПутешественника. Может мне стоит спросить его, есть ли у него какое-нибудь задание для меня?");
            text.AddTranslation(GameCulture.Chinese, "我目前没有冒险家的任何任务\n也许我应该问他是否有一个能够给我？");
            AddTranslation(text);

            text = CreateTranslation("TrackerButton");
            text.SetDefault("You can move the tracker by holding it.\nPress this button to get full quest description.\nPress {0} to open/close the tracker.");
            text.AddTranslation(GameCulture.Russian, "Вы можете передвигать трэкер, держа его.\nНажмите эту кнопку, чтобы получить полное описание квеста.\nPress {0}, чтобы открыть/закрыть трэкер.");
            text.AddTranslation(GameCulture.Chinese, "");
            AddTranslation(text);

            text = CreateTranslation("TrackerButton1");
            text.SetDefault("You can move the tracker by holding it.\nPress this button to get full quest description.\nPress ");
            text.AddTranslation(GameCulture.Russian, "Вы можете передвигать трэкер, держа его.\nНажмите эту кнопку, чтобы получить полное описание квеста.\nНажмите ");
            text.AddTranslation(GameCulture.Chinese, "你可以按住任务追踪器来拖动它\n点击这个按钮得到完整的任务描述\n点击 ");
            AddTranslation(text);

            text = CreateTranslation("TrackerButton2");
            text.SetDefault(" to open/close the tracker.");
            text.AddTranslation(GameCulture.Russian, ", чтобы открыть/закрыть трэкер");
            text.AddTranslation(GameCulture.Chinese, " 打开/关闭追踪器");
            AddTranslation(text);

            text = CreateTranslation("TrackerNoQuest1");
            text.SetDefault("You have no quest active!");
            text.AddTranslation(GameCulture.Russian, "У вас нету активного квеста!");
            text.AddTranslation(GameCulture.Chinese, "你目前没有任务！");
            AddTranslation(text);

            text = CreateTranslation("TrackerNoQuest2");
            text.SetDefault("");
            text.AddTranslation(GameCulture.Russian, "");
            text.AddTranslation(GameCulture.Chinese, "");
            AddTranslation(text);

            text = CreateTranslation("TrackerNoQuest3");
            text.SetDefault("");
            text.AddTranslation(GameCulture.Russian, "");
            text.AddTranslation(GameCulture.Chinese, "");
            AddTranslation(text);

            text = CreateTranslation("LifeCrystalCanUse");
            text.SetDefault("[c/E5000B:Amount of Life Crystals you can use: {0}]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Количество Кристаллов жизни, которых вы можете использовать: {0}]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你可以使用的生命水晶数量 {0}]");
            AddTranslation(text);

            text = CreateTranslation("LifeCrystalNoUse");
            text.SetDefault("[c/E5000B:You've reached the limit of using Life Crystals!]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Вы достигли лимит по использованию Кристаллов жизни!]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你已经达到使用生命水晶的最大上限了！]");
            AddTranslation(text);

            text = CreateTranslation("LifeCrystalNoUse2");
            text.SetDefault("[c/E5000B:In order to increase maximum amount of health, find Blazing Hearts in the Underworld.]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Для того, чтобы увеличить максимальное количество жизней, найдите Пылающие сердца в Аду.]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:如果你想继续增加最大体力值，请到地狱寻找燃烧之心]");
            AddTranslation(text);

            text = CreateTranslation("BlazingHeartCanUse");
            text.SetDefault("[c/E5000B:Amount of Blazing Hearts you can use: {0}]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Количество Пылающих сердец, которых вы можете использовать: {0}]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你可以使用的燃烧之心数量 {0}]");
            AddTranslation(text);

            text = CreateTranslation("BlazingHeartCantUse");
            text.SetDefault("[c/E5000B:You can't use Blazing Hearts until you reach 300 health!]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Вы не можете использовать Пылающие сердца, пока не достигнете 300 здоровья!]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你不能够在最大体力值到 300 之前使用燃烧之心！]");
            AddTranslation(text);

            text = CreateTranslation("BlazingHeartNoUse");
            text.SetDefault("[c/E5000B:You've reached the limit of using Blazing Hearts!]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Вы достигли лимит по использованию Пылающих сердец!]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你已经达到使用燃烧之心的最大上限了！]");
            AddTranslation(text);

            text = CreateTranslation("BlazingHeartNoUse2");
            text.SetDefault("[c/E5000B:In order to increase maximum amount of health, find Dazzling Hearts in the Underground Hallow.]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Для того, чтобы увеличить максимальное количество жизней, найдите Сияющие сердца в подземном Святом биоме.]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:如果你想继续增加最大体力值，请到神圣之地的地下寻找璀璨之心]");
            AddTranslation(text);

            text = CreateTranslation("DazzlingHeartCanUse");
            text.SetDefault("[c/E5000B:Amount of Dazzling Hearts you can use: {0}]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Количество Сияющих сердец, которых вы можете использовать: {0}]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你可以使用的璀璨之心数量 {0}]");
            AddTranslation(text);

            text = CreateTranslation("DazzlingHeartCantUse");
            text.SetDefault("[c/E5000B:You can't use Dazzling Hearts until you reach 400 health!]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Вы не можете использовать Сияющие сердца, пока не достигнете 400 здоровья!]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你不能够在最大体力值到 400 之前使用璀璨之心！]");
            AddTranslation(text);

            text = CreateTranslation("DazzlingHeartNoUse");
            text.SetDefault("[c/E5000B:You've reached the limit of using Dazzling Hearts!]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Вы достигли лимит по использованию Сияющих сердец!]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你已经达到使用璀璨之心的最大上限了！]");
            AddTranslation(text);

            text = CreateTranslation("DazzlingHeartNoUse2");
            text.SetDefault("[c/E5000B:In order to increase maximum amount of health, find Life Fruits in the Underground Jungle after you defeat any mechanical boss.]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Для того, чтобы увеличить максимальное количество жизней, найдите Фрукты жизни в подземных Джунглях после победы над любым механическим боссом.]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:如果你想继续增加最大体力值，请在击败所有机械Boss后进入丛林地下寻找生命果]");
            AddTranslation(text);

            text = CreateTranslation("LifeFruitCanUse");
            text.SetDefault("[c/E5000B:Amount of Life Fruits you can use: {0}]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Количество Фруктов жизни, которых вы можете использовать: {0}]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你可以使用的生命果数量 {0}]");
            AddTranslation(text);

            text = CreateTranslation("LifeFruitCantUse");
            text.SetDefault("[c/E5000B:You can't use Life Fruits until you reach 450 health!]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Вы не можете использовать Фрукты жизни, пока не достигнете 450 здоровья!]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你不能够在最大体力值到 450 之前使用生命果！]");
            AddTranslation(text);

            text = CreateTranslation("LifeFruitNoUse");
            text.SetDefault("[c/E5000B:You've reached the limit of using Life Fruits!]");
            text.AddTranslation(GameCulture.Russian, "[c/E5000B:Вы достигли лимит по использованию Фруктов жизни!]");
            text.AddTranslation(GameCulture.Chinese, "[c/E5000B:你已经达到使用生命果的最大上限了！]");
            AddTranslation(text);

            text = CreateTranslation("RodBroken");
            text.SetDefault("<{0}> Uh-oh... I hope the Adventurer will not notice that I've broken his fishing rod again...");
            text.AddTranslation(GameCulture.Russian, "<{0}> Ой-ой... Я надеюсь, Путешественник не заметит, что я вновь сломал его удочку...");
            text.AddTranslation(GameCulture.Chinese, "<{0}> 啊哦…我希望冒险家不会注意到我又弄坏了他的鱼竿...");
            AddTranslation(text);

            text = CreateTranslation("ThanksRod1");
            text.SetDefault("Thank you for your help! I really appreciate it!");
            text.AddTranslation(GameCulture.Chinese, "十分感谢，我真的感激不尽！");
            text.AddTranslation(GameCulture.Russian, "Спасибо за твою помощь! Я очень ценю это!");
            AddTranslation(text);

            text = CreateTranslation("ThanksRod2");
            text.SetDefault("Great! Thank your for your he-... Wait... What are these strange marks?.. Oh, now I get it. Did you really tried to fish with my rod and it broke again!? Did I allow you to use it? Go away, you will not get any rewards.");
            text.AddTranslation(GameCulture.Russian, "Отлично! Спасибо тебе за по-.. Погоди-ка... Что это за странные следы?.. А, теперь мне всё ясно. Ты что, правда решил порыбачить моей удочкой и она опять сломалась!? Я тебе вообще разрешал использовать её? Уходи, не получишь ты никаких наград.");
            text.AddTranslation(GameCulture.Chinese, "漂亮！谢谢你的…诶…等等，这些奇怪的痕迹是什么？哦，我现在明白了。你真的试着使用它并且又弄坏了！？我让你用它了吗？走开，我不会给你任何报酬。");
            AddTranslation(text);

            text = CreateTranslation("HarpyEggBroken");
            text.SetDefault("<{0}> Great, the egg got destroyed! Now I have to find another one.");
            text.AddTranslation(GameCulture.Russian, "<{0}> Отлично, яйцо было уничтожено! Теперь мне придется искать ещё одно.");
            text.AddTranslation(GameCulture.Chinese, "<{0}> 漂亮，蛋已经被破坏了！现在我必须要找到下一个。");
            AddTranslation(text);

            text = CreateTranslation("HarpyEggDeath");
            text.SetDefault("Baby Egg was obliterated...");
            text.AddTranslation(GameCulture.Russian, "Маленькое Яйцо было уничтожено...");
            text.AddTranslation(GameCulture.Chinese, "蛋被扼杀了...");
            AddTranslation(text);
            #endregion
        }

        public override void Unload()
        {
            cQuestTexture = null;
            Instance = null;
            Thorium = null;
			kRPG = null;
			RockosARPG = null;
            TerrariaOverhaul = null;
            Unleveled = null;
            stand = null;
            hideTracker = null;
            trackerTexture = null;
            adventurerKey = null;


            AntiarisGlowMasks.Unload();						
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (questInterface != null)
                questInterface.Update(gameTime);
            if (questLog != null)
                questLog.Update(gameTime);
        }

        public override void AddRecipes()
        {
			ModRecipe recipe = new ModRecipe(this);
            recipe.AddIngredient(null, "Shadowflame", 12);
			recipe.AddIngredient(ItemID.SoulofNight, 6);
			recipe.AddIngredient(ItemID.Marrow);
            recipe.SetResult(3052);
            recipe.AddTile(134);
            recipe.AddRecipe();
			
			recipe = new ModRecipe(this);
            recipe.AddIngredient(null, "Shadowflame", 10);
			recipe.AddIngredient(ItemID.SoulofNight, 5);
			recipe.AddIngredient(ItemID.MagicDagger);
            recipe.SetResult(3054);
            recipe.AddTile(134);
            recipe.AddRecipe();
		}

        public override void UpdateMusic(ref int music, ref MusicPriority priority)
		{
			if (Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active)
			{
				var aPlayer = Main.player[Main.myPlayer].GetModPlayer<AntiarisPlayer>(mod);
				// Make sure your logic here goes from lowest priority to highest so your intended priority is maintained.
				if (AntiarisWorld.frozenTime)
				{
					music = 0;
					priority = MusicPriority.BossHigh;
				}
			}
		}

        public override object Call(params object[] args)
        {
            try
            {
                string message = args[0] as string;
                if (message == "AddItemQuest")
                {
                    string name = args[1] as string;
                    int itemID = Convert.ToInt32(args[2]);
                    int itemAmount = Convert.ToInt32(args[3]);
                    double weight = Convert.ToInt32(args[4]);
                    string specialThanks = args[5] as string;
                    var quest = new ItemQuest(name, itemID, itemAmount, weight, specialThanks);
                    if (args.Length > 6)
                        quest.SpawnReward = (Action<NPC>)args[6];
                    if (args.Length > 7)
                        quest.IsAvailable = (Func<bool>)args[7];
                    return QuestSystem.Quests.Count - 1;
                }
                else if (message == "AddKillQuest")
                {
                    string name = args[1] as string;
                    int[] npcType = args[2] as int[];
                    int npcAmount = Convert.ToInt32(args[3]);
                    double weight = Convert.ToInt32(args[4]);
                    string specialThanks = args[5] as string;
                    var quest = new KillQuest(name, npcType, npcAmount, weight, specialThanks);
                    if (args.Length > 6)
                        quest.SpawnReward = (Action<NPC>)args[6];
                    if (args.Length > 7)
                        quest.IsAvailable = (Func<bool>)args[7];
                    return QuestSystem.Quests.Count - 1;
                }
                else if (message == "GetCurrentQuest")
                {
                    var player = Main.player[Main.myPlayer];
                    if (args.Length > 1)
                        player = args[1] as Player;
                    return player.GetModPlayer<QuestSystem>().CurrentQuest;
                }
                else
                {
                    ErrorLogger.Log("Oh no, an error happened! Report this to Zerokk and send him the file Terraria/ModLoader/Logs/Logs.txt");
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Log("Oh no, an error happened! Report this to Zerokk and send him the file Terraria/ModLoader/Logs/Logs.txt");
                ErrorLogger.Log(e.ToString());
            }
            return "ERROR!";
        }

        public override void AddRecipeGroups()
		{
			RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemType("WoodenPickaxe")), new int[]
			{
				ItemType("WoodenPickaxe"),
				ItemType("BorealWoodPickaxe"),
				ItemType("EbonwoodPickaxe"),
				ItemType("PearlwoodPickaxe"),
				ItemType("ShadewoodPickaxe"),
				ItemType("PalmWoodPickaxe"),
				ItemType("PhantomwoodPickaxe"),
				ItemType("RichMahoganyPickaxe")
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodenPickaxe", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemType("WoodenAxe")), new int[]
			{
				ItemType("WoodenAxe"),
				ItemType("BorealWoodAxe"),
				ItemType("EbonwoodAxe"),
				ItemType("PearlwoodAxe"),
				ItemType("ShadewoodAxe"),
				ItemType("PalmWoodAxe"),
				ItemType("RichMahoganyAxe")
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodenAxe", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.WoodenHammer), new int[]
			{
				ItemID.WoodenHammer,
				ItemID.BorealWoodHammer,
				ItemID.EbonwoodHammer,
				ItemID.PearlwoodHammer,
				ItemID.ShadewoodHammer,
				ItemID.PalmWoodHammer,
				ItemID.RichMahoganyHammer
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodenHammer", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemType("WoodenSpear")), new int[]
			{
				ItemType("WoodenSpear"),
				ItemType("BorealWoodSpear"),
				ItemType("EbonwoodSpear"),
				ItemType("PearlwoodSpear"),
				ItemType("ShadewoodSpear"),
				ItemType("PalmWoodSpear"),
				ItemType("RichMahoganySpear")
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodenSpear", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.WoodenBow), new int[]
			{
				ItemID.WoodenBow,
				ItemID.BorealWoodBow,
				ItemID.EbonwoodBow,
				ItemID.PearlwoodBow,
				ItemID.ShadewoodBow,
				ItemID.PalmWoodBow,
				ItemID.RichMahoganyBow
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodenBow", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.WoodenSword), new int[]
			{
				ItemID.WoodenSword,
				ItemID.BorealWoodSword,
				ItemID.EbonwoodSword,
				ItemID.PearlwoodSword,
				ItemID.ShadewoodSword,
				ItemID.PalmWoodSword
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodenSword", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.WoodHelmet), new int[]
			{
				ItemID.WoodHelmet,
				ItemID.BorealWoodHelmet,
				ItemID.EbonwoodHelmet,
				ItemID.PearlwoodHelmet,
				ItemID.ShadewoodHelmet,
				ItemID.PalmWoodHelmet,
				ItemID.RichMahoganyHelmet
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodHelmet", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.WoodBreastplate), new int[]
			{
				ItemID.WoodBreastplate,
				ItemID.BorealWoodBreastplate,
				ItemID.EbonwoodBreastplate,
				ItemID.PearlwoodBreastplate,
				ItemID.ShadewoodBreastplate,
				ItemID.PalmWoodBreastplate,
				ItemID.RichMahoganyBreastplate
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodBreastplate", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.WoodGreaves), new int[]
			{
				ItemID.WoodGreaves,
				ItemID.BorealWoodGreaves,
				ItemID.EbonwoodGreaves,
				ItemID.PearlwoodGreaves,
				ItemID.ShadewoodGreaves,
				ItemID.PalmWoodGreaves,
				ItemID.RichMahoganyGreaves
			});
			RecipeGroup.RegisterGroup("Antiaris:WoodGreaves", group);
			
			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.SilverBar), new int[]
			{
				ItemID.SilverBar,
				ItemID.TungstenBar,
			});
			RecipeGroup.RegisterGroup("Antiaris:SilverBar", group);

            group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.CopperBar), new int[]
            {
                ItemID.CopperBar,
                ItemID.TinBar,
            });
            RecipeGroup.RegisterGroup("Antiaris:CopperBar", group);

			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.GoldBar), new int[]
			{
				ItemID.GoldBar,
				ItemID.PlatinumBar,
			});
            RecipeGroup.RegisterGroup("Antiaris:GoldBar", group);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            #region Quest Tracker + log
            Mod mod = ModLoader.GetMod("Antiaris");
			var questSystem = Main.player[Main.myPlayer].GetModPlayer<QuestSystem>(mod);
            var aPlayer = Main.player[Main.myPlayer].GetModPlayer<AntiarisPlayer>(mod);
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (MouseTextIndex != -1)
            {
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                    "Antiaris: Quest UI",
                    delegate
                    {
                        if(QuestTrackerUI.visible)
                            questTracker.Draw(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                    "Antiaris: Quest UI",
                    delegate
                    {
                        if (CurrentQuestUI.visible)
                            cQuestUI.Draw(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }

            #endregion
            if (!Main.player[Main.myPlayer].ghost && aPlayer.OpenWindow)
            {
                var index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
                var UIState = new LegacyGameInterfaceLayer("Antiaris: UI",
                    delegate
                    {
                        DrawButton(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI);
                layers.Insert(index, UIState);
            }
			if (!Main.player[Main.myPlayer].ghost && aPlayer.OpenWindow3)
            {
                var index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
                var UIState = new LegacyGameInterfaceLayer("Antiaris: UI",
                    delegate
                    {
                        DrawButton2(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI);
                layers.Insert(index, UIState);
            }
			if (!Main.player[Main.myPlayer].ghost && aPlayer.OpenWindow4)
            {
                var index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
                var UIState = new LegacyGameInterfaceLayer("Antiaris: UI",
                    delegate
                    {
                        DrawButton3(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI);
                layers.Insert(index, UIState);
            }
			if (Antiaris.kRPG == null && Antiaris.RockosARPG == null && Antiaris.Unleveled == null)
			{
				var heartLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
				var heartState = new LegacyGameInterfaceLayer("Antiaris: UI2",
					delegate
					{
						DrawNewHearts(Main.spriteBatch);
						return true;
					},
					InterfaceScaleType.UI);
				layers.Insert(heartLayer, heartState);
			}
        }

        public void DrawButton(SpriteBatch spriteBatch)
        {
            var mod = ModLoader.GetMod("Antiaris");
            var background = mod.GetTexture("Miscellaneous/NoteBackground");
            string note = Language.GetTextValue("Mods.Antiaris.Note1");
            spriteBatch.Draw(background, new Rectangle(Main.screenWidth / 2, 120, background.Width, background.Height), null, Color.White, 0f, new Vector2(background.Width / 2, background.Height / 2), SpriteEffects.None, 0f);
            Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, note, Main.screenWidth / 2 - 130, 41, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, new Vector2());
        }

        public void DrawButton2(SpriteBatch spriteBatch)
        {
            var mod = ModLoader.GetMod("Antiaris");
            var background = mod.GetTexture("Miscellaneous/NoteBackground");
            string note = Language.GetTextValue("Mods.Antiaris.Note2");
            spriteBatch.Draw(background, new Rectangle(Main.screenWidth / 2, 120, background.Width, background.Height), null, Color.White, 0f, new Vector2(background.Width / 2, background.Height / 2), SpriteEffects.None, 0f);
            Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, note, Main.screenWidth / 2 - 130, 41, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, new Vector2());
        }

        public void DrawButton3(SpriteBatch spriteBatch)
        {
            var mod = ModLoader.GetMod("Antiaris");
            var background = mod.GetTexture("Miscellaneous/NoteBackground");
            string note = Language.GetTextValue("Mods.Antiaris.Note3");
            spriteBatch.Draw(background, new Rectangle(Main.screenWidth / 2, 120, background.Width, background.Height), null, Color.White, 0f, new Vector2(background.Width / 2, background.Height / 2), SpriteEffects.None, 0f);
            Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, note, Main.screenWidth / 2 - 130, 41, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, new Vector2());
		}

        private float timer = 0.0f;
        private int currentState = 0;
        public void DrawNewHearts(SpriteBatch spriteBatch)
        {
            lifePerHeart = 20f;
            var lifeForHeart = Main.player[Main.myPlayer].statLifeMax / 20;
            var lifeForBlazingHeart = (int)((Main.player[Main.myPlayer].statLifeMax - 300) / 5f);
            if (lifeForBlazingHeart < 0)
                lifeForBlazingHeart = 0;
            if (lifeForBlazingHeart > 0)
            {
                lifeForHeart = Main.player[Main.myPlayer].statLifeMax / (20 + lifeForBlazingHeart / 4);
                lifePerHeart = (float)Main.player[Main.myPlayer].statLifeMax / 20f;
            }
            var lifeForDazzlingHeart = (int)((Main.player[Main.myPlayer].statLifeMax - 400) / 2.5f);
            if (lifeForDazzlingHeart < 0)
                lifeForDazzlingHeart = 0;
            if (lifeForDazzlingHeart > 0)
            {
                lifeForHeart = Main.player[Main.myPlayer].statLifeMax / (20 + lifeForDazzlingHeart / 4);
                lifePerHeart = (float)Main.player[Main.myPlayer].statLifeMax / 20f;
            }
            var lifeForLifeFruit = (int)((Main.player[Main.myPlayer].statLifeMax - 450) / 2.5f);
            if (lifeForLifeFruit < 0)
                lifeForLifeFruit = 0;
            if (lifeForLifeFruit > 0)
            {
                lifeForHeart = Main.player[Main.myPlayer].statLifeMax / (20 + lifeForLifeFruit / 4);
                lifePerHeart = (float)Main.player[Main.myPlayer].statLifeMax / 20f;
            }
            var playerLife = Main.player[Main.myPlayer].statLifeMax2 - Main.player[Main.myPlayer].statLifeMax;
            lifePerHeart += (float)(playerLife / lifeForHeart);
            var hearts = (int)((double)Main.player[Main.myPlayer].statLifeMax2 / (double)lifePerHeart);
            if (hearts >= 10)
                hearts = 10;
            for (int oneHeart = 1; oneHeart < (int)((double)Main.player[Main.myPlayer].statLifeMax2 / (double)lifePerHeart) + 1; ++oneHeart)
            {
                var scale = 1f;
                var checkDrawPos = false;
                var statLife = 0;
                if ((double)Main.player[Main.myPlayer].statLife >= (double)oneHeart * (double)lifePerHeart)
                {
                    statLife = 255;
                    if ((double)Main.player[Main.myPlayer].statLife == (double)oneHeart * (double)lifePerHeart)
                        checkDrawPos = true;
                }
                else
                {
                    float checkOwnLifeForDraw = ((float)Main.player[Main.myPlayer].statLife - (float)(oneHeart - 1) * lifePerHeart) / lifePerHeart;
                    statLife = (int)(30.0 + 225.0 * (double)checkOwnLifeForDraw);
                    if (statLife < 30)
                        statLife = 30;
                    scale = (float)((double)checkOwnLifeForDraw / 4.0 + 0.75);
                    if ((double)scale < 0.75)
                        scale = 0.75f;
                    if ((double)checkOwnLifeForDraw > 0.0)
                        checkDrawPos = true;
                }
                if (checkDrawPos)
                    scale += Main.cursorScale - 1.0f;
                var x = 0;
                var y = 0;
                if (oneHeart > 10)
                {
                    x -= 260;
                    y += 26;
                }
                var a = (int)((double)statLife * 0.9);
                int startX;
                var info = typeof(Main).GetField("UI_ScreenAnchorX",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                startX = (int)info.GetValue(null);
                ++timer;
                if (timer % 80f == 0f) currentState += 1;
                if (timer >= 80f) timer = 0.0f;
                if (currentState > 2) currentState = 0;
                if (!Main.player[Main.myPlayer].ghost)
                {
                    if (lifeForBlazingHeart > 0)
                    {
                        --lifeForBlazingHeart;
                        var texture2 = mod.GetTexture("Miscellaneous/LifeCrystal2");
                        spriteBatch.Draw(texture2, new Vector2((float)(500 + 26 * (oneHeart - 1) + x + startX + texture2.Width / 2), (float)(32.0 + ((double)texture2.Height - (double)texture2.Height * (double)scale) / 2.0) + (float)y + (float)(texture2.Height / 2)), new Rectangle?(new Rectangle(0, 0, texture2.Width, texture2.Height)), new Color(statLife, statLife, statLife, a), 0.0f, new Vector2((float)(texture2.Width / 2), (float)(texture2.Height / 2)), scale, SpriteEffects.None, 0.0f);
                    }
                    if (lifeForDazzlingHeart > 0)
                    {
                        --lifeForDazzlingHeart;
                        var texture2 = mod.GetTexture("Miscellaneous/LifeCrystal3");
                        spriteBatch.Draw(texture2, new Vector2((float)(500 + 26 * (oneHeart - 1) + x + startX + texture2.Width / 2), (float)(32.0 + ((double)texture2.Height - (double)texture2.Height * (double)scale) / 2.0) + (float)y + (float)(texture2.Height / 2)), new Rectangle?(new Rectangle(0, 0, texture2.Width, texture2.Height)), new Color(statLife, statLife, statLife, a), 0.0f, new Vector2((float)(texture2.Width / 2), (float)(texture2.Height / 2)), scale, SpriteEffects.None, 0.0f);
                    }
                    if (lifeForLifeFruit > 0)
                    {
                        --lifeForLifeFruit;
                        var texture3 = mod.GetTexture("Miscellaneous/LifeCrystal4");
                        spriteBatch.Draw(texture3, new Vector2((float)(500 + 26 * (oneHeart - 1) + x + startX + texture3.Width / 2), (float)(32.0 + ((double)texture3.Height - (double)texture3.Height * (double)scale) / 2.0) + (float)y + (float)(texture3.Height / 2)), new Rectangle?(new Rectangle(0, 0, texture3.Width, texture3.Height)), new Color(statLife, statLife, statLife, a), 0.0f, new Vector2((float)(texture3.Width / 2), (float)(texture3.Height / 2)), scale, SpriteEffects.None, 0.0f);
                    }
                }
            }
        }

        public static bool NoInvasion(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.invasion && ((!Main.pumpkinMoon && !Main.snowMoon) || spawnInfo.spawnTileY > Main.worldSurface || Main.dayTime) && (!Main.eclipse || spawnInfo.spawnTileY > Main.worldSurface || !Main.dayTime);
        }

        public static bool NoBiome(NPCSpawnInfo spawnInfo)
        {
            var player = spawnInfo.player;
            return !player.ZoneJungle && !player.ZoneDungeon && !player.ZoneCorrupt && !player.ZoneCrimson && !player.ZoneHoly && !player.ZoneSnow && !player.ZoneUndergroundDesert;
        }

        public static bool NoZoneAllowWater(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.sky && !spawnInfo.player.ZoneMeteor && !spawnInfo.spiderCave;
        }

        public static bool NoZone(NPCSpawnInfo spawnInfo)
        {
            return NoZoneAllowWater(spawnInfo) && !spawnInfo.water;
        }

        public static bool NormalSpawn(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.playerInTown && NoInvasion(spawnInfo);
        }

        public static bool NoZoneNormalSpawn(NPCSpawnInfo spawnInfo)
        {
            return NormalSpawn(spawnInfo) && NoZone(spawnInfo);
        }

        public static bool NoZoneNormalSpawnAllowWater(NPCSpawnInfo spawnInfo)
        {
            return NormalSpawn(spawnInfo) && NoZoneAllowWater(spawnInfo);
        }

        public static bool NoBiomeNormalSpawn(NPCSpawnInfo spawnInfo)
        {
            return NormalSpawn(spawnInfo) && NoBiome(spawnInfo) && NoZone(spawnInfo);
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            ModExplorer._drawMapIcon(this, ref mouseText);
        }
    }

    enum QuestMessageType : byte
    {
        QuestID
    }
}


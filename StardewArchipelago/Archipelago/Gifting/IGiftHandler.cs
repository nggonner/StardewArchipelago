﻿using System.Collections.Generic;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago.Gifting
{
    public interface IGiftHandler
    {
        GiftSender Sender { get; }
        void Initialize(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail);
        bool HandleGiftItemCommand(string message);
        void ReceiveAllGiftsTomorrow();
        void ExportAllGifts(string filePath);
        void SetGiftIDList(List<string> apGifts);
        void AddGiftIDToList(string giftId);
        bool OwnPlayerSentGift(string giftId);
        List<string> GetGiftIDList();
    }
}

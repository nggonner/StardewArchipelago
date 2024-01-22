﻿using System;
using System.Linq;
using Archipelago.Gifting.Net.Service;
using Microsoft.Xna.Framework;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Archipelago.Gifting
{
    private class GiftInfo
    {
        public _giftItem;
        public _giftTraits;
        public _slotName;
        
        public GiftInfo(var giftItem, var giftTraits, string slotName)
        {
            _giftItem = giftItem;
            _giftTraits = giftTraits;
            _slotName = slotName;
        }
    }
    
    public class GiftSender
    {
        private readonly IMonitor _monitor;
        private readonly ArchipelagoClient _archipelago;
        private readonly IGiftingService _giftService;
        internal GiftGenerator GiftGenerator { get; }
        private List<GiftInfo> deliveryQueue = new List<GiftInfo>();

        public GiftSender(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager, IGiftingService giftService)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _giftService = giftService;
            GiftGenerator = new GiftGenerator(itemManager);
        }

        public void SendGift(string slotName, bool isTrap)
        {
            try
            {
                if (!_archipelago.PlayerExists(slotName))
                {
                    Game1.chatBox?.addMessage($"Could not find player named {slotName}", Color.Gold);
                    return;
                }

                var giftObject = Game1.player.ActiveObject;
                if (!GiftGenerator.TryCreateGiftItem(Game1.player.ActiveObject, isTrap, out var giftItem,
                        out var giftTraits))
                {
                    // TryCreateGiftItem will log the reason if it fails
                    return;
                }

                var isValidRecipient = _giftService.CanGiftToPlayer(slotName, giftTraits.Select(x => x.Trait));
                var giftOrTrap = isTrap ? "trap" : "gift";
                if (!isValidRecipient)
                {
                    Game1.chatBox?.addMessage($"{slotName} cannot receive this {giftOrTrap}", Color.Gold);
                    return;
                }

                var itemValue = giftObject.Price * giftObject.Stack;
                var taxRate = _archipelago.SlotData.BankTax;
                var tax = (int)Math.Round(taxRate * itemValue);

                if (Game1.player.Money < tax)
                {
                    Game1.chatBox?.addMessage($"You cannot afford Joja Prime for this item", Color.Gold);
                    Game1.chatBox?.addMessage($"The tax is {taxRate * 100}% of the item's value of {itemValue}g, so you must pay {tax}g to send it",
                        Color.Gold);
                    return;
                }


                deliveryQueue.Add(new GiftInfo(giftItem, giftTraits, slotName));
                /*
                var success = _giftService.SendGift(giftItem, giftTraits, slotName, out var giftId);
                _monitor.Log(
                    $"Sending {giftOrTrap} of {giftItem.Amount} {giftItem.Name} to {slotName} with {giftTraits.Length} traits. [ID: {giftId}]",
                    LogLevel.Info);
                if (!success)
                {
                    _monitor.Log($"Gift Failed to send properly", LogLevel.Error);
                    Game1.chatBox?.addMessage($"Unknown Error occurred while sending {giftOrTrap}.", Color.Red);
                    return;
                }
                */
                Game1.player.ActiveObject = null;
                Game1.player.Money -= tax;
                Game1.chatBox?.addMessage(
                    $"{slotName} will receive your {giftOrTrap} of {giftItem.Amount} {giftItem.Name} within 1 business day",
                    Color.Gold);
                Game1.chatBox?.addMessage($"You have been charged a tax of {tax}g", Color.Gold);
                Game1.chatBox?.addMessage($"Thank you for using Joja Prime", Color.Gold);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Unknown error occurred while attempting to process gift command.{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}");
                Game1.chatBox?.addMessage($"Could not complete gifting operation. Check SMAPI for error details.", Color.Red);
                return;
            }
        }

        public void SendAllGifts()
        {   
            foreach (var gift in deliveryQueue)
            {
                try
                {
                    var success = _giftService.SendGift(gift._giftItem, gift._giftTraits, gift._slotName, out var giftId);
                    _monitor.Log($"Sending {giftOrTrap} of {giftItem.Amount} {giftItem.Name} to {slotName} with {giftTraits.Length} traits. [ID: {giftId}]",
                        LogLevel.Info);
                    if (!success)
                    {
                        _monitor.Log($"Gift Failed to send properly", LogLevel.Error);
                        Game1.chatBox?.addMessage($"Unknown Error occurred while sending {giftOrTrap}.", Color.Red);
                        return;
                    }       
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Unknown error occurred while attempting to process gift command.{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", LogLevel.Error);
                    Game1.chatBox?.addMessage($"Could not complete gifting operation. Check SMAPI for error details.", Color.Red);
                    return;
                }
            }
            deliveryQueue.Clear();
        }
    }
}

using System;
using System.Linq;

using Plus.Core;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Items.Utilities;
using Plus.HabboHotel.Catalog.Utilities;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    public class CatalogPageComposer : ServerPacket
    {
        public CatalogPageComposer(CatalogPage Page, string CataMode)
            : base(ServerPacketHeader.CatalogPageMessageComposer)
        {
            base.WriteInteger(Page.Id);
           base.WriteString(CataMode);
           base.WriteString(Page.Template);

            base.WriteInteger(Page.PageStrings1.Count);
            foreach (string s in Page.PageStrings1)
            {
               base.WriteString(s);
            }

            base.WriteInteger(Page.PageStrings2.Count);
            foreach (string s in Page.PageStrings2)
            {
               base.WriteString(s);
            }

            if (!Page.Template.Equals("frontpage") && !Page.Template.Equals("club_buy"))
            {
                base.WriteInteger(Page.Items.Count);
                foreach (CatalogItem Item in Page.Items.Values)
                {
                    base.WriteInteger(Item.Id);
                   base.WriteString(Item.Name);
                    base.WriteBoolean(false);//IsRentable
                    base.WriteInteger(Item.CostCredits);
                    if (Item.CostGotw > 0)
                    {
                        base.WriteInteger(Item.CostGotw);
                        base.WriteInteger(103);
                    }
                    else if (Item.CostDiamonds > 0)
                    {
                        base.WriteInteger(Item.CostDiamonds);
                        base.WriteInteger(5); // Diamonds
                    }
                    else
                    {
                        base.WriteInteger(Item.CostPixels);
                        base.WriteInteger(0); // Type of PixelCost
                    }

                    base.WriteBoolean(ItemUtility.CanGiftItem(Item));

                    if (Item.Data.InteractionType == InteractionType.DEAL)
                    {
                        foreach (CatalogDeal Deal in Page.Deals.Values)
                        {
                            base.WriteInteger(Deal.ItemDataList.Count);//Count

                            foreach (CatalogItem DealItem in Deal.ItemDataList.ToList())
                            {
                               base.WriteString(DealItem.Data.Type.ToString());
                                base.WriteInteger(DealItem.Data.SpriteId);
                               base.WriteString("");
                                base.WriteInteger(1);
                                base.WriteBoolean(false);
                            }
                            base.WriteInteger(0);//club_level
                            base.WriteBoolean(ItemUtility.CanSelectAmount(Item));
                            base.WriteBoolean(true);
                            base.WriteString("");
                        }
                    }
                    else
                    {
                        base.WriteInteger(string.IsNullOrEmpty(Item.Badge) ? 1 : 2);//Count 1 item if there is no badge, otherwise count as 2.
                        {
                            if (!string.IsNullOrEmpty(Item.Badge))
                            {
                               base.WriteString("b");
                               base.WriteString(Item.Badge);
                            }

                           base.WriteString(Item.Data.Type.ToString());
                            if (Item.Data.Type.ToString().ToLower() == "b")
                            {
                                //This is just a badge, append the name.
                               base.WriteString(Item.Data.ItemName);
                            }
                            else
                            {
                                base.WriteInteger(Item.Data.SpriteId);
                                if (Item.Data.InteractionType == InteractionType.WALLPAPER || Item.Data.InteractionType == InteractionType.FLOOR || Item.Data.InteractionType == InteractionType.LANDSCAPE)
                                {
                                   base.WriteString(Item.Name.Split('_')[2]);
                                }
                                else if (Item.Data.InteractionType == InteractionType.BOT)//Bots
                                {
                                    CatalogBot CatalogBot = null;
                                    if (!PlusEnvironment.GetGame().GetCatalog().TryGetBot(Item.ItemId, out CatalogBot))
                                       base.WriteString("hd-180-7.ea-1406-62.ch-210-1321.hr-831-49.ca-1813-62.sh-295-1321.lg-285-92");
                                    else
                                       base.WriteString(CatalogBot.Figure);
                                }
                                else if (Item.ExtraData != null)
                                {
                                   base.WriteString(Item.ExtraData != null ? Item.ExtraData : string.Empty);
                                }
                                base.WriteInteger(Item.Amount);
                                base.WriteBoolean(Item.IsLimited); // IsLimited
                                if (Item.IsLimited)
                                {
                                    base.WriteInteger(Item.LimitedEditionStack);
                                    base.WriteInteger(Item.LimitedEditionStack - Item.LimitedEditionSells);
                                }
                            }
                            base.WriteInteger(0); //club_level
                            base.WriteBoolean(ItemUtility.CanSelectAmount(Item));
                            base.WriteBoolean(true);
                            base.WriteString("");
                        }
                    }
                }
            }
            else
            base.WriteInteger(0);
            base.WriteInteger(-1);
            base.WriteBoolean(false);
            if (Page.Template.Equals("vip_buy"))
            {
                base.WriteInteger(912393); //Page ID
                base.WriteString("NORMAL");
                base.WriteString("vip_buy");
                base.WriteInteger(2);
                base.WriteString("hc2_clubtitle");
                base.WriteString("clubcat_pic");
                base.WriteInteger(0); //This is added in this production
                base.WriteInteger(0);
                base.WriteInteger(-1);
                base.WriteBoolean(false);
            }
            if (Page.Template.Equals("frontpage4"))
            {
                base.WriteInteger(4); // count

                //New Rare Jellyfish Lamp!
                base.WriteInteger(1); // id
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.1.name")); // name
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.1.image")); // image
                base.WriteInteger(0);
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.1.link")); // page link?
                base.WriteInteger(-1); // page id?


                base.WriteInteger(2);
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.2.name"));
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.2.image"));
                base.WriteInteger(0);
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.2.link"));
                base.WriteInteger(-1);


                base.WriteInteger(3);
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.3.name"));
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.3.image"));
                base.WriteInteger(0);
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.3.link"));
                base.WriteInteger(-1);


                base.WriteInteger(4);
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.4.name"));
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.4.image"));
                base.WriteInteger(0);
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("Landing.4.link"));
                base.WriteInteger(-1);
            }
        }
    }
}
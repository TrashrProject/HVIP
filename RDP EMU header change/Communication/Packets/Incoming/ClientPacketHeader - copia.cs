namespace Plus.Communication.Packets.Incoming
{
    public static class ClientPacketHeader
    {
        // Handshake    /*Generated @ 10-9-2016 15:39:49 -- 289 packets For PRODUCTION201608171204*/
        public const int InitCryptoMessageEvent = 260;
        public const int GenerateSecretKeyMessageEvent = 682;
        public const int UniqueIDMessageEvent = 1936;
        public const int SSOTicketMessageEvent = 435;
        public const int InfoRetrieveMessageEvent = 295;

        // Avatar
        public const int GetWardrobeMessageEvent = 3102;
        public const int SaveWardrobeOutfitMessageEvent = 2289;

        // Catalog
        public const int GetCatalogIndexMessageEvent = 1778;
        public const int GetCatalogPageMessageEvent = 1560;
        public const int PurchaseFromCatalogMessageEvent = 616;
        public const int PurchaseFromCatalogAsGiftMessageEvent = 2479;

        // Navigator

        // Messenger
        public const int GetBuddyRequestsMessageEvent = 1622;

        // Quests
        public const int GetQuestListMessageEvent = 3137;
        public const int StartQuestMessageEvent = 2931;
        public const int GetCurrentQuestMessageEvent = 705;
        public const int CancelQuestMessageEvent = 3406;

        // Room Avatar
        public const int ActionMessageEvent = 3227;
        public const int ApplySignMessageEvent = 3935;
        public const int DanceMessageEvent = 3139;
        public const int SitMessageEvent = 1905;
        public const int ChangeMottoMessageEvent = 321;
        public const int LookToMessageEvent = 3930;
        public const int DropHandItemMessageEvent = 469;

        // Room Connection
        public const int OpenFlatConnectionMessageEvent = 3326;
        public const int GoToFlatMessageEvent = 2324;

        // Room Chat
        public const int ChatMessageEvent = 2677;
        public const int ShoutMessageEvent = 529;
        public const int WhisperMessageEvent = 2617;

        // Room Engine

        // Room Furniture

        // Room Settings

        // Room Action

        // Users
        public const int GetIgnoredUsersMessageEvent = 494;

        // Moderation
        public const int OpenHelpToolMessageEvent = 1586;
        // NUEVO public const int CallForHelpPendingCallsDeletedMessageEvent = 1059;//3643
        public const int ModeratorActionMessageEvent = 3810;
        public const int ModerationMsgMessageEvent = 2435;
        public const int ModerationMuteMessageEvent = 1840;
        public const int ModerationTradeLockMessageEvent = 3501;
        public const int GetModeratorUserRoomVisitsMessageEvent = 2050;
        public const int ModerationKickMessageEvent = 2607;
        public const int GetModeratorRoomInfoMessageEvent = 1695;
        public const int GetModeratorUserInfoMessageEvent = 200;
        public const int GetModeratorRoomChatlogMessageEvent = 2493;
        public const int ModerateRoomMessageEvent = 3312;
        public const int GetModeratorUserChatlogMessageEvent = 2637;
        public const int GetModeratorTicketChatlogsMessageEvent = 2490;
        public const int ModerationCautionMessageEvent = 3434;
        public const int ModerationBanMessageEvent = 3564;
        public const int SubmitNewTicketMessageEvent = 2642;
        // NUEVO public const int CloseIssueDefaultActionEvent = 682;

        // Inventory
        public const int GetCreditsInfoMessageEvent = 2592;
        public const int GetAchievementsMessageEvent = 2093;
        public const int GetBadgesMessageEvent = 3265;
        public const int RequestFurniInventoryMessageEvent = 3379;
        public const int SetActivatedBadgesMessageEvent = 2859;
        public const int AvatarEffectActivatedMessageEvent = 3763;
        public const int AvatarEffectSelectedMessageEvent = 1700;

        public const int InitTradeMessageEvent = 1235;
        public const int TradingCancelConfirmMessageEvent = 2796;
        public const int TradingModifyMessageEvent = 3113;
        public const int TradingOfferItemMessageEvent = 1293;
        public const int TradingCancelMessageEvent = 1942;
        public const int TradingConfirmMessageEvent = 3308;
        public const int TradingOfferItemsMessageEvent = 3623;
        public const int TradingRemoveItemMessageEvent = 847;
        public const int TradingAcceptMessageEvent = 2320;

        // Register
        public const int UpdateFigureDataMessageEvent = 3919;

        // Groups
        public const int GetBadgeEditorPartsMessageEvent = 299;
        public const int GetGroupCreationWindowMessageEvent = 3111;
        public const int GetGroupFurniSettingsMessageEvent = 1519;
        public const int DeclineGroupMembershipMessageEvent = 718;
        public const int JoinGroupMessageEvent = 143;
        public const int UpdateGroupColoursMessageEvent = 1513;
        public const int SetGroupFavouriteMessageEvent = 2377;
        public const int GetGroupMembersMessageEvent = 247;

        // Group Forums
        public const int PostGroupContentMessageEvent = 2237;
        public const int GetForumStatsMessageEvent = 959;
        // NUEVO public const int UpdateForumReadMarkerMessageEvent = 972;//1659
        // NUEVO public const int UpdateForumThreadStatusMessageEvent = 3724;//2980

        // Builders Club Packets


        // Camera
        public const int HabboCameraEvent = 2333;
        public const int GetCameraRequestEvent = 664;
        // NUEVO public const int HabboCameraPictureDataMessageEvent = -1405;
        // NUEVO public const int PublishCameraPictureMessageEvent = -2933;
        // NUEVO public const int PurchaseCameraPictureMessageEvent = -3221;
        // NUEVO public const int ParticipatePictureCameraCompetitionMessageEvent = -1419;

        
            
            
            
            
            
            
        // Sound


        public const int RemoveMyRightsMessageEvent = 704;
        public const int GiveHandItemMessageEvent = 150;
        public const int GetClubGiftsMessageEvent = 1979;
        public const int GoToHotelViewMessageEvent = 3183;
        public const int GetRoomFilterListMessageEvent = 2291;
        public const int GetPromoArticlesMessageEvent = 800;
        public const int ModifyWhoCanRideHorseMessageEvent = 1917;
        public const int RemoveBuddyMessageEvent = 283;
        public const int RefreshCampaignMessageEvent = 1731;
        public const int AcceptBuddyMessageEvent = 3254;
        public const int YouTubeVideoInformationMessageEvent = 408;
        public const int FollowFriendMessageEvent = 2025;
        public const int SaveBotActionMessageEvent = 1613;
        public const int LetUserInMessageEvent = 479;
        public const int GetMarketplaceItemStatsMessageEvent = 1538;
        public const int GetSellablePetBreedsMessageEvent = 1602;
        public const int ForceOpenCalendarBoxMessageEvent = 43;
        public const int SetFriendBarStateMessageEvent = 3573;
        public const int DeleteRoomMessageEvent = 1328;
        public const int SetSoundSettingsMessageEvent = 908;
        public const int InitializeGameCenterMessageEvent = 872;
        public const int RedeemOfferCreditsMessageEvent = 2367;
        public const int FriendListUpdateMessageEvent = 2814;
        public const int ConfirmLoveLockMessageEvent = 1278;
        public const int UseHabboWheelMessageEvent = 3931;
        public const int SaveRoomSettingsMessageEvent = 2244;
        public const int ToggleMoodlightMessageEvent = 3200;
        public const int GetDailyQuestMessageEvent = 1822;
        public const int SetMannequinNameMessageEvent = 2247;
        public const int UseOneWayGateMessageEvent = 3560;
        public const int EventTrackerMessageEvent = 1480;
        public const int FloorPlanEditorRoomPropertiesMessageEvent = 3063;
        public const int PickUpPetMessageEvent = 714;
        public const int GetPetInventoryMessageEvent = 765;
        public const int InitializeFloorPlanSessionMessageEvent = 1556;
        public const int GetOwnOffersMessageEvent = 106;
        public const int CheckPetNameMessageEvent = 1060;
        public const int SetUserFocusPreferenceEvent = 897;
        public const int SubmitBullyReportMessageEvent = 1615;
        public const int RemoveRightsMessageEvent = 2559;
        public const int MakeOfferMessageEvent = 3117;
        public const int KickUserMessageEvent = 1240;
        public const int GetRoomSettingsMessageEvent = 1378;
        public const int GetThreadsListDataMessageEvent = 2709;
        public const int GetForumUserProfileMessageEvent = 1794;
        public const int SaveWiredEffectConfigMessageEvent = 1394;
        public const int GetRoomEntryDataMessageEvent = 2518;
        public const int JoinPlayerQueueMessageEvent = 0;
        public const int CanCreateRoomMessageEvent = 999;
        public const int SetTonerMessageEvent = 1655;
        public const int SaveWiredTriggerConfigMessageEvent = 3964;
        public const int PlaceBotMessageEvent = 2036;
        public const int GetRelationshipsMessageEvent = 744;
        public const int SetMessengerInviteStatusMessageEvent = 3148;
        public const int UseFurnitureMessageEvent = 3023;
        public const int GetUserFlatCatsMessageEvent = 3673;
        public const int AssignRightsMessageEvent = 351;
        public const int GetRoomBannedUsersMessageEvent = 3251;
        public const int ReleaseTicketMessageEvent = 175;
        public const int OpenPlayerProfileMessageEvent = 1456;
        public const int GetSanctionStatusMessageEvent = 3467;
        public const int CreditFurniRedeemMessageEvent = 171;
        public const int DisconnectionMessageEvent = 2663;
        public const int PickupObjectMessageEvent = 3353;
        public const int FindRandomFriendingRoomMessageEvent = 2534;
        public const int UseSellableClothingMessageEvent = 1692;
        public const int MoveObjectMessageEvent = 2034;
        public const int GetFurnitureAliasesMessageEvent = 2326;
        public const int TakeAdminRightsMessageEvent = 1892;
        public const int ModifyRoomFilterListMessageEvent = 1164;
        public const int MoodlightUpdateMessageEvent = 2798;
        public const int GetPetTrainingPanelMessageEvent = 3474;
        public const int GetSongInfoMessageEvent = 303;
        public const int UseWallItemMessageEvent = 2773;
        public const int GetTalentTrackMessageEvent = 2133;
        public const int GiveAdminRightsMessageEvent = 2904;
        public const int GetCatalogModeMessageEvent = 870;
        public const int SendBullyReportMessageEvent = 3621;
        public const int CancelOfferMessageEvent = 3832;
        public const int SaveWiredConditionConfigMessageEvent = 2776;
        public const int RedeemVoucherMessageEvent = 1709;
        public const int ThrowDiceMessageEvent = 1636;
        public const int CraftSecretMessageEvent = 3092;
        public const int GetGameListingMessageEvent = 941;
        public const int SetRelationshipMessageEvent = 605;
        public const int RequestBuddyMessageEvent = 2640;
        public const int MemoryPerformanceMessageEvent = 576;
        public const int ToggleYouTubeVideoMessageEvent = 3330;
        public const int SetMannequinFigureMessageEvent = 1351;
        public const int GetEventCategoriesMessageEvent = 1919;
        public const int DeleteGroupThreadMessageEvent = 2877;
        public const int PurchaseGroupMessageEvent = 2249;
        public const int MessengerInitMessageEvent = 1873;
        public const int CancelTypingMessageEvent = 3678;
        public const int GetMoodlightConfigMessageEvent = 380;
        public const int GetGroupInfoMessageEvent = 2443;
        public const int CreateFlatMessageEvent = 1802;
        public const int LatencyTestMessageEvent = 2274;
        public const int GetSelectedBadgesMessageEvent = 3087;
        public const int AddStickyNoteMessageEvent = 294;
        public const int ChangeNameMessageEvent = 1318;
        public const int RideHorseMessageEvent = 2408;
        public const int InitializeNewNavigatorMessageEvent = 410;
        public const int SetChatPreferenceMessageEvent = 1938;
        public const int GetForumsListDataMessageEvent = 1035;
        public const int ToggleMuteToolMessageEvent = 3112;
        public const int UpdateGroupIdentityMessageEvent = 1407;
        public const int UpdateStickyNoteMessageEvent = 3076;
        public const int UnbanUserFromRoomMessageEvent = 2245;
        public const int UnIgnoreUserMessageEvent = 1881;
        public const int OpenGiftMessageEvent = 1138;
        public const int ApplyDecorationMessageEvent = 1406;
        public const int GetRecipeConfigMessageEvent = 192;
        public const int ScrGetUserInfoMessageEvent = 983;
        public const int RemoveGroupMemberMessageEvent = 1922;
        public const int DiceOffMessageEvent = 2933;
        public const int YouTubeGetNextVideo = 2110;
        public const int DeleteFavouriteRoomMessageEvent = 3176;
        public const int RespectUserMessageEvent = 3913;
        public const int AddFavouriteRoomMessageEvent = 3765;
        public const int DeclineBuddyMessageEvent = 1102;
        public const int StartTypingMessageEvent = 567;
        public const int GetGroupFurniConfigMessageEvent = 3513;
        public const int SendRoomInviteMessageEvent = 3737;
        public const int RemoveAllRightsMessageEvent = 1593;
        public const int GetYouTubeTelevisionMessageEvent = 1044;
        public const int FindNewFriendsMessageEvent = 1645;
        public const int GetPromotableRoomsMessageEvent = 965;
        public const int GetBotInventoryMessageEvent = 1348;
        public const int GetRentableSpaceMessageEvent = 634;
        public const int OpenBotActionMessageEvent = 3565;
        public const int OpenCalendarBoxMessageEvent = 309;
        public const int DeleteGroupPostMessageEvent = 241;
        public const int CheckValidNameMessageEvent = 2356;
        public const int UpdateGroupBadgeMessageEvent = 3270;
        public const int PlaceObjectMessageEvent = 3914;
        public const int RemoveGroupFavouriteMessageEvent = 3108;
        public const int UpdateNavigatorSettingsMessageEvent = 2597;
        public const int CheckGnomeNameMessageEvent = 3574;
        public const int NavigatorSearchMessageEvent = 3708;
        public const int GetPetInformationMessageEvent = 3171;
        public const int GetGuestRoomMessageEvent = 127;
        public const int UpdateThreadMessageEvent = 988;
        public const int AcceptGroupMembershipMessageEvent = 3659;
        public const int GetMarketplaceConfigurationMessageEvent = 2037;
        public const int Game2GetWeeklyLeaderboardMessageEvent = 288;
        public const int BuyOfferMessageEvent = 1510;
        public const int RemoveSaddleFromHorseMessageEvent = 2615;
        public const int GiveRoomScoreMessageEvent = 2254;
        public const int GetHabboClubWindowMessageEvent = 3078;
        public const int DeleteStickyNoteMessageEvent = 1104;
        public const int MuteUserMessageEvent = 1841;
        public const int ApplyHorseEffectMessageEvent = 626;
        public const int GetClientVersionMessageEvent = 4000;
        public const int OnBullyClickMessageEvent = 2030;
        public const int HabboSearchMessageEvent = 2307;
        public const int PickTicketMessageEvent = 3105;
        public const int GetGiftWrappingConfigurationMessageEvent = 3283;
        public const int GetCraftingRecipesAvailableMessageEvent = 1774;
        public const int GetThreadDataMessageEvent = 2855;
        public const int ManageGroupMessageEvent = 432;
        public const int PlacePetMessageEvent = 1740;
        public const int EditRoomPromotionMessageEvent = 2809;
        public const int GetCatalogOfferMessageEvent = 1114;
        public const int SaveFloorPlanModelMessageEvent = 2115;
        public const int MoveWallItemMessageEvent = 3865;
        public const int ClientVariablesMessageEvent = 3175;
        public const int PingMessageEvent = 3219;
        public const int DeleteGroupMessageEvent = 2123;
        public const int UpdateGroupSettingsMessageEvent = 2459;
        public const int GetRecyclerRewardsMessageEvent = 1807;
        public const int PurchaseRoomPromotionMessageEvent = 562;
        public const int PickUpBotMessageEvent = 680;
        public const int GetOffersMessageEvent = 2116;
        public const int GetHabboGroupBadgesMessageEvent = 296;
        public const int GetUserTagsMessageEvent = 1471;
        public const int GetPlayableGamesMessageEvent = 1576;
        public const int GetCatalogRoomPromotionMessageEvent = 3998;
        public const int MoveAvatarMessageEvent = 1887;
        public const int SaveBrandingItemMessageEvent = 1333;
        public const int SaveEnforcedCategorySettingsMessageEvent = 1608;
        public const int RespectPetMessageEvent = 608;
        public const int GetMarketplaceCanMakeOfferMessageEvent = 3944;
        public const int UpdateMagicTileMessageEvent = 2912;
        public const int GetStickyNoteMessageEvent = 3586;
        public const int IgnoreUserMessageEvent = 521;
        public const int BanUserMessageEvent = 618;
        public const int UpdateForumSettingsMessageEvent = 1493;
        public const int GetRoomRightsMessageEvent = 645;
        public const int SendMsgMessageEvent = 1224;
        public const int CloseTicketMesageEvent = 1284;


        /* NO ESTÁN EN ALASKA */

        public const int TourRequestEvent = 3263;        
        public const int EventLogMessageEvent = 1480;
        public const int PickRoomEvent = 1265;
        public const int GuideSessionOnDutyUpdateMessageEvent = 2806;
        public const int GuideSessionIsTypingMessageEvent = 967;
        public const int PollRejectMessageEvent = 2293;        
        public const int SetObjectDataMessageEvent = 1333;        
        public const int GuideSessionCreateMessageEvent = 2226;
        public const int ForumViewThreadButtonClickEvent = 523;
        public const int SaveRoomThumbnailEvent = 1440;
        public const int GetHabboClubCenterInfoMessageEvent = 1683;
        public const int AmbassadorWarningMessageEvent = 3181;
        public const int GuideSessionInviteRequesterMessageEvent = 1620;        
        public const int ApproveNameMessageEvent = 1060;
        public const int CallForHelpMessageEvent = 2642;
        public const int RoomDimmerSavePresetMessageEvent = 2798;
        public const int PostQuizAnswersMessageEvent = 118;
        public const int PollStartMessageEvent = 3182;
        public const int PerformanceLogMessageEvent = 576;
        public const int GetMarketplaceOffersMessageEvent = 2116;
        public const int CallForHelpFromForumThreadMessageEvent = 2131;
        public const int ModerationDefaultSanctionMessageEvent = 2656;
        public const int CallForHelpFromIMMessageEvent = 672;
        public const int CommandBotMessageEvent = 1613;
        public const int SetCustomStackingHeightMessageEvent = 2912;
        public const int GetClubOffersMessageEvent = 3078;
        public const int UpdateFloorPropertiesMessageEvent = 2115;
        public const int CallForHelpFromForumMessageMessageEvent = 676;
        public const int PollAnswerMessageEvent = 783;
        public const int GetSoundSettingsMessageEvent = 2500;        
        public const int GuideSessionResolvedMessageEvent = 907;
        public const int InitializeNavigatorMessageEvent = 410;
        public const int GetExtendedProfileMessageEvent = 1456;
        public const int GuideSessionMsgMessageEvent = 627;
        public const int VersionCheckMessageEvent = 3175;
        public const int GuideSessionGetRequesterRoomMessageEvent = 2169;
        public const int SaveNavigatorPositionEvent = 2744;
        public const int MySanctionStatusMessageEvent = 3467;
        public const int GuideHelpMessageEvent = 3263;
        public const int NewNavigatorSearchMessageEvent = 3708;        
        public const int GetGroupForumDataMessageEvent = 959;
        public const int SetUsernameMessageEvent = 494;        
        public const int GetGroupForumThreadRootMessageEvent = 2709;
        public const int CheckQuizTypeEvent = 3542;
        public const int ReadForumThreadMessageEvent = 2855;        
        public const int GetGroupForumsMessageEvent = 1035;
        public const int PublishForumThreadMessageEvent = 2237;
        public const int AlterForumThreadStateMessageEvent = 2877;
        public const int NuxCloseMessageEvent = 629;
        public const int CallGuide = 3263;
        public const int AnswerGuideRequest = 2226;
        public const int OpenGuideTool = 2806;
        public const int GuideSpeak = 627;
        public const int InviteToRoom = 1620;
        public const int VisitRoom = 2169;
        public const int CloseGuideRequest = 907;
        public const int CancelCallGuide = 2;
        public const int BullyRequestAccept = 1121;
        public const int BullyRequestVote = 1611;
        public const int BullyRequestNoVote = 204;
        public const int VolverAmostrar = 2468;
        public const int VolverAmostrar2 = 967;
        public const int AmbassadorAlert = 3181;
        //to buy from targeted offers
        public const int TargetedOfferBuyEvent = 1553;
        // Camera 2016
        public const int RenderRoomEvent = 664;
        public const int PurchasePhotoEvent = 2333;
        public const int PublishPhotoEvent = 3776;
        public const int GetCameraPriceEvent = 2690;
        public const int ReportCameraPhotoEvent = 1662;
        public const int SetRoomThumbnailMessageEvent = 1440;
    }
}
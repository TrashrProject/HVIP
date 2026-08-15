namespace Plus.Communication.Packets.Incoming
{
    public static class ClientPacketHeader
    {
         // Handshake    /*Generated @ 10-9-2016 15:39:49 -- 289 packets For PRODUCTION201608171204*/
        public const int InitCryptoMessageEvent = 4000; // PRODUCTION-201709052204-426856518
        public const int GenerateSecretKeyMessageEvent = 3823; // PRODUCTION-201709052204-426856518
        public const int UniqueIDMessageEvent = 2701; // PRODUCTION-201709052204-426856518
        public const int SSOTicketMessageEvent = 1029; // PRODUCTION-201709052204-426856518
        public const int InfoRetrieveMessageEvent = 2078; // PRODUCTION-201709052204-426856518

        // Avatar
        public const int GetWardrobeMessageEvent = 1602; // PRODUCTION-201709052204-426856518
        public const int SaveWardrobeOutfitMessageEvent = 1794; // PRODUCTION-201709052204-426856518

        // Catalog
        public const int GetCatalogIndexMessageEvent = 1215; // PRODUCTION-201709052204-426856518
        public const int GetCatalogPageMessageEvent = 3365; // PRODUCTION-201709052204-426856518
        public const int PurchaseFromCatalogMessageEvent = 2223; // PRODUCTION-201709052204-426856518
        public const int PurchaseFromCatalogAsGiftMessageEvent = 53; // PRODUCTION-201709052204-426856518

        // Navigator

        // Messenger
        public const int GetBuddyRequestsMessageEvent = 688; // PRODUCTION-201709052204-426856518

        // Quests
        public const int GetQuestListMessageEvent = 2081; // PRODUCTION-201709052204-426856518
        public const int StartQuestMessageEvent = 1395; // PRODUCTION-201709052204-426856518
        public const int GetCurrentQuestMessageEvent = 1107; // PRODUCTION-201709052204-426856518
        public const int CancelQuestMessageEvent = 1985; // PRODUCTION-201709052204-426856518

        // Room Avatar
        public const int ActionMessageEvent = 3097; // PRODUCTION-201709052204-426856518
        public const int ApplySignMessageEvent = 205; // PRODUCTION-201709052204-426856518
        public const int DanceMessageEvent = 1197; // PRODUCTION-201709052204-426856518
        public const int SitMessageEvent = 639; // PRODUCTION-201709052204-426856518
        public const int ChangeMottoMessageEvent = 570; // PRODUCTION-201709052204-426856518
        public const int LookToMessageEvent = 1772; // PRODUCTION-201709052204-426856518
        public const int DropHandItemMessageEvent = 2776; // PRODUCTION-201709052204-426856518

        // Room Connection
        public const int OpenFlatConnectionMessageEvent = 3305; // PRODUCTION-201709052204-426856518
        public const int GoToFlatMessageEvent = 982; // PRODUCTION-201709052204-426856518

        // Room Chat
        public const int ChatMessageEvent = 563; // PRODUCTION-201709052204-426856518
        public const int ShoutMessageEvent = 1565; // PRODUCTION-201709052204-426856518
        public const int WhisperMessageEvent = 2599; // PRODUCTION-201709052204-426856518

        // Room Engine

        // Room Furniture

        // Room Settings

        // Room Action

        // Users
        public const int GetIgnoredUsersMessageEvent = 2645; // PRODUCTION-201709052204-426856518

        // Moderation
        public const int OpenHelpToolMessageEvent = 1781; // PRODUCTION-201709052204-426856518
        // NUEVO public const int CallForHelpPendingCallsDeletedMessageEvent =-1;//error 404
        public const int ModeratorActionMessageEvent = 3514; // PRODUCTION-201709052204-426856518
        public const int ModerationMsgMessageEvent = 3844; // PRODUCTION-201709052204-426856518
        public const int ModerationMuteMessageEvent = 508; // PRODUCTION-201709052204-426856518
        public const int ModerationTradeLockMessageEvent = 279; // PRODUCTION-201709052204-426856518
        public const int GetModeratorUserRoomVisitsMessageEvent = 2798; // PRODUCTION-201709052204-426856518
        public const int ModerationKickMessageEvent = 1867; // PRODUCTION-201709052204-426856518
        public const int GetModeratorRoomInfoMessageEvent = 826; // PRODUCTION-201709052204-426856518
        public const int GetModeratorUserInfoMessageEvent = 1844; // PRODUCTION-201709052204-426856518
        public const int GetModeratorRoomChatlogMessageEvent = 906; // PRODUCTION-201709052204-426856518
        public const int ModerateRoomMessageEvent = 801; // PRODUCTION-201709052204-426856518
        public const int GetModeratorUserChatlogMessageEvent = 2105; // PRODUCTION-201709052204-426856518
        public const int GetModeratorTicketChatlogsMessageEvent = 450; // PRODUCTION-201709052204-426856518
        public const int ModerationCautionMessageEvent = 318; // PRODUCTION-201709052204-426856518
        public const int ModerationBanMessageEvent = 3344; // PRODUCTION-201709052204-426856518
        public const int SubmitNewTicketMessageEvent = 2244; // PRODUCTION-201709052204-426856518
        // NUEVO public const int CloseIssueDefaultActionEvent = 3823; // PRODUCTION-201709052204-426856518

        // Inventory
        public const int GetCreditsInfoMessageEvent = 2522; // PRODUCTION-201709052204-426856518
        public const int GetAchievementsMessageEvent = 1797; // PRODUCTION-201709052204-426856518
        public const int GetBadgesMessageEvent = 166; // PRODUCTION-201709052204-426856518
        public const int RequestFurniInventoryMessageEvent = 3818; // PRODUCTION-201709052204-426856518
        public const int SetActivatedBadgesMessageEvent = 2466; // PRODUCTION-201709052204-426856518
        public const int AvatarEffectActivatedMessageEvent = 3786; // PRODUCTION-201709052204-426856518
        public const int AvatarEffectSelectedMessageEvent = 1364; // PRODUCTION-201709052204-426856518

        public const int InitTradeMessageEvent = 293; // PRODUCTION-201709052204-426856518
        public const int TradingCancelConfirmMessageEvent = 1065; // PRODUCTION-201709052204-426856518
        public const int TradingModifyMessageEvent = 739; // PRODUCTION-201709052204-426856518
        public const int TradingOfferItemMessageEvent = 2886; // PRODUCTION-201709052204-426856518
        public const int TradingCancelMessageEvent = 1569; // PRODUCTION-201709052204-426856518
        public const int TradingConfirmMessageEvent = 2598; // PRODUCTION-201709052204-426856518
        public const int TradingOfferItemsMessageEvent = 1160; // PRODUCTION-201709052204-426856518
        public const int TradingRemoveItemMessageEvent = 1846; // PRODUCTION-201709052204-426856518
        public const int TradingAcceptMessageEvent = 1129; // PRODUCTION-201709052204-426856518

        // Register
        public const int UpdateFigureDataMessageEvent = 1631; // PRODUCTION-201709052204-426856518

        // Groups
        public const int GetBadgeEditorPartsMessageEvent = 121; // PRODUCTION-201709052204-426856518
        public const int GetGroupCreationWindowMessageEvent = 1051; // PRODUCTION-201709052204-426856518
        public const int GetGroupFurniSettingsMessageEvent = 1786; // PRODUCTION-201709052204-426856518
        public const int DeclineGroupMembershipMessageEvent = 1308; // PRODUCTION-201709052204-426856518
        public const int JoinGroupMessageEvent = 3749; // PRODUCTION-201709052204-426856518
        public const int UpdateGroupColoursMessageEvent = 1475; // PRODUCTION-201709052204-426856518
        public const int SetGroupFavouriteMessageEvent = 1604; // PRODUCTION-201709052204-426856518
        public const int GetGroupMembersMessageEvent = 139; // PRODUCTION-201709052204-426856518

        // Group Forums
        public const int PostGroupContentMessageEvent = 794; // PRODUCTION-201709052204-426856518
        public const int GetForumStatsMessageEvent = 228; // PRODUCTION-201709052204-426856518
        // NUEVO public const int UpdateForumReadMarkerMessageEvent =-1;//error 404
        // NUEVO public const int UpdateForumThreadStatusMessageEvent =-1;//error 404

        // Builders Club Packets


        // Camera
        public const int HabboCameraEvent = 602; // PRODUCTION-201709052204-426856518
        public const int GetCameraRequestEvent = 362; // PRODUCTION-201709052204-426856518
        // NUEVO public const int HabboCameraPictureDataMessageEvent =-1;//error 404
        // NUEVO public const int PublishCameraPictureMessageEvent =-1;//error 404
        // NUEVO public const int PurchaseCameraPictureMessageEvent =-1;//error 404
        // NUEVO public const int ParticipatePictureCameraCompetitionMessageEvent =-1;//error 404

        
            
            
            
            
            
            
        // Sound


        public const int RemoveMyRightsMessageEvent = 673; // PRODUCTION-201709052204-426856518
        public const int GiveHandItemMessageEvent = 467; // PRODUCTION-201709052204-426856518
        public const int GetClubGiftsMessageEvent = 3142; // PRODUCTION-201709052204-426856518
        public const int GoToHotelViewMessageEvent = 2539; // PRODUCTION-201709052204-426856518
        public const int GetRoomFilterListMessageEvent = 566; // PRODUCTION-201709052204-426856518
        public const int GetPromoArticlesMessageEvent = 3678; // PRODUCTION-201709052204-426856518
        public const int ModifyWhoCanRideHorseMessageEvent = 2253; // PRODUCTION-201709052204-426856518
        public const int RemoveBuddyMessageEvent = 3851; // PRODUCTION-201709052204-426856518
        public const int RefreshCampaignMessageEvent = 3134; // PRODUCTION-201709052204-426856518
        public const int AcceptBuddyMessageEvent = 408; // PRODUCTION-201709052204-426856518
        public const int YouTubeVideoInformationMessageEvent = 2294; // PRODUCTION-201709052204-426856518
        public const int FollowFriendMessageEvent = 659; // PRODUCTION-201709052204-426856518
        public const int SaveBotActionMessageEvent = 909; // PRODUCTION-201709052204-426856518
        public const int LetUserInMessageEvent = 1670; // PRODUCTION-201709052204-426856518
        public const int GetMarketplaceItemStatsMessageEvent = 730; // PRODUCTION-201709052204-426856518
        public const int GetSellablePetBreedsMessageEvent = 3692; // PRODUCTION-201709052204-426856518
        public const int ForceOpenCalendarBoxMessageEvent = 1405; // PRODUCTION-201709052204-426856518
        public const int SetFriendBarStateMessageEvent = 2932; // PRODUCTION-201709052204-426856518
        public const int DeleteRoomMessageEvent = 2990; // PRODUCTION-201709052204-426856518
        public const int SetSoundSettingsMessageEvent = 3056; // PRODUCTION-201709052204-426856518
        public const int InitializeGameCenterMessageEvent = 2594; // PRODUCTION-201709052204-426856518
        public const int RedeemOfferCreditsMessageEvent = 2913; // PRODUCTION-201709052204-426856518
        public const int FriendListUpdateMessageEvent = 227; // PRODUCTION-201709052204-426856518
        public const int ConfirmLoveLockMessageEvent = 2019; // PRODUCTION-201709052204-426856518
        public const int UseHabboWheelMessageEvent = 1537; // PRODUCTION-201709052204-426856518
        public const int SaveRoomSettingsMessageEvent = 1099; // PRODUCTION-201709052204-426856518
        public const int ToggleMoodlightMessageEvent = 281; // PRODUCTION-201709052204-426856518
        public const int GetDailyQuestMessageEvent = 2154; // PRODUCTION-201709052204-426856518
        public const int SetMannequinNameMessageEvent = 1055; // PRODUCTION-201709052204-426856518
        public const int UseOneWayGateMessageEvent = 2838; // PRODUCTION-201709052204-426856518
        public const int EventTrackerMessageEvent = 734; // PRODUCTION-201709052204-426856518
        public const int FloorPlanEditorRoomPropertiesMessageEvent = 2796; // PRODUCTION-201709052204-426856518
        public const int PickUpPetMessageEvent = 2681; // PRODUCTION-201709052204-426856518
        public const int GetPetInventoryMessageEvent = 3735; // PRODUCTION-201709052204-426856518
        public const int InitializeFloorPlanSessionMessageEvent = 965; // PRODUCTION-201709052204-426856518
        public const int GetOwnOffersMessageEvent = 1677; // PRODUCTION-201709052204-426856518
        public const int CheckPetNameMessageEvent = 2794; // PRODUCTION-201709052204-426856518
        public const int SetUserFocusPreferenceEvent = 3405; // PRODUCTION-201709052204-426856518
        public const int SubmitBullyReportMessageEvent = 3173; // PRODUCTION-201709052204-426856518
        public const int RemoveRightsMessageEvent = 1109; // PRODUCTION-201709052204-426856518
        public const int MakeOfferMessageEvent = 1744; // PRODUCTION-201709052204-426856518
        public const int KickUserMessageEvent = 2301; // PRODUCTION-201709052204-426856518
        public const int GetRoomSettingsMessageEvent = 146; // PRODUCTION-201709052204-426856518
        public const int GetThreadsListDataMessageEvent = 1148; // PRODUCTION-201709052204-426856518
        public const int GetForumUserProfileMessageEvent = 3959; // PRODUCTION-201709052204-426856518
        public const int SaveWiredEffectConfigMessageEvent = 513; // PRODUCTION-201709052204-426856518
        public const int GetRoomEntryDataMessageEvent = 1545; // PRODUCTION-201709052204-426856518
        public const int JoinPlayerQueueMessageEvent = 1357; // PRODUCTION-201709052204-426856518
        public const int CanCreateRoomMessageEvent = 3614; // PRODUCTION-201709052204-426856518
        public const int SetTonerMessageEvent = 2931; // PRODUCTION-201709052204-426856518
        public const int SaveWiredTriggerConfigMessageEvent = 3892; // PRODUCTION-201709052204-426856518
        public const int PlaceBotMessageEvent = 7; // PRODUCTION-201709052204-426856518
        public const int GetRelationshipsMessageEvent = 155; // PRODUCTION-201709052204-426856518
        public const int SetMessengerInviteStatusMessageEvent = 3436; // PRODUCTION-201709052204-426856518
        public const int UseFurnitureMessageEvent = 926; // PRODUCTION-201709052204-426856518
        public const int GetUserFlatCatsMessageEvent = 3329; // PRODUCTION-201709052204-426856518
        public const int AssignRightsMessageEvent = 948; // PRODUCTION-201709052204-426856518
        public const int GetRoomBannedUsersMessageEvent = 2359; // PRODUCTION-201709052204-426856518
        public const int ReleaseTicketMessageEvent = 2507; // PRODUCTION-201709052204-426856518
        public const int OpenPlayerProfileMessageEvent = 1058; // PRODUCTION-201709052204-426856518
        public const int GetSanctionStatusMessageEvent = 1015; // PRODUCTION-201709052204-426856518
        public const int CreditFurniRedeemMessageEvent = 153; // PRODUCTION-201709052204-426856518
        public const int DisconnectionMessageEvent = 2057; // PRODUCTION-201709052204-426856518
        public const int PickupObjectMessageEvent = 1046; // PRODUCTION-201709052204-426856518
        public const int FindRandomFriendingRoomMessageEvent = 2638; // PRODUCTION-201709052204-426856518
        public const int UseSellableClothingMessageEvent = 3114; // PRODUCTION-201709052204-426856518
        public const int MoveObjectMessageEvent = 3174; // PRODUCTION-201709052204-426856518
        public const int GetFurnitureAliasesMessageEvent = 723; // PRODUCTION-201709052204-426856518
        public const int TakeAdminRightsMessageEvent = 258; // PRODUCTION-201709052204-426856518
        public const int ModifyRoomFilterListMessageEvent = 590; // PRODUCTION-201709052204-426856518
        public const int MoodlightUpdateMessageEvent = 1203; // PRODUCTION-201709052204-426856518
        public const int GetPetTrainingPanelMessageEvent = 3907; // PRODUCTION-201709052204-426856518
        public const int GetSongInfoMessageEvent = 1511; // PRODUCTION-201709052204-426856518
        public const int UseWallItemMessageEvent = 264; // PRODUCTION-201709052204-426856518
        public const int GetTalentTrackMessageEvent = 3202; // PRODUCTION-201709052204-426856518
        public const int GiveAdminRightsMessageEvent = 3116; // PRODUCTION-201709052204-426856518
        public const int GetCatalogModeMessageEvent = 2481; // PRODUCTION-201709052204-426856518
        public const int SendBullyReportMessageEvent = 1435; // PRODUCTION-201709052204-426856518
        public const int CancelOfferMessageEvent = 2278; // PRODUCTION-201709052204-426856518
        public const int SaveWiredConditionConfigMessageEvent = 1820; // PRODUCTION-201709052204-426856518
        public const int RedeemVoucherMessageEvent = 3444; // PRODUCTION-201709052204-426856518
        public const int ThrowDiceMessageEvent = 2977; // PRODUCTION-201709052204-426856518
        public const int CraftSecretMessageEvent = 110; // PRODUCTION-201709052204-426856518
        public const int GetGameListingMessageEvent = 2056; // PRODUCTION-201709052204-426856518
        public const int SetRelationshipMessageEvent = 930; // PRODUCTION-201709052204-426856518
        public const int RequestBuddyMessageEvent = 3816; // PRODUCTION-201709052204-426856518
        public const int MemoryPerformanceMessageEvent = 661; // PRODUCTION-201709052204-426856518
        public const int ToggleYouTubeVideoMessageEvent = 2880; // PRODUCTION-201709052204-426856518
        public const int SetMannequinFigureMessageEvent = 1599; // PRODUCTION-201709052204-426856518
        public const int GetEventCategoriesMessageEvent = 3524; // PRODUCTION-201709052204-426856518
        public const int DeleteGroupThreadMessageEvent = 3609; // PRODUCTION-201709052204-426856518
        public const int PurchaseGroupMessageEvent = 1753; // PRODUCTION-201709052204-426856518
        public const int MessengerInitMessageEvent = 743; // PRODUCTION-201709052204-426856518
        public const int CancelTypingMessageEvent = 1986; // PRODUCTION-201709052204-426856518
        public const int GetMoodlightConfigMessageEvent = 1322; // PRODUCTION-201709052204-426856518
        public const int GetGroupInfoMessageEvent = 283; // PRODUCTION-201709052204-426856518
        public const int CreateFlatMessageEvent = 3516; // PRODUCTION-201709052204-426856518
        public const int LatencyTestMessageEvent = 2998; // PRODUCTION-201709052204-426856518
        public const int GetSelectedBadgesMessageEvent = 1935; // PRODUCTION-201709052204-426856518
        public const int AddStickyNoteMessageEvent = 577; // PRODUCTION-201709052204-426856518
        public const int ChangeNameMessageEvent = 1834; // PRODUCTION-201709052204-426856518
        public const int RideHorseMessageEvent = 1481; // PRODUCTION-201709052204-426856518
        public const int InitializeNewNavigatorMessageEvent = 1217; // PRODUCTION-201709052204-426856518
        public const int SetChatPreferenceMessageEvent = 3582; // PRODUCTION-201709052204-426856518
        public const int GetForumsListDataMessageEvent = 918; // PRODUCTION-201709052204-426856518
        public const int ToggleMuteToolMessageEvent = 2677; // PRODUCTION-201709052204-426856518
        public const int UpdateGroupIdentityMessageEvent = 516; // PRODUCTION-201709052204-426856518
        public const int UpdateStickyNoteMessageEvent = 1847; // PRODUCTION-201709052204-426856518
        public const int UnbanUserFromRoomMessageEvent = 2700; // PRODUCTION-201709052204-426856518
        public const int UnIgnoreUserMessageEvent = 3677; // PRODUCTION-201709052204-426856518
        public const int OpenGiftMessageEvent = 3867; // PRODUCTION-201709052204-426856518
        public const int ApplyDecorationMessageEvent = 1416; // PRODUCTION-201709052204-426856518
        public const int GetRecipeConfigMessageEvent = 2336; // PRODUCTION-201709052204-426856518
        public const int ScrGetUserInfoMessageEvent = 220; // PRODUCTION-201709052204-426856518
        public const int RemoveGroupMemberMessageEvent = 2240; // PRODUCTION-201709052204-426856518
        public const int DiceOffMessageEvent = 1838; // PRODUCTION-201709052204-426856518
        public const int YouTubeGetNextVideo = 3788; // PRODUCTION-201709052204-426856518
        public const int DeleteFavouriteRoomMessageEvent = 3544; // PRODUCTION-201709052204-426856518
        public const int RespectUserMessageEvent = 3537; // PRODUCTION-201709052204-426856518
        public const int AddFavouriteRoomMessageEvent = 1413; // PRODUCTION-201709052204-426856518
        public const int DeclineBuddyMessageEvent = 3726; // PRODUCTION-201709052204-426856518
        public const int StartTypingMessageEvent = 403; // PRODUCTION-201709052204-426856518
        public const int GetGroupFurniConfigMessageEvent = 75; // PRODUCTION-201709052204-426856518
        public const int SendRoomInviteMessageEvent = 3746; // PRODUCTION-201709052204-426856518
        public const int RemoveAllRightsMessageEvent = 3296; // PRODUCTION-201709052204-426856518
        public const int GetYouTubeTelevisionMessageEvent = 66; // PRODUCTION-201709052204-426856518
        public const int FindNewFriendsMessageEvent = 2653; // PRODUCTION-201709052204-426856518
        public const int GetPromotableRoomsMessageEvent = 769; // PRODUCTION-201709052204-426856518
        public const int GetBotInventoryMessageEvent = 2017; // PRODUCTION-201709052204-426856518
        public const int GetRentableSpaceMessageEvent = 2908; // PRODUCTION-201709052204-426856518
        public const int OpenBotActionMessageEvent = 836; // PRODUCTION-201709052204-426856518
        public const int OpenCalendarBoxMessageEvent = 634; // PRODUCTION-201709052204-426856518
        public const int DeleteGroupPostMessageEvent = 2519; // PRODUCTION-201709052204-426856518
        public const int CheckValidNameMessageEvent = 3014; // PRODUCTION-201709052204-426856518
        public const int UpdateGroupBadgeMessageEvent = 1082; // PRODUCTION-201709052204-426856518
        public const int PlaceObjectMessageEvent = 3651; // PRODUCTION-201709052204-426856518
        public const int RemoveGroupFavouriteMessageEvent = 2093; // PRODUCTION-201709052204-426856518
        public const int UpdateNavigatorSettingsMessageEvent = 1738; // PRODUCTION-201709052204-426856518
        public const int CheckGnomeNameMessageEvent = 2325; // PRODUCTION-201709052204-426856518
        public const int NavigatorSearchMessageEvent = 2456; // PRODUCTION-201709052204-426856518
        public const int GetPetInformationMessageEvent = 2139; // PRODUCTION-201709052204-426856518
        public const int GetGuestRoomMessageEvent = 2420; // PRODUCTION-201709052204-426856518
        public const int UpdateThreadMessageEvent = 3724; // PRODUCTION-201709052204-426856518
        public const int AcceptGroupMembershipMessageEvent = 3136; // PRODUCTION-201709052204-426856518
        public const int GetMarketplaceConfigurationMessageEvent = 3065; // PRODUCTION-201709052204-426856518
        public const int Game2GetWeeklyLeaderboardMessageEvent = 2929; // PRODUCTION-201709052204-426856518
        public const int BuyOfferMessageEvent = 119; // PRODUCTION-201709052204-426856518
        public const int RemoveSaddleFromHorseMessageEvent = 994; // PRODUCTION-201709052204-426856518
        public const int GiveRoomScoreMessageEvent = 3777; // PRODUCTION-201709052204-426856518
        public const int GetHabboClubWindowMessageEvent = 3031; // PRODUCTION-201709052204-426856518
        public const int DeleteStickyNoteMessageEvent = 2458; // PRODUCTION-201709052204-426856518
        public const int MuteUserMessageEvent = 2646; // PRODUCTION-201709052204-426856518
        public const int ApplyHorseEffectMessageEvent = 2262; // PRODUCTION-201709052204-426856518
        public const int GetClientVersionMessageEvent = 4000; // PRODUCTION-201709052204-426856518
        public const int OnBullyClickMessageEvent = 3953; // PRODUCTION-201709052204-426856518
        public const int HabboSearchMessageEvent = 2745; // PRODUCTION-201709052204-426856518
        public const int PickTicketMessageEvent = 316; // PRODUCTION-201709052204-426856518
        public const int GetGiftWrappingConfigurationMessageEvent = 1027; // PRODUCTION-201709052204-426856518
        public const int GetCraftingRecipesAvailableMessageEvent = 1767; // PRODUCTION-201709052204-426856518
        public const int GetThreadDataMessageEvent = 2377; // PRODUCTION-201709052204-426856518
        public const int ManageGroupMessageEvent = 266; // PRODUCTION-201709052204-426856518
        public const int PlacePetMessageEvent = 886; // PRODUCTION-201709052204-426856518
        public const int EditRoomPromotionMessageEvent = 2562; // PRODUCTION-201709052204-426856518
        public const int GetCatalogOfferMessageEvent = 2907; // PRODUCTION-201709052204-426856518
        public const int SaveFloorPlanModelMessageEvent = 1707; // PRODUCTION-201709052204-426856518
        public const int MoveWallItemMessageEvent = 663; // PRODUCTION-201709052204-426856518
        public const int ClientVariablesMessageEvent = 3126; // PRODUCTION-201709052204-426856518
        public const int PingMessageEvent = 1623; // PRODUCTION-201709052204-426856518
        public const int DeleteGroupMessageEvent = 3320; // PRODUCTION-201709052204-426856518
        public const int UpdateGroupSettingsMessageEvent = 2178; // PRODUCTION-201709052204-426856518
        public const int GetRecyclerRewardsMessageEvent = 3846; // PRODUCTION-201709052204-426856518
        public const int PurchaseRoomPromotionMessageEvent = 1839; // PRODUCTION-201709052204-426856518
        public const int PickUpBotMessageEvent = 2090; // PRODUCTION-201709052204-426856518
        public const int GetOffersMessageEvent = 776; // PRODUCTION-201709052204-426856518
        public const int GetHabboGroupBadgesMessageEvent = 3020; // PRODUCTION-201709052204-426856518
        public const int GetUserTagsMessageEvent = 3200; // PRODUCTION-201709052204-426856518
        public const int GetPlayableGamesMessageEvent = 2792; // PRODUCTION-201709052204-426856518
        public const int GetCatalogRoomPromotionMessageEvent = 891; // PRODUCTION-201709052204-426856518
        public const int MoveAvatarMessageEvent = 2923; // PRODUCTION-201709052204-426856518
        public const int SaveBrandingItemMessageEvent = 2926; // PRODUCTION-201709052204-426856518
        public const int SaveEnforcedCategorySettingsMessageEvent = 642; // PRODUCTION-201709052204-426856518
        public const int RespectPetMessageEvent = 3804; // PRODUCTION-201709052204-426856518
        public const int GetMarketplaceCanMakeOfferMessageEvent = 3547; // PRODUCTION-201709052204-426856518
        public const int UpdateMagicTileMessageEvent = 1513; // PRODUCTION-201709052204-426856518
        public const int GetStickyNoteMessageEvent = 3389; // PRODUCTION-201709052204-426856518
        public const int IgnoreUserMessageEvent = 1473; // PRODUCTION-201709052204-426856518
        public const int BanUserMessageEvent = 464; // PRODUCTION-201709052204-426856518
        public const int UpdateForumSettingsMessageEvent = 2752; // PRODUCTION-201709052204-426856518
        public const int GetRoomRightsMessageEvent = 2772; // PRODUCTION-201709052204-426856518
        public const int SendMsgMessageEvent = 2083; // PRODUCTION-201709052204-426856518
        public const int CloseTicketMesageEvent = 3520; // PRODUCTION-201709052204-426856518


        /* NO ESTÁN EN ALASKA */

        public const int TourRequestEvent = 1581; // PRODUCTION-201709052204-426856518
        public const int EventLogMessageEvent = 734; // PRODUCTION-201709052204-426856518
        public const int PickRoomEvent = 1292; // PRODUCTION-201709052204-426856518
        public const int GuideSessionOnDutyUpdateMessageEvent = 540; // PRODUCTION-201709052204-426856518
        public const int GuideSessionIsTypingMessageEvent = 1777; // PRODUCTION-201709052204-426856518
        public const int PollRejectMessageEvent = 157; // PRODUCTION-201709052204-426856518
        public const int SetObjectDataMessageEvent = 2926; // PRODUCTION-201709052204-426856518
        public const int GuideSessionCreateMessageEvent = 351; // PRODUCTION-201709052204-426856518
        public const int ForumViewThreadButtonClickEvent = 972; // PRODUCTION-201709052204-426856518
        public const int SaveRoomThumbnailEvent = 2855; // PRODUCTION-201709052204-426856518
        public const int GetHabboClubCenterInfoMessageEvent = 1711; // PRODUCTION-201709052204-426856518
        public const int AmbassadorWarningMessageEvent = 2549; // PRODUCTION-201709052204-426856518
        public const int GuideSessionInviteRequesterMessageEvent = 1305; // PRODUCTION-201709052204-426856518
        public const int ApproveNameMessageEvent = 2794; // PRODUCTION-201709052204-426856518
        public const int CallForHelpMessageEvent = 2244; // PRODUCTION-201709052204-426856518
        public const int RoomDimmerSavePresetMessageEvent = 1203; // PRODUCTION-201709052204-426856518
        public const int PostQuizAnswersMessageEvent = 2210; // PRODUCTION-201709052204-426856518
        public const int PollStartMessageEvent = 2702; // PRODUCTION-201709052204-426856518
        public const int PerformanceLogMessageEvent = 661; // PRODUCTION-201709052204-426856518
        public const int GetMarketplaceOffersMessageEvent = 776; // PRODUCTION-201709052204-426856518
        public const int CallForHelpFromForumThreadMessageEvent = 1479; // PRODUCTION-201709052204-426856518
        public const int ModerationDefaultSanctionMessageEvent = 1572; // PRODUCTION-201709052204-426856518
        public const int CallForHelpFromIMMessageEvent = 2038; // PRODUCTION-201709052204-426856518
        public const int CommandBotMessageEvent = 909; // PRODUCTION-201709052204-426856518
        public const int SetCustomStackingHeightMessageEvent = 1513; // PRODUCTION-201709052204-426856518
        public const int GetClubOffersMessageEvent = 3031; // PRODUCTION-201709052204-426856518
        public const int UpdateFloorPropertiesMessageEvent = 1707; // PRODUCTION-201709052204-426856518
        public const int CallForHelpFromForumMessageMessageEvent = 223; // PRODUCTION-201709052204-426856518
        public const int PollAnswerMessageEvent = 1723; // PRODUCTION-201709052204-426856518
        public const int GetSoundSettingsMessageEvent = 3213; // PRODUCTION-201709052204-426856518
        public const int GuideSessionResolvedMessageEvent = 1136; // PRODUCTION-201709052204-426856518
        public const int InitializeNavigatorMessageEvent = 1217; // PRODUCTION-201709052204-426856518
        public const int GetExtendedProfileMessageEvent = 1058; // PRODUCTION-201709052204-426856518
        public const int GuideSessionMsgMessageEvent = 1307; // PRODUCTION-201709052204-426856518
        public const int VersionCheckMessageEvent = 3126; // PRODUCTION-201709052204-426856518
        public const int GuideSessionGetRequesterRoomMessageEvent = 2; // PRODUCTION-201709052204-426856518
        public const int SaveNavigatorPositionEvent = 3131; // PRODUCTION-201709052204-426856518
        public const int MySanctionStatusMessageEvent = 1015; // PRODUCTION-201709052204-426856518
        public const int GuideHelpMessageEvent = 1581; // PRODUCTION-201709052204-426856518
        public const int NewNavigatorSearchMessageEvent = 2456; // PRODUCTION-201709052204-426856518
        public const int GetGroupForumDataMessageEvent = 228; // PRODUCTION-201709052204-426856518
        public const int SetUsernameMessageEvent = 2645; // PRODUCTION-201709052204-426856518
        public const int GetGroupForumThreadRootMessageEvent = 1148; // PRODUCTION-201709052204-426856518
        public const int CheckQuizTypeEvent = 1920; // PRODUCTION-201709052204-426856518
        public const int ReadForumThreadMessageEvent = 2377; // PRODUCTION-201709052204-426856518
        public const int GetGroupForumsMessageEvent = 918; // PRODUCTION-201709052204-426856518
        public const int PublishForumThreadMessageEvent = 794; // PRODUCTION-201709052204-426856518
        public const int AlterForumThreadStateMessageEvent = 3609; // PRODUCTION-201709052204-426856518
        public const int NuxCloseMessageEvent = 863; // PRODUCTION-201709052204-426856518
        public const int CallGuide = 1581; // PRODUCTION-201709052204-426856518
        public const int AnswerGuideRequest = 351; // PRODUCTION-201709052204-426856518
        public const int OpenGuideTool = 540; // PRODUCTION-201709052204-426856518
        public const int GuideSpeak = 1307; // PRODUCTION-201709052204-426856518
        public const int InviteToRoom = 1305; // PRODUCTION-201709052204-426856518
        public const int VisitRoom = 2; // PRODUCTION-201709052204-426856518
        public const int CloseGuideRequest = 1136; // PRODUCTION-201709052204-426856518
        public const int CancelCallGuide = 25; // PRODUCTION-201709052204-426856518
        public const int BullyRequestAccept = 65; // PRODUCTION-201709052204-426856518
        public const int BullyRequestVote = 1402; // PRODUCTION-201709052204-426856518
        public const int BullyRequestNoVote = 501; // PRODUCTION-201709052204-426856518
        public const int VolverAmostrar = 1520; // PRODUCTION-201709052204-426856518
        public const int VolverAmostrar2 = 1777; // PRODUCTION-201709052204-426856518
        public const int AmbassadorAlert = 2549; // PRODUCTION-201709052204-426856518
        //to buy from targeted offers
        public const int TargetedOfferBuyEvent = 2089; // PRODUCTION-201709052204-426856518
        // Camera 2016
        public const int RenderRoomEvent = 362; // PRODUCTION-201709052204-426856518
        public const int PurchasePhotoEvent = 602; // PRODUCTION-201709052204-426856518
        public const int PublishPhotoEvent = 1554; // PRODUCTION-201709052204-426856518
        public const int GetCameraPriceEvent = 2680; // PRODUCTION-201709052204-426856518
        public const int ReportCameraPhotoEvent = 326; // PRODUCTION-201709052204-426856518
        public const int SetRoomThumbnailMessageEvent = 2855; // PRODUCTION-201709052204-426856518
    }
}
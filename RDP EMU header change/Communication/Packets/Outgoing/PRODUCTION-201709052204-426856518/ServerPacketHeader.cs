namespace Plus.Communication.Packets.Outgoing
{
    public static class ServerPacketHeader
    {
        // Handshake    /*Generated @ 10-9-2016 15:39:49 -- 289 packets For PRODUCTION201608171204*/
        public const int InitCryptoMessageComposer = 1233; // PRODUCTION-201709052204-426856518
        public const int SecretKeyMessageComposer = 1631; // PRODUCTION-201709052204-426856518
        public const int AuthenticationOKMessageComposer = 1294; // PRODUCTION-201709052204-426856518
        public const int UserObjectMessageComposer = 3231; // PRODUCTION-201709052204-426856518
        public const int UserPerksMessageComposer = 3877; // PRODUCTION-201709052204-426856518
        public const int UserRightsMessageComposer = 975; // PRODUCTION-201709052204-426856518
        public const int GenericErrorMessageComposer = 1329; // PRODUCTION-201709052204-426856518
        public const int SetUniqueIdMessageComposer = 226; // PRODUCTION-201709052204-426856518
        public const int AvailabilityStatusMessageComposer = 1312; // PRODUCTION-201709052204-426856518

        // Avatar
        public const int WardrobeMessageComposer = 1137; // PRODUCTION-201709052204-426856518

        // Catalog
        public const int CatalogIndexMessageComposer = 1222; // PRODUCTION-201709052204-426856518
        public const int CatalogItemDiscountMessageComposer = 2987; // PRODUCTION-201709052204-426856518
        public const int PurchaseOKMessageComposer = 2513; // PRODUCTION-201709052204-426856518
        public const int CatalogOfferMessageComposer = 2072; // PRODUCTION-201709052204-426856518
        public const int CatalogPageMessageComposer = 3316; // PRODUCTION-201709052204-426856518
        public const int CatalogUpdatedMessageComposer = 2253; // PRODUCTION-201709052204-426856518
        public const int SellablePetBreedsMessageComposer = 2240; // PRODUCTION-201709052204-426856518
        public const int GroupFurniConfigMessageComposer = 2525; // PRODUCTION-201709052204-426856518
        public const int PresentDeliverErrorMessageComposer = 1429; // PRODUCTION-201709052204-426856518

        // Quest
        public const int QuestListMessageComposer = 1827; // PRODUCTION-201709052204-426856518
        public const int QuestCompletedMessageComposer = 1950; // PRODUCTION-201709052204-426856518
        public const int QuestAbortedMessageComposer = 963; // PRODUCTION-201709052204-426856518
        public const int QuestStartedMessageComposer = 3891; // PRODUCTION-201709052204-426856518

        // Room Avatar
        public const int ActionMessageComposer = 3064; // PRODUCTION-201709052204-426856518
        public const int SleepMessageComposer = 2050; // PRODUCTION-201709052204-426856518
        public const int DanceMessageComposer = 1872; // PRODUCTION-201709052204-426856518
        public const int CarryObjectMessageComposer = 1755; // PRODUCTION-201709052204-426856518
        public const int AvatarEffectMessageComposer = 362; // PRODUCTION-201709052204-426856518

        // Room Chat
        public const int ChatMessageComposer = 1659; // PRODUCTION-201709052204-426856518
        public const int ShoutMessageComposer = 2765; // PRODUCTION-201709052204-426856518
        public const int WhisperMessageComposer = 1899; // PRODUCTION-201709052204-426856518
        public const int FloodControlMessageComposer = 1889; // PRODUCTION-201709052204-426856518
        public const int UserTypingMessageComposer = 3170; // PRODUCTION-201709052204-426856518

        // Room Engine
        public const int UsersMessageComposer = 1031; // PRODUCTION-201709052204-426856518
        public const int FurnitureAliasesMessageComposer = 29; // PRODUCTION-201709052204-426856518
        public const int ObjectAddMessageComposer = 824; // PRODUCTION-201709052204-426856518
        public const int ObjectsMessageComposer = 1147; // PRODUCTION-201709052204-426856518
        public const int ObjectUpdateMessageComposer = 2880; // PRODUCTION-201709052204-426856518
        public const int ObjectRemoveMessageComposer = 2993; // PRODUCTION-201709052204-426856518
        public const int SlideObjectBundleMessageComposer = 2561; // PRODUCTION-201709052204-426856518
        public const int ItemsMessageComposer = 877; // PRODUCTION-201709052204-426856518
        public const int ItemAddMessageComposer = 2251; // PRODUCTION-201709052204-426856518
        public const int ItemUpdateMessageComposer = 2582; // PRODUCTION-201709052204-426856518
        public const int ItemRemoveMessageComposer = 3762; // PRODUCTION-201709052204-426856518

        // Room Session
        public const int RoomForwardMessageComposer = 1048; // PRODUCTION-201709052204-426856518
        public const int RoomReadyMessageComposer = 1098; // PRODUCTION-201709052204-426856518
        public const int OpenConnectionMessageComposer = 3908; // PRODUCTION-201709052204-426856518
        public const int CloseConnectionMessageComposer = 2260; // PRODUCTION-201709052204-426856518
        public const int FlatAccessibleMessageComposer = 2557; // PRODUCTION-201709052204-426856518
        public const int CantConnectMessageComposer = 748; // PRODUCTION-201709052204-426856518

        // Room Permissions
        public const int YouAreControllerMessageComposer = 231; // PRODUCTION-201709052204-426856518
        public const int YouAreNotControllerMessageComposer = 3745; // PRODUCTION-201709052204-426856518
        public const int YouAreOwnerMessageComposer = 3976; // PRODUCTION-201709052204-426856518

        // Room Settings
        public const int RoomSettingsDataMessageComposer = 3133; // PRODUCTION-201709052204-426856518
        public const int RoomSettingsSavedMessageComposer = 1057; // PRODUCTION-201709052204-426856518
        public const int FlatControllerRemovedMessageComposer = 3470; // PRODUCTION-201709052204-426856518
        public const int FlatControllerAddedMessageComposer = 419; // PRODUCTION-201709052204-426856518
        public const int RoomRightsListMessageComposer = 1865; // PRODUCTION-201709052204-426856518

        // Room Furniture
        public const int HideWiredConfigMessageComposer = 3620; // PRODUCTION-201709052204-426856518
        public const int WiredEffectConfigMessageComposer = 3535; // PRODUCTION-201709052204-426856518
        public const int WiredConditionConfigMessageComposer = 1234; // PRODUCTION-201709052204-426856518
        public const int WiredTriggerConfigMessageComposer = 3175; // PRODUCTION-201709052204-426856518
        public const int MoodlightConfigMessageComposer = 2104; // PRODUCTION-201709052204-426856518
        public const int GroupFurniSettingsMessageComposer = 853; // PRODUCTION-201709052204-426856518
        public const int OpenGiftMessageComposer = 1090; // PRODUCTION-201709052204-426856518

        // Navigator
        public const int UpdateFavouriteRoomMessageComposer = 1261; // PRODUCTION-201709052204-426856518
        public const int NavigatorLiftedRoomsMessageComposer = 3580; // PRODUCTION-201709052204-426856518
        public const int NavigatorPreferencesMessageComposer = 735; // PRODUCTION-201709052204-426856518
        public const int NavigatorFlatCatsMessageComposer = 2144; // PRODUCTION-201709052204-426856518
        public const int NavigatorMetaDataParserMessageComposer = 3830; // PRODUCTION-201709052204-426856518
        public const int NavigatorCollapsedCategoriesMessageComposer = 966; // PRODUCTION-201709052204-426856518

        // Messenger
        public const int BuddyListMessageComposer = 758; // PRODUCTION-201709052204-426856518
        public const int BuddyRequestsMessageComposer = 1783; // PRODUCTION-201709052204-426856518
        public const int NewBuddyRequestMessageComposer = 3779; // PRODUCTION-201709052204-426856518

        // Moderation
        public const int ModeratorInitMessageComposer = 3781; // PRODUCTION-201709052204-426856518
        public const int ModeratorUserRoomVisitsMessageComposer = 161; // PRODUCTION-201709052204-426856518
        public const int ModeratorRoomChatlogMessageComposer = 2564; // PRODUCTION-201709052204-426856518
        public const int ModeratorUserInfoMessageComposer = 3375; // PRODUCTION-201709052204-426856518
        public const int ModeratorSupportTicketResponseMessageComposer = 1212; // PRODUCTION-201709052204-426856518
        public const int ModeratorUserChatlogMessageComposer = 583; // PRODUCTION-201709052204-426856518
        public const int ModeratorRoomInfoMessageComposer = 467; // PRODUCTION-201709052204-426856518
        public const int ModeratorSupportTicketMessageComposer = 2027; // PRODUCTION-201709052204-426856518
        public const int ModeratorTicketChatlogMessageComposer = 935; // PRODUCTION-201709052204-426856518
        public const int CallForHelpPendingCallsMessageComposer = 1733; // PRODUCTION-201709052204-426856518
        public const int CfhTopicsInitMessageComposer = 1762; // PRODUCTION-201709052204-426856518

        // Inventory
        public const int CreditBalanceMessageComposer = 1662; // PRODUCTION-201709052204-426856518
        public const int BadgesMessageComposer = 2220; // PRODUCTION-201709052204-426856518
        public const int FurniListAddMessageComposer = 466; // PRODUCTION-201709052204-426856518
        public const int FurniListNotificationMessageComposer = 700; // PRODUCTION-201709052204-426856518
        public const int FurniListRemoveMessageComposer = 2278; // PRODUCTION-201709052204-426856518
        public const int FurniListMessageComposer = 1307; // PRODUCTION-201709052204-426856518
        public const int FurniListUpdateMessageComposer = 1521; // PRODUCTION-201709052204-426856518
        public const int AvatarEffectsMessageComposer = 350; // PRODUCTION-201709052204-426856518
        public const int AvatarEffectActivatedMessageComposer = 2642; // PRODUCTION-201709052204-426856518
        public const int AvatarEffectExpiredMessageComposer = 929; // PRODUCTION-201709052204-426856518
        public const int AvatarEffectAddedMessageComposer = 1137; // PRODUCTION-201709052204-426856518
        public const int TradingErrorMessageComposer = 962; // PRODUCTION-201709052204-426856518
        public const int TradingAcceptMessageComposer = 3467; // PRODUCTION-201709052204-426856518
        public const int TradingStartMessageComposer = 372; // PRODUCTION-201709052204-426856518
        public const int TradingUpdateMessageComposer = 2364; // PRODUCTION-201709052204-426856518
        public const int TradingClosedMessageComposer = 2911; // PRODUCTION-201709052204-426856518
        public const int TradingCompleteMessageComposer = 2849; // PRODUCTION-201709052204-426856518
        public const int TradingConfirmedMessageComposer = 3467; // PRODUCTION-201709052204-426856518
        public const int TradingFinishMessageComposer = 1027; // PRODUCTION-201709052204-426856518

        // Inventory Achievements
        public const int AchievementsMessageComposer = 1658; // PRODUCTION-201709052204-426856518
        public const int AchievementScoreMessageComposer = 3221; // PRODUCTION-201709052204-426856518
        public const int AchievementUnlockedMessageComposer = 811; // PRODUCTION-201709052204-426856518
        public const int AchievementProgressedMessageComposer = 2098; // PRODUCTION-201709052204-426856518

        // Notifications
        public const int ActivityPointsMessageComposer = 992; // PRODUCTION-201709052204-426856518
        public const int HabboActivityPointNotificationMessageComposer = 1546; // PRODUCTION-201709052204-426856518

        // Users
        public const int ScrSendUserInfoMessageComposer = 1925; // PRODUCTION-201709052204-426856518
        public const int IgnoredUsersMessageComposer = 2074; // PRODUCTION-201709052204-426856518

        //Groups
        public const int UnknownGroupMessageComposer = 1309; // PRODUCTION-201709052204-426856518
        public const int GroupMembershipRequestedMessageComposer = 1576; // PRODUCTION-201709052204-426856518
        public const int ManageGroupMessageComposer = 991; // PRODUCTION-201709052204-426856518
        public const int HabboGroupBadgesMessageComposer = 84; // PRODUCTION-201709052204-426856518
        public const int NewGroupInfoMessageComposer = 2197; // PRODUCTION-201709052204-426856518
        public const int GroupInfoMessageComposer = 1530; // PRODUCTION-201709052204-426856518
        public const int GroupCreationWindowMessageComposer = 2815; // PRODUCTION-201709052204-426856518
        public const int SetGroupIdMessageComposer = 3437; // PRODUCTION-201709052204-426856518
        public const int GroupMembersMessageComposer = 3602; // PRODUCTION-201709052204-426856518
        public const int UpdateFavouriteGroupMessageComposer = 3293; // PRODUCTION-201709052204-426856518
        public const int GroupMemberUpdatedMessageComposer = 2896; // PRODUCTION-201709052204-426856518
        public const int RefreshFavouriteGroupMessageComposer = 3611; // PRODUCTION-201709052204-426856518

        // Group Forums
        public const int ForumsListDataMessageComposer = 2054; // PRODUCTION-201709052204-426856518
        public const int ForumDataMessageComposer = 1331; // PRODUCTION-201709052204-426856518
        public const int ThreadCreatedMessageComposer = 306; // PRODUCTION-201709052204-426856518
        public const int ThreadDataMessageComposer = 3183; // PRODUCTION-201709052204-426856518
        public const int ThreadsListDataMessageComposer = 1501; // PRODUCTION-201709052204-426856518
        public const int ThreadUpdatedMessageComposer = 2265; // PRODUCTION-201709052204-426856518
        public const int ThreadReplyMessageComposer = 2406; // PRODUCTION-201709052204-426856518

        // Sound
        public const int SoundSettingsMessageComposer = 903; // PRODUCTION-201709052204-426856518

        public const int QuestionParserMessageComposer = 2571; // PRODUCTION-201709052204-426856518
        public const int AvatarAspectUpdateMessageComposer = 125; // PRODUCTION-201709052204-426856518
        // NUEVO public const int HelperToolMessageComposer =-1;//error 404
        public const int RoomErrorNotifMessageComposer = 415; // PRODUCTION-201709052204-426856518
        public const int FollowFriendFailedMessageComposer = 1157; // PRODUCTION-201709052204-426856518

        public const int FindFriendsProcessResultMessageComposer = 1079; // PRODUCTION-201709052204-426856518
        public const int UserChangeMessageComposer = 50; // PRODUCTION-201709052204-426856518
        public const int FloorHeightMapMessageComposer = 2100; // PRODUCTION-201709052204-426856518
        public const int RoomInfoUpdatedMessageComposer = 3246; // PRODUCTION-201709052204-426856518
        public const int MessengerErrorMessageComposer = 3143; // PRODUCTION-201709052204-426856518
        public const int MarketplaceCanMakeOfferResultMessageComposer = 1988; // PRODUCTION-201709052204-426856518
        public const int GameAccountStatusMessageComposer = 773; // PRODUCTION-201709052204-426856518
        public const int GuestRoomSearchResultMessageComposer = 762; // PRODUCTION-201709052204-426856518
        public const int NewUserExperienceGiftOfferMessageComposer = 1223; // PRODUCTION-201709052204-426856518
        public const int UpdateUsernameMessageComposer = 2266; // PRODUCTION-201709052204-426856518
        public const int VoucherRedeemOkMessageComposer = 2462; // PRODUCTION-201709052204-426856518
        public const int FigureSetIdsMessageComposer = 2837; // PRODUCTION-201709052204-426856518
        public const int StickyNoteMessageComposer = 104; // PRODUCTION-201709052204-426856518
        public const int UserRemoveMessageComposer = 2756; // PRODUCTION-201709052204-426856518
        public const int GetGuestRoomResultMessageComposer = 836; // PRODUCTION-201709052204-426856518
        public const int DoorbellMessageComposer = 698; // PRODUCTION-201709052204-426856518

        public const int GiftWrappingConfigurationMessageComposer = 3419; // PRODUCTION-201709052204-426856518
        public const int GetRelationshipsMessageComposer = 246; // PRODUCTION-201709052204-426856518
        public const int FriendNotificationMessageComposer = 2183; // PRODUCTION-201709052204-426856518
        public const int BadgeEditorPartsMessageComposer = 2910; // PRODUCTION-201709052204-426856518
        public const int TraxSongInfoMessageComposer = 182; // PRODUCTION-201709052204-426856518
        public const int PostUpdatedMessageComposer = 2479; // PRODUCTION-201709052204-426856518
        public const int UserUpdateMessageComposer = 2241; // PRODUCTION-201709052204-426856518
        public const int MutedMessageComposer = 1671; // PRODUCTION-201709052204-426856518
        public const int MarketplaceConfigurationMessageComposer = 478; // PRODUCTION-201709052204-426856518
        public const int CheckGnomeNameMessageComposer = 572; // PRODUCTION-201709052204-426856518
        public const int OpenBotActionMessageComposer = 2343; // PRODUCTION-201709052204-426856518
        public const int FavouritesMessageComposer = 1422; // PRODUCTION-201709052204-426856518
        public const int TalentLevelUpMessageComposer = 2063; // PRODUCTION-201709052204-426856518

        public const int BCBorrowedItemsMessageComposer = 554; // PRODUCTION-201709052204-426856518
        public const int UserTagsMessageComposer = 2274; // PRODUCTION-201709052204-426856518
        public const int CampaignMessageComposer = 1621; // PRODUCTION-201709052204-426856518
        public const int RoomEventMessageComposer = 2725; // PRODUCTION-201709052204-426856518
        public const int MarketplaceItemStatsMessageComposer = 1776; // PRODUCTION-201709052204-426856518
        public const int HabboSearchResultMessageComposer = 3272; // PRODUCTION-201709052204-426856518
        public const int PetHorseFigureInformationMessageComposer = 1845; // PRODUCTION-201709052204-426856518
        public const int PetInventoryMessageComposer = 78; // PRODUCTION-201709052204-426856518
        public const int PongMessageComposer = 3101; // PRODUCTION-201709052204-426856518
        public const int RentableSpaceMessageComposer = 66; // PRODUCTION-201709052204-426856518
        public const int GetYouTubePlaylistMessageComposer = 3653; // PRODUCTION-201709052204-426856518
        public const int RespectNotificationMessageComposer = 1785; // PRODUCTION-201709052204-426856518
        public const int RecyclerRewardsMessageComposer = 1916; // PRODUCTION-201709052204-426856518
        public const int GetRoomBannedUsersMessageComposer = 3521; // PRODUCTION-201709052204-426856518
        public const int RoomRatingMessageComposer = 2019; // PRODUCTION-201709052204-426856518
        public const int PlayableGamesMessageComposer = 3525; // PRODUCTION-201709052204-426856518
        public const int TalentTrackLevelMessageComposer = 3655; // PRODUCTION-201709052204-426856518
        public const int JoinQueueMessageComposer = 3674; // PRODUCTION-201709052204-426856518
        public const int MarketPlaceOwnOffersMessageComposer = 88; // PRODUCTION-201709052204-426856518
        public const int PetBreedingMessageComposer = 746; // PRODUCTION-201709052204-426856518
        public const int SubmitBullyReportMessageComposer = 3743; // PRODUCTION-201709052204-426856518
        public const int UserNameChangeMessageComposer = 1568; // PRODUCTION-201709052204-426856518
        public const int LoveLockDialogueCloseMessageComposer = 3484; // PRODUCTION-201709052204-426856518
        public const int SendBullyReportMessageComposer = 2488; // PRODUCTION-201709052204-426856518
        public const int VoucherRedeemErrorMessageComposer = 2650; // PRODUCTION-201709052204-426856518
        public const int PurchaseErrorMessageComposer = 708; // PRODUCTION-201709052204-426856518
        public const int UnknownCalendarMessageComposer = 540; // PRODUCTION-201709052204-426856518
        public const int FriendListUpdateMessageComposer = 1382; // PRODUCTION-201709052204-426856518

        public const int UserFlatCatsMessageComposer = 845; // PRODUCTION-201709052204-426856518
        public const int UpdateFreezeLivesMessageComposer = 581; // PRODUCTION-201709052204-426856518
        public const int UnbanUserFromRoomMessageComposer = 2945; // PRODUCTION-201709052204-426856518
        public const int PetTrainingPanelMessageComposer = 3044; // PRODUCTION-201709052204-426856518
        public const int LoveLockDialogueMessageComposer = 3884; // PRODUCTION-201709052204-426856518
        public const int BuildersClubMembershipMessageComposer = 1505; // PRODUCTION-201709052204-426856518
        public const int FlatAccessDeniedMessageComposer = 3344; // PRODUCTION-201709052204-426856518
        public const int LatencyResponseMessageComposer = 2485; // PRODUCTION-201709052204-426856518
        public const int HabboUserBadgesMessageComposer = 1185; // PRODUCTION-201709052204-426856518
        public const int HeightMapMessageComposer = 3973; // PRODUCTION-201709052204-426856518

        public const int CanCreateRoomMessageComposer = 2221; // PRODUCTION-201709052204-426856518
        public const int InstantMessageErrorMessageComposer = 1070; // PRODUCTION-201709052204-426856518
        public const int GnomeBoxMessageComposer = 3189; // PRODUCTION-201709052204-426856518
        public const int IgnoreStatusMessageComposer = 697; // PRODUCTION-201709052204-426856518
        public const int PetInformationMessageComposer = 1570; // PRODUCTION-201709052204-426856518
        public const int NavigatorSearchResultSetMessageComposer = 1036; // PRODUCTION-201709052204-426856518
        public const int ConcurrentUsersGoalProgressMessageComposer = 3097; // PRODUCTION-201709052204-426856518
        public const int VideoOffersRewardsMessageComposer = 3458; // PRODUCTION-201709052204-426856518
        public const int SanctionStatusMessageComposer = 1745; // PRODUCTION-201709052204-426856518
        public const int GetYouTubeVideoMessageComposer = 1955; // PRODUCTION-201709052204-426856518
        public const int CheckPetNameMessageComposer = 2599; // PRODUCTION-201709052204-426856518
        public const int RespectPetNotificationMessageComposer = 3577; // PRODUCTION-201709052204-426856518
        public const int EnforceCategoryUpdateMessageComposer = 3519; // PRODUCTION-201709052204-426856518
        public const int CommunityGoalHallOfFameMessageComposer =-1;//error 404
        public const int FloorPlanFloorMapMessageComposer = 2151; // PRODUCTION-201709052204-426856518
        public const int SendGameInvitationMessageComposer = 1738; // PRODUCTION-201709052204-426856518
        public const int GiftWrappingErrorMessageComposer = 2041; // PRODUCTION-201709052204-426856518
        public const int PromoArticlesMessageComposer = 3845; // PRODUCTION-201709052204-426856518
        public const int Game1WeeklyLeaderboardMessageComposer = 345; // PRODUCTION-201709052204-426856518
        public const int RentableSpacesErrorMessageComposer = 2919; // PRODUCTION-201709052204-426856518
        public const int AddExperiencePointsMessageComposer = 1139; // PRODUCTION-201709052204-426856518
        public const int OpenHelpToolMessageComposer = 1733; // PRODUCTION-201709052204-426856518
        public const int GetRoomFilterListMessageComposer = 3297; // PRODUCTION-201709052204-426856518
        public const int GameAchievementListMessageComposer = 1711; // PRODUCTION-201709052204-426856518
        public const int PromotableRoomsMessageComposer = 3698; // PRODUCTION-201709052204-426856518
        public const int FloorPlanSendDoorMessageComposer = 1716; // PRODUCTION-201709052204-426856518
        public const int RoomEntryInfoMessageComposer = 2147; // PRODUCTION-201709052204-426856518
        public const int RoomNotificationMessageComposer = 2500; // PRODUCTION-201709052204-426856518
        public const int ClubGiftsMessageComposer =-1;//error 404
        public const int MOTDNotificationMessageComposer = 54; // PRODUCTION-201709052204-426856518
        public const int PopularRoomTagsResultMessageComposer = 2679; // PRODUCTION-201709052204-426856518
        public const int NewConsoleMessageMessageComposer = 3834; // PRODUCTION-201709052204-426856518
        public const int RoomPropertyMessageComposer = 2558; // PRODUCTION-201709052204-426856518
        public const int MarketPlaceOffersMessageComposer = 886; // PRODUCTION-201709052204-426856518
        public const int TalentTrackMessageComposer = 1512; // PRODUCTION-201709052204-426856518
        public const int ProfileInformationMessageComposer = 3415; // PRODUCTION-201709052204-426856518
        public const int BadgeDefinitionsMessageComposer = 582; // PRODUCTION-201709052204-426856518
        public const int Game2WeeklyLeaderboardMessageComposer = 345; // PRODUCTION-201709052204-426856518
        public const int NameChangeUpdateMessageComposer = 3319; // PRODUCTION-201709052204-426856518
        public const int RoomVisualizationSettingsMessageComposer = 3997; // PRODUCTION-201709052204-426856518
        
        public const int FlatCreatedMessageComposer = 912; // PRODUCTION-201709052204-426856518
        public const int BotInventoryMessageComposer = 1072; // PRODUCTION-201709052204-426856518
        public const int LoadGameMessageComposer = 3747; // PRODUCTION-201709052204-426856518
        public const int UpdateMagicTileMessageComposer = 3857; // PRODUCTION-201709052204-426856518
        public const int CampaignCalendarDataMessageComposer = 906; // PRODUCTION-201709052204-426856518
        public const int MaintenanceStatusMessageComposer = 609; // PRODUCTION-201709052204-426856518
        public const int Game3WeeklyLeaderboardMessageComposer = 345; // PRODUCTION-201709052204-426856518
        public const int GameListMessageComposer = 3824; // PRODUCTION-201709052204-426856518
        public const int RoomMuteSettingsMessageComposer = 2243; // PRODUCTION-201709052204-426856518
        public const int RoomInviteMessageComposer = 2983; // PRODUCTION-201709052204-426856518
        public const int LoveLockDialogueSetLockedMessageComposer = 3484; // PRODUCTION-201709052204-426856518
        public const int BroadcastMessageAlertMessageComposer = 777; // PRODUCTION-201709052204-426856518
        public const int MarketplaceCancelOfferResultMessageComposer = 2091; // PRODUCTION-201709052204-426856518
        public const int NavigatorSettingsMessageComposer = 3503; // PRODUCTION-201709052204-426856518

        public const int MessengerInitMessageComposer = 3160; // PRODUCTION-201709052204-426856518

        //polls
        // NUEVO public const int PollAnswerComposer =-1;//error 404


        /* NO ESTÁN EN ALASKA */
        public const int ServerErrorMessageComposer = 220; // PRODUCTION-201709052204-426856518
        public const int CameraPurchaseOkComposer = 3273; // PRODUCTION-201709052204-426856518
        public const int HabboClubOffersMessageComposer = 3847; // PRODUCTION-201709052204-426856518
        
        public const int QuizDataMessageComposer = 450; // PRODUCTION-201709052204-426856518
        public const int PollContentsMessageComposer = 3307; // PRODUCTION-201709052204-426856518
        
        public const int UserGameAchievementsMessageComposer = 1711; // PRODUCTION-201709052204-426856518
        public const int ModeratorUserClassMessageComposer = 3833; // PRODUCTION-201709052204-426856518
        public const int UniqueMachineIDMessageComposer = 226; // PRODUCTION-201709052204-426856518
        public const int GroupForumDataMessageComposer = 1331; // PRODUCTION-201709052204-426856518
        public const int GroupDeletedMessageComposer = 2546; // PRODUCTION-201709052204-426856518
        public const int YoutubeDisplayPlaylistsMessageComposer = 3653; // PRODUCTION-201709052204-426856518
        public const int MarketplaceMakeOfferResultMessageComposer = 1390; // PRODUCTION-201709052204-426856518
        public const int MuteAllInRoomMessageComposer = 2243; // PRODUCTION-201709052204-426856518
        public const int FigureUpdateMessageComposer = 125; // PRODUCTION-201709052204-426856518
        public const int PollOfferMessageComposer = 1190; // PRODUCTION-201709052204-426856518
        public const int ThumbnailSuccessMessageComposer = 3800; // PRODUCTION-201709052204-426856518
        public const int PostQuizAnswersMessageComposer = 2626; // PRODUCTION-201709052204-426856518
        public const int GuideSessionRequesterRoomMessageComposer = 1338; // PRODUCTION-201709052204-426856518
        public const int HallOfFameMessageComposer = 761; // PRODUCTION-201709052204-426856518
        
        
        public const int CameraStorageUrlMessageComposer = 1654; // PRODUCTION-201709052204-426856518
        public const int PerkAllowancesMessageComposer = 3877; // PRODUCTION-201709052204-426856518
        public const int YouTubeDisplayVideoMessageComposer = 1955; // PRODUCTION-201709052204-426856518
        public const int QuestionAnswersMessageComposer = 692; // PRODUCTION-201709052204-426856518
        public const int EndPollMessageComposer = 3713; // PRODUCTION-201709052204-426856518
        public const int SetCameraPriceMessageComposer = 3092; // PRODUCTION-201709052204-426856518
        public const int GuideSessionEndedMessageComposer = 2747; // PRODUCTION-201709052204-426856518
        
        public const int CampaignCalendarGiftMessageComposer =-1;//error 404
        public const int HabboClubCenterInfoMessageComposer = 3860; // PRODUCTION-201709052204-426856518
        public const int QuizResultsMessageComposer = 2626; // PRODUCTION-201709052204-426856518
        public const int GroupForumNewThreadMessageComposer = 306; // PRODUCTION-201709052204-426856518
        public const int GroupForumReadThreadMessageComposer = 3183; // PRODUCTION-201709052204-426856518
        public const int GroupForumListingsMessageComposer = 2054; // PRODUCTION-201709052204-426856518
        public const int GetClubComposer = 3847; // PRODUCTION-201709052204-426856518
        public const int GroupForumThreadUpdateMessageComposer = 2265; // PRODUCTION-201709052204-426856518
        public const int GroupForumNewResponseMessageComposer = 2406; // PRODUCTION-201709052204-426856518
        public const int GroupForumThreadRootMessageComposer = 1501; // PRODUCTION-201709052204-426856518
        
        
        public const int SuperNotificationMessageComposer = 2500; // PRODUCTION-201709052204-426856518
        public const int NuxUserStatus = 1054; // PRODUCTION-201709052204-426856518
        
        public const int HelperToolConfigurationMessageComposer = 3757; // PRODUCTION-201709052204-426856518
        public const int OnGuideSessionStartedMessageComposer = 3601; // PRODUCTION-201709052204-426856518
        public const int OnGuideSessionPartnerIsTypingMessageComposer = 3523; // PRODUCTION-201709052204-426856518
        public const int OnGuideSessionMsgMessageComposer = 941; // PRODUCTION-201709052204-426856518
        public const int OnGuideSessionInvitedToGuideRoomMessageComposer = 3267; // PRODUCTION-201709052204-426856518
        public const int OnGuideSessionAttachedMessageComposer = 3093; // PRODUCTION-201709052204-426856518
        public const int OnGuideSessionDetachedMessageComposer = 2116; // PRODUCTION-201709052204-426856518
        public const int OnGuideSessionError = 3585; // PRODUCTION-201709052204-426856518
        public const int Tool2 = 3420; // PRODUCTION-201709052204-426856518
        //new alert with only a link
        public const int NuxAlertMessageComposer = 3445; // PRODUCTION-201709052204-426856518
        public const int TargetedOfferComposer = 254; // PRODUCTION-201709052204-426856518
        public const int NuxNotificationMessageComposer = 3445; // PRODUCTION-201709052204-426856518
        
        //Camera 2016 PRODUCTION 201607
        public const int CameraPhotoPreviewComposer = 1654; // PRODUCTION-201709052204-426856518
        public const int CameraPhotoPurchaseOkComposer = 3273; // PRODUCTION-201709052204-426856518
        public const int CameraPriceComposer = 3092; // PRODUCTION-201709052204-426856518
        public const int SendRoomThumbnailAlertMessageComposer = 3800; // PRODUCTION-201709052204-426856518

    }
}
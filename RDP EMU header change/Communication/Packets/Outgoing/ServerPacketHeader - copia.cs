namespace Plus.Communication.Packets.Outgoing
{
    public static class ServerPacketHeader
    {
        // Handshake    /*Generated @ 10-9-2016 15:39:49 -- 289 packets For PRODUCTION201608171204*/
        public const int InitCryptoMessageComposer = 3891;
        public const int SecretKeyMessageComposer = 259;
        public const int AuthenticationOKMessageComposer = 742;
        public const int UserObjectMessageComposer = 3966;
        public const int UserPerksMessageComposer = 441;
        public const int UserRightsMessageComposer = 3419;
        public const int GenericErrorMessageComposer = 1298;
        public const int SetUniqueIdMessageComposer = 2747;
        public const int AvailabilityStatusMessageComposer = 562;

        // Avatar
        public const int WardrobeMessageComposer = 1208;

        // Catalog
        public const int CatalogIndexMessageComposer = 1609;
        public const int CatalogItemDiscountMessageComposer = 1047;
        public const int PurchaseOKMessageComposer = 1086;
        public const int CatalogOfferMessageComposer = 2632;
        public const int CatalogPageMessageComposer = 1013;
        public const int CatalogUpdatedMessageComposer = 1038;
        public const int SellablePetBreedsMessageComposer = 164;
        public const int GroupFurniConfigMessageComposer = 1552;
        public const int PresentDeliverErrorMessageComposer = 918;

        // Quest
        public const int QuestListMessageComposer = 1285;
        public const int QuestCompletedMessageComposer = 3996;
        public const int QuestAbortedMessageComposer = 31;
        public const int QuestStartedMessageComposer = 343;

        // Room Avatar
        public const int ActionMessageComposer = 41;
        public const int SleepMessageComposer = 352;
        public const int DanceMessageComposer = 1226;
        public const int CarryObjectMessageComposer = 1551;
        public const int AvatarEffectMessageComposer = 3604;

        // Room Chat
        public const int ChatMessageComposer = 3956;
        public const int ShoutMessageComposer = 3080;
        public const int WhisperMessageComposer = 1984;
        public const int FloodControlMessageComposer = 2140;
        public const int UserTypingMessageComposer = 3202;

        // Room Engine
        public const int UsersMessageComposer = 1154;
        public const int FurnitureAliasesMessageComposer = 3998;
        public const int ObjectAddMessageComposer = 1276;
        public const int ObjectsMessageComposer = 530;
        public const int ObjectUpdateMessageComposer = 2017;
        public const int ObjectRemoveMessageComposer = 1265;
        public const int SlideObjectBundleMessageComposer = 644;
        public const int ItemsMessageComposer = 2713;
        public const int ItemAddMessageComposer = 2255;
        public const int ItemUpdateMessageComposer = 1332;
        public const int ItemRemoveMessageComposer = 1570;

        // Room Session
        public const int RoomForwardMessageComposer = 1079;
        public const int RoomReadyMessageComposer = 3411;
        public const int OpenConnectionMessageComposer = 3500;
        public const int CloseConnectionMessageComposer = 1976;        
        public const int FlatAccessibleMessageComposer = 3992;
        public const int CantConnectMessageComposer = 3095;

        // Room Permissions
        public const int YouAreControllerMessageComposer = 3888;
        public const int YouAreNotControllerMessageComposer = 830;
        public const int YouAreOwnerMessageComposer = 3441;

        // Room Settings
        public const int RoomSettingsDataMessageComposer = 790;
        public const int RoomSettingsSavedMessageComposer = 1794;
        public const int FlatControllerRemovedMessageComposer = 0;
        public const int FlatControllerAddedMessageComposer = 1586;
        public const int RoomRightsListMessageComposer = 2936;

        // Room Furniture
        public const int HideWiredConfigMessageComposer = 3343;
        public const int WiredEffectConfigMessageComposer = 455;
        public const int WiredConditionConfigMessageComposer = 58;
        public const int WiredTriggerConfigMessageComposer = 3579;
        public const int MoodlightConfigMessageComposer = 2574;
        public const int GroupFurniSettingsMessageComposer = 2837;
        public const int OpenGiftMessageComposer = 2280;

        // Navigator
        public const int UpdateFavouriteRoomMessageComposer = 3661;
        public const int NavigatorLiftedRoomsMessageComposer = 357;
        public const int NavigatorPreferencesMessageComposer = 272;
        public const int NavigatorFlatCatsMessageComposer = 1219;
        public const int NavigatorMetaDataParserMessageComposer = 753;
        public const int NavigatorCollapsedCategoriesMessageComposer = 3802;

        // Messenger
        public const int BuddyListMessageComposer = 1353;
        public const int BuddyRequestsMessageComposer = 1713;
        public const int NewBuddyRequestMessageComposer = 3140;

        // Moderation
        public const int ModeratorInitMessageComposer = 2464;
        public const int ModeratorUserRoomVisitsMessageComposer = 768;
        public const int ModeratorRoomChatlogMessageComposer = 2132;
        public const int ModeratorUserInfoMessageComposer = 2711;
        public const int ModeratorSupportTicketResponseMessageComposer = 642;
        public const int ModeratorUserChatlogMessageComposer = 3179;
        public const int ModeratorRoomInfoMessageComposer = 3380;
        public const int ModeratorSupportTicketMessageComposer = 2667;
        public const int ModeratorTicketChatlogMessageComposer = 3900;
        public const int CallForHelpPendingCallsMessageComposer = 2166;
        public const int CfhTopicsInitMessageComposer = 2605;

        // Inventory
        public const int CreditBalanceMessageComposer = 2509;
        public const int BadgesMessageComposer = 998;
        public const int FurniListAddMessageComposer = 3736;
        public const int FurniListNotificationMessageComposer = 2277;
        public const int FurniListRemoveMessageComposer = 1579;        
        public const int FurniListMessageComposer = 2264;
        public const int FurniListUpdateMessageComposer = 1623;
        public const int AvatarEffectsMessageComposer = 1186;
        public const int AvatarEffectActivatedMessageComposer = 1618;
        public const int AvatarEffectExpiredMessageComposer = 1717;
        public const int AvatarEffectAddedMessageComposer = 1208;
        public const int TradingErrorMessageComposer = 602;
        public const int TradingAcceptMessageComposer = 1243;
        public const int TradingStartMessageComposer = 1959;
        public const int TradingUpdateMessageComposer = 739;
        public const int TradingClosedMessageComposer = 3774;
        public const int TradingCompleteMessageComposer = 1084;
        public const int TradingConfirmedMessageComposer = 1243;
        public const int TradingFinishMessageComposer = 1889;

        // Inventory Achievements
        public const int AchievementsMessageComposer = 1171;
        public const int AchievementScoreMessageComposer = 2350;
        public const int AchievementUnlockedMessageComposer = 880;
        public const int AchievementProgressedMessageComposer = 2776;

        // Notifications
        public const int ActivityPointsMessageComposer = 1337;
        public const int HabboActivityPointNotificationMessageComposer = 146;

        // Users
        public const int ScrSendUserInfoMessageComposer = 1210;
        public const int IgnoredUsersMessageComposer = 225;

        //Groups
        public const int UnknownGroupMessageComposer = 3578;
        public const int GroupMembershipRequestedMessageComposer = 3020;
        public const int ManageGroupMessageComposer = 744;
        public const int HabboGroupBadgesMessageComposer = 1101;
        public const int NewGroupInfoMessageComposer = 2407;
        public const int GroupInfoMessageComposer = 951;
        public const int GroupCreationWindowMessageComposer = 509;
        public const int SetGroupIdMessageComposer = 2436;
        public const int GroupMembersMessageComposer = 2477;
        public const int UpdateFavouriteGroupMessageComposer = 896;
        public const int GroupMemberUpdatedMessageComposer = 2489;
        public const int RefreshFavouriteGroupMessageComposer = 3250;

        // Group Forums
        public const int ForumsListDataMessageComposer = 757;
        public const int ForumDataMessageComposer = 2857;
        public const int ThreadCreatedMessageComposer = 1002;
        public const int ThreadDataMessageComposer = 3887;
        public const int ThreadsListDataMessageComposer = 2020;
        public const int ThreadUpdatedMessageComposer = 2339;
        public const int ThreadReplyMessageComposer = 255;

        // Sound
        public const int SoundSettingsMessageComposer = 907;

        public const int QuestionParserMessageComposer = 3893; //make question
        public const int AvatarAspectUpdateMessageComposer = 2439;
        // NUEVO public const int HelperToolMessageComposer = 3757;
        public const int RoomErrorNotifMessageComposer = 3313;
        public const int FollowFriendFailedMessageComposer = 685;

        public const int FindFriendsProcessResultMessageComposer = 2982;
        public const int UserChangeMessageComposer = 2534;
        public const int FloorHeightMapMessageComposer = 2389;
        public const int RoomInfoUpdatedMessageComposer = 658;
        public const int MessengerErrorMessageComposer = 3884;
        public const int MarketplaceCanMakeOfferResultMessageComposer = 1291;
        public const int GameAccountStatusMessageComposer = 3834;
        public const int GuestRoomSearchResultMessageComposer = 1373;
        public const int NewUserExperienceGiftOfferMessageComposer = 228;
        public const int UpdateUsernameMessageComposer = 3525;
        public const int VoucherRedeemOkMessageComposer = 1315;
        public const int FigureSetIdsMessageComposer = 2001;
        public const int StickyNoteMessageComposer = 3917;
        public const int UserRemoveMessageComposer = 1745;
        public const int GetGuestRoomResultMessageComposer = 925;
        public const int DoorbellMessageComposer = 2610;

        public const int GiftWrappingConfigurationMessageComposer = 3444;
        public const int GetRelationshipsMessageComposer = 1577;
        public const int FriendNotificationMessageComposer = 84;
        public const int BadgeEditorPartsMessageComposer = 931;
        public const int TraxSongInfoMessageComposer = 449;
        public const int PostUpdatedMessageComposer = 1295;
        public const int UserUpdateMessageComposer = 2404;
        public const int MutedMessageComposer = 3510;
        public const int MarketplaceConfigurationMessageComposer = 648;
        public const int CheckGnomeNameMessageComposer = 1087;
        public const int OpenBotActionMessageComposer = 1687;
        public const int FavouritesMessageComposer = 1044;
        public const int TalentLevelUpMessageComposer = 3753;

        public const int BCBorrowedItemsMessageComposer = 762;
        public const int UserTagsMessageComposer = 1880;
        public const int CampaignMessageComposer = 3241;
        public const int RoomEventMessageComposer = 1466;
        public const int MarketplaceItemStatsMessageComposer = 3212;
        public const int HabboSearchResultMessageComposer = 1248;
        public const int PetHorseFigureInformationMessageComposer = 3953;
        public const int PetInventoryMessageComposer = 2565;
        public const int PongMessageComposer = 3878;
        public const int RentableSpaceMessageComposer = 1825;
        public const int GetYouTubePlaylistMessageComposer = 2638;
        public const int RespectNotificationMessageComposer = 1509;
        public const int RecyclerRewardsMessageComposer = 2720;
        public const int GetRoomBannedUsersMessageComposer = 3327;
        public const int RoomRatingMessageComposer = 99;
        public const int PlayableGamesMessageComposer = 3268;
        public const int TalentTrackLevelMessageComposer = 425;
        public const int JoinQueueMessageComposer = 2800;
        public const int MarketPlaceOwnOffersMessageComposer = 2067;
        public const int PetBreedingMessageComposer = 1006;
        public const int SubmitBullyReportMessageComposer = 2183;
        public const int UserNameChangeMessageComposer = 3872;
        public const int LoveLockDialogueCloseMessageComposer = 271;
        public const int SendBullyReportMessageComposer = 2074;
        public const int VoucherRedeemErrorMessageComposer = 2207;
        public const int PurchaseErrorMessageComposer = 1318;
        public const int UnknownCalendarMessageComposer = 1585;
        public const int FriendListUpdateMessageComposer = 1691;

        public const int UserFlatCatsMessageComposer = 2833;
        public const int UpdateFreezeLivesMessageComposer = 477;
        public const int UnbanUserFromRoomMessageComposer = 2807;
        public const int PetTrainingPanelMessageComposer = 3121;
        public const int LoveLockDialogueMessageComposer = 3789;
        public const int BuildersClubMembershipMessageComposer = 2778;
        public const int FlatAccessDeniedMessageComposer = 3251;
        public const int LatencyResponseMessageComposer = 594;
        public const int HabboUserBadgesMessageComposer = 3808;
        public const int HeightMapMessageComposer = 2830;

        public const int CanCreateRoomMessageComposer = 584;
        public const int InstantMessageErrorMessageComposer = 302;
        public const int GnomeBoxMessageComposer = 2872;
        public const int IgnoreStatusMessageComposer = 226;
        public const int PetInformationMessageComposer = 2780;
        public const int NavigatorSearchResultSetMessageComposer = 659;
        public const int ConcurrentUsersGoalProgressMessageComposer = 616;
        public const int VideoOffersRewardsMessageComposer = 40;
        public const int SanctionStatusMessageComposer = 3393;
        public const int GetYouTubeVideoMessageComposer = 2013;
        public const int CheckPetNameMessageComposer = 740;
        public const int RespectPetNotificationMessageComposer = 298;
        public const int EnforceCategoryUpdateMessageComposer = 3851;
        public const int CommunityGoalHallOfFameMessageComposer = 2182;
        public const int FloorPlanFloorMapMessageComposer = 2282;
        public const int SendGameInvitationMessageComposer = 469;
        public const int GiftWrappingErrorMessageComposer = 1124;
        public const int PromoArticlesMessageComposer = 1106;
        public const int Game1WeeklyLeaderboardMessageComposer = 1450;
        public const int RentableSpacesErrorMessageComposer = 2325;
        public const int AddExperiencePointsMessageComposer = 2301;
        public const int OpenHelpToolMessageComposer = 2166;
        public const int GetRoomFilterListMessageComposer = 3160;
        public const int GameAchievementListMessageComposer = 547;
        public const int PromotableRoomsMessageComposer = 1597;
        public const int FloorPlanSendDoorMessageComposer = 1605;
        public const int RoomEntryInfoMessageComposer = 1094;
        public const int RoomNotificationMessageComposer = 3007;
        public const int ClubGiftsMessageComposer = 2751; //3187
        public const int MOTDNotificationMessageComposer = 1244;
        public const int PopularRoomTagsResultMessageComposer = 1103;
        public const int NewConsoleMessageMessageComposer = 1415;
        public const int RoomPropertyMessageComposer = 908;
        public const int MarketPlaceOffersMessageComposer = 3146;
        public const int TalentTrackMessageComposer = 1222;
        public const int ProfileInformationMessageComposer = 1912;
        public const int BadgeDefinitionsMessageComposer = 1856;
        public const int Game2WeeklyLeaderboardMessageComposer = 1450;
        public const int NameChangeUpdateMessageComposer = 3514;
        public const int RoomVisualizationSettingsMessageComposer = 1198;
        
        public const int FlatCreatedMessageComposer = 2678;
        public const int BotInventoryMessageComposer = 2253;
        public const int LoadGameMessageComposer = 2475;
        public const int UpdateMagicTileMessageComposer = 3292;
        public const int CampaignCalendarDataMessageComposer = 2467;
        public const int MaintenanceStatusMessageComposer = 1246;
        public const int Game3WeeklyLeaderboardMessageComposer = 1450;
        public const int GameListMessageComposer = 3550;
        public const int RoomMuteSettingsMessageComposer = 1224;
        public const int RoomInviteMessageComposer = 2733;
        public const int LoveLockDialogueSetLockedMessageComposer = 271;
        public const int BroadcastMessageAlertMessageComposer = 3848;
        public const int MarketplaceCancelOfferResultMessageComposer = 1813;
        public const int NavigatorSettingsMessageComposer = 482;

        public const int MessengerInitMessageComposer = 1914;

        //polls
        // NUEVO public const int PollAnswerComposer = 1723;


        /* NO ESTÁN EN ALASKA */
        public const int ServerErrorMessageComposer = 2271;        
        public const int CameraPurchaseOkComposer = 2742;        
        public const int HabboClubOffersMessageComposer = 3360;        
        
        public const int QuizDataMessageComposer = 808;        
        public const int PollContentsMessageComposer = 2238;
        
        public const int UserGameAchievementsMessageComposer = 547;
        public const int ModeratorUserClassMessageComposer = 1628;
        public const int UniqueMachineIDMessageComposer = 2747;        
        public const int GroupForumDataMessageComposer = 2857;
        public const int GroupDeletedMessageComposer = 2231;
        public const int YoutubeDisplayPlaylistsMessageComposer = 2638;                
        public const int MarketplaceMakeOfferResultMessageComposer = 3565;
        public const int MuteAllInRoomMessageComposer = 1224;
        public const int FigureUpdateMessageComposer = 2439;
        public const int PollOfferMessageComposer = 1456;
        public const int ThumbnailSuccessMessageComposer = 3173;
        public const int PostQuizAnswersMessageComposer = 3758;
        public const int GuideSessionRequesterRoomMessageComposer = 2654;
        public const int HallOfFameMessageComposer = 185;
        
        
        public const int CameraStorageUrlMessageComposer = 3695;
        public const int PerkAllowancesMessageComposer = 441;
        public const int YouTubeDisplayVideoMessageComposer = 2013;        
        public const int QuestionAnswersMessageComposer = 783; //results
        public const int EndPollMessageComposer = 3232;        
        public const int SetCameraPriceMessageComposer = 786;
        public const int GuideSessionEndedMessageComposer = 334;
        
        public const int CampaignCalendarGiftMessageComposer = 2036;
        public const int HabboClubCenterInfoMessageComposer = 1314;
        public const int QuizResultsMessageComposer = 3758;      
        public const int GroupForumNewThreadMessageComposer = 1002;
        public const int GroupForumReadThreadMessageComposer = 3887;        
        public const int GroupForumListingsMessageComposer = 757;        
        public const int GetClubComposer = 3360;       
        public const int GroupForumThreadUpdateMessageComposer = 2339;
        public const int GroupForumNewResponseMessageComposer = 255;
        public const int GroupForumThreadRootMessageComposer = 2020;
        
        
        public const int SuperNotificationMessageComposer = 3007;//Jonteh
        public const int NuxUserStatus = 3727;
        
        public const int HelperToolConfigurationMessageComposer = 862;
        public const int OnGuideSessionStartedMessageComposer = 1090;
        public const int OnGuideSessionPartnerIsTypingMessageComposer = 873;
        public const int OnGuideSessionMsgMessageComposer = 1738;
        public const int OnGuideSessionInvitedToGuideRoomMessageComposer = 1828;
        public const int OnGuideSessionAttachedMessageComposer = 1173;
        public const int OnGuideSessionDetachedMessageComposer = 2975;
        public const int OnGuideSessionError = 2181;
        public const int Tool2 = 1695;
        //new alert with only a link
        public const int NuxAlertMessageComposer = 2130;
        public const int TargetedOfferComposer = 2907; 
        public const int NuxNotificationMessageComposer = 2130;
        
        //Camera 2016 PRODUCTION 201607
        public const int CameraPhotoPreviewComposer = 3695;
        public const int CameraPhotoPurchaseOkComposer = 2742;
        public const int CameraPriceComposer = 786;
        public const int SendRoomThumbnailAlertMessageComposer = 3173;

    }
}
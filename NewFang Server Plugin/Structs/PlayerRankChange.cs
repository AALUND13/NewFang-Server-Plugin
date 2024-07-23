using NewFangServerPlugin.Utils;
using Sandbox.Game.World;
using VRage.Game.ModAPI;

namespace NewFangServerPlugin.Structs {
    public enum RankChangeType {
        Promoted,
        Demoted,
        SameRank
    }

    public struct PlayerRankChange {
        public string PlayerName;

        public RankChangeType ChangeType;

        public MyPromoteLevel OldRank;
        public MyPromoteLevel NewRank = MyPromoteLevel.None;

        public PlayerRankChange(string playerName, RankChangeType changeType, MyPromoteLevel oldRank) {
            PlayerName = playerName;
            ChangeType = changeType;
            OldRank = oldRank;
        }

        private static MyPromoteLevel GetNextPromotedRank(MyPromoteLevel oldRank) {
            return (oldRank < MyPromoteLevel.Admin) ? oldRank + (!MySession.Static.Settings.EnableScripterRole && oldRank == MyPromoteLevel.None ? 2 : 1) : MyPromoteLevel.Admin;
        }

        private static MyPromoteLevel GetNextDemotedRank(MyPromoteLevel oldRank) {
            return (oldRank > MyPromoteLevel.None && oldRank < MyPromoteLevel.Owner) ? oldRank - (!MySession.Static.Settings.EnableScripterRole && oldRank == MyPromoteLevel.Moderator ? 2 : 1) : MyPromoteLevel.None;
        }

        public static PlayerRankChange PromotePlayer(ulong playerSteamID) {
            MyPromoteLevel oldPromoteLevel = ManagerUtils.MultiplayerManager.GetUserPromoteLevel(playerSteamID);
            ManagerUtils.MultiplayerManager?.PromoteUser(playerSteamID);
            MyPromoteLevel newPromoteLevel = GetNextPromotedRank(oldPromoteLevel);

            RankChangeType rankChangeType = oldPromoteLevel == newPromoteLevel ? RankChangeType.SameRank : RankChangeType.Promoted;
            PlayerRankChange playerRankChange = new PlayerRankChange(ManagerUtils.MultiplayerManager.GetPlayerBySteamId(playerSteamID).DisplayName, rankChangeType, oldPromoteLevel);

            playerRankChange.NewRank = newPromoteLevel;
            return playerRankChange;
        }

        public static PlayerRankChange DemotePlayer(ulong playerSteamID) {
            MyPromoteLevel oldPromoteLevel = ManagerUtils.MultiplayerManager.GetUserPromoteLevel(playerSteamID);
            ManagerUtils.MultiplayerManager?.DemoteUser(playerSteamID);
            MyPromoteLevel newPromoteLevel = GetNextDemotedRank(oldPromoteLevel);

            RankChangeType rankChangeType = oldPromoteLevel == newPromoteLevel ? RankChangeType.SameRank : RankChangeType.Demoted;
            PlayerRankChange playerRankChange = new PlayerRankChange(ManagerUtils.MultiplayerManager.GetPlayerBySteamId(playerSteamID).DisplayName, rankChangeType, oldPromoteLevel);

            playerRankChange.NewRank = newPromoteLevel;
            return playerRankChange;
        }
    }
}

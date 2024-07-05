using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Photon.Pun;
using Photon.Realtime;

public static class PhotonExtensions {

    private static readonly Dictionary<string, string> SPECIAL_PLAYERS = new() {
        ["01d143e824c4b643da8fda2175d7f3093044704d9bf7998e00c810e5b624bab8"] = "FrostyCake",
        ["8bf9542bac588ab5a664784bbe6080d1f84c78fefef2217766903e1106a55b95"] = "BluCor",
        ["d7be7eb61c96debad675cbcda205989ee01af26fd7fa91b722433ce865674e98"] = "KingKittyTurnip",
        ["3697084da76ae50c3a3d023d574beef5656b6ed940f088a3548fc2d3eb17a234"] = "Foxyyy",
        ["050415559706daa17f34930e583a723d3e9e5d1fe9132e2f450582854dd5b471"] = "zomblebobble",
        ["7faa2bbc006e95bfb0000451c2052764c1120019c12c5b129427a46fc2ca3b47"] = "vic",
        ["61213ee830c493f194018d7756d194435deb588f88df6beaa84e19285be20165"] = "Lust",
        ["3bafa966443ec6e61e0e444daad7a233962cb790a5e601f1a6b034a289350ed3"] = "Windows10V",
    };

    public static bool IsMineOrLocal(this PhotonView view) {
        return !view || view.IsMine;
    }

    public static bool HasRainbowName(this Player player) {
        if (player == null || player.UserId == null)
            return false;

        byte[] bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(player.UserId));
        StringBuilder sb = new();
        foreach (byte b in bytes)
            sb.Append(b.ToString("X2"));

        string hash = sb.ToString().ToLower();
        return SPECIAL_PLAYERS.ContainsKey(hash) && player.NickName == SPECIAL_PLAYERS[hash];
    }

    //public static void RPCFunc(this PhotonView view, Delegate action, RpcTarget target, params object[] parameters) {
    //    view.RPC(nameof(action), target, parameters);
    //}
}
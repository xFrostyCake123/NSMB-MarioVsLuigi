using UnityEngine;
using Photon.Pun;
using NSMB.Utils;

[CreateAssetMenu(fileName = "NormalSwitchTile", menuName = "ScriptableObjects/Tiles/Switchs/NormalSwitchTile", order = 0)]
public class NewSwitchTile : InteractableTile {
    [ColorUsage(false)]
    public Color particleColor;
    public bool breakableBySmallMario = false, breakableByLargeMario = false, breakableByGiantMario = false, breakableByShells = false, breakableByBombs = false, bumpIfNotBroken = true, bumpIfBroken = false,onlyOne = false, lockmode = false;
    private bool off = true;
    public int otherSpawnPointX = 0;
    public int otherSpawnPointY = 0;
    public int repeatX = 1;
    public int repeatY = 1;
    public string resultTile;
    public string Path = "Switchs/";
    public bool breakSound = false, breakParticle = false;
    public Color breakTileColor;
    protected bool BreakBlockCheck(MonoBehaviour interacter, InteractionDirection direction, Vector3 worldLocation) {
        bool doBump = false, doBreak = false, giantBreak = false;
        if (interacter is PlayerController pl) {
            if (pl.state <= Enums.PowerupState.Small && !pl.drill) {
                doBreak = breakableBySmallMario;
                doBump = true;
            } else if (pl.state == Enums.PowerupState.MegaMushroom) {
                doBreak = breakableByGiantMario;
                giantBreak = true;
                doBump = false;
            } else if (pl.state >= Enums.PowerupState.Mushroom || pl.drill) {
                doBreak = breakableByLargeMario;
                doBump = true;
            }

        } else if (interacter is SpinyWalk) {
            doBump = true;
            doBreak = breakableByShells;
        } else if (interacter is KoopaWalk) {
            doBump = true;
            doBreak = breakableByShells;
        } else if (interacter is BobombWalk) {
            doBump = false;
            doBreak = breakableByBombs;
        }
        if (doBump && doBreak && bumpIfBroken)
            Bump(interacter, direction, worldLocation);
        if (doBump && !doBreak && bumpIfNotBroken)
        BumpWithAnimation(interacter, direction, worldLocation);
        SetEvent(interacter, worldLocation);
        if (doBreak)
            Break(interacter, worldLocation, giantBreak ? Enums.Sounds.Powerup_MegaMushroom_Break_Block : Enums.Sounds.World_Block_Break);
        return doBreak;
    }
    public void Break(MonoBehaviour interacter, Vector3 worldLocation, Enums.Sounds sound) {
        Vector3Int tileLocation = Utils.WorldToTilemapPosition(worldLocation);

        //Tilemap
        object[] parametersTile = new object[] { tileLocation.x, tileLocation.y, null };
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);

        object[] parametersParticle = new object[]{ tileLocation.x, tileLocation.y, "BrickBreak", new Vector3(particleColor.r, particleColor.g, particleColor.b) };
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SpawnParticle, parametersParticle, ExitGames.Client.Photon.SendOptions.SendUnreliable);

        if (interacter is MonoBehaviourPun pun)
            pun.photonView.RPC("PlaySound", RpcTarget.All, sound);
    }
    public void BumpWithAnimation(MonoBehaviour interacter, InteractionDirection direction, Vector3 worldLocation) {
        Bump(interacter, direction, worldLocation);
        Vector3Int tileLocation = Utils.WorldToTilemapPosition(worldLocation);

        //Bump
        object[] parametersBump = new object[]{tileLocation.x, tileLocation.y, direction == InteractionDirection.Down, "SpecialTiles/" + Path + name, ""};
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.BumpTile, parametersBump, ExitGames.Client.Photon.SendOptions.SendReliable);
    }
    public void SetEvent(MonoBehaviour interacter, Vector3 worldLocation)
    {
        Vector3Int tileLocation = Utils.WorldToTilemapPosition(worldLocation);

        //Tilemap
        if (off == true && lockmode == false && onlyOne == false)
        {
            for (int i = 0; i < repeatX; i++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX + i, otherSpawnPointY, null };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            for (int k = 0; k < repeatY; k++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX, otherSpawnPointY + k, null };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            off = false;
        }
        else if(off == false && lockmode == false && onlyOne == false)
        {
            for (int j = 0; j < repeatX; j++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX + j, otherSpawnPointY, resultTile };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            for (int l = 0; l < repeatY; l++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX, otherSpawnPointY + l, resultTile };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            off = true;
        }
        else if(lockmode == true && onlyOne == false)
        {
            for (int q = 0; q < repeatX; q++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX + q, otherSpawnPointY, resultTile };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            for (int w = 0; w < repeatY; w++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX, otherSpawnPointY + w, resultTile };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
        }
        else if(lockmode == false && onlyOne == true)
        {
            for (int a = 0; a < repeatX; a++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX + a, otherSpawnPointY, resultTile };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            for (int b = 0; b < repeatY; b++)
            {
                object[] parametersTile = new object[] { otherSpawnPointX, otherSpawnPointY + b, resultTile };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            Break(interacter, worldLocation, Enums.Sounds.World_Block_Break);
        }
        if (breakSound == true) sound(interacter, worldLocation, breakParticle);
    }
    public void sound(MonoBehaviour interacter, Vector3 worldLocation, bool breakParticle)
    {
        Vector3Int tileLocation = Utils.WorldToTilemapPosition(worldLocation);
        if(breakParticle == true)
        {
            for (int i = 0; i < repeatX; i++)
            {
                object[] parametersParticle = new object[] { otherSpawnPointX + i, otherSpawnPointY, "BrickBreak", new Vector3(breakTileColor.r, breakTileColor.g, breakTileColor.b) };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SpawnParticle, parametersParticle, ExitGames.Client.Photon.SendOptions.SendUnreliable);
            }
            for (int k = 0; k < repeatY; k++)
            {
                object[] parametersParticle = new object[] { otherSpawnPointX, otherSpawnPointY + k, "BrickBreak", new Vector3(breakTileColor.r, breakTileColor.g, breakTileColor.b) };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SpawnParticle, parametersParticle, ExitGames.Client.Photon.SendOptions.SendUnreliable);
            }
        }
        if (interacter is MonoBehaviourPun pun)
            pun.photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Block_Break);
    }
    public override bool Interact(MonoBehaviour interacter, InteractionDirection direction, Vector3 worldLocation) {
        //Breaking block check.
        return BreakBlockCheck(interacter, direction, worldLocation);
    }
}

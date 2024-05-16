using UnityEngine;
using Photon.Pun;
using NSMB.Utils;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PowerupTile", menuName = "ScriptableObjects/Tiles/GhostPowerupTile", order = 9)]
public class GhostPowerupTile : BreakableBrickTile {
    public List<Vector2> launchVelocities;
    public GameObject smallPowerup;
    public GameObject powerup;
    public GameObject instantiatedObject;
    public string resultTile;
    public override bool Interact(MonoBehaviour interacter, InteractionDirection direction, Vector3 worldLocation) {
        if (base.Interact(interacter, direction, worldLocation))
            return true;

        Vector3Int tileLocation = Utils.WorldToTilemapPosition(worldLocation);

        GameObject spawnResult = smallPowerup;

        if ((interacter is PlayerController) || (interacter is KoopaWalk koopa && koopa.previousHolder != null)) {
            PlayerController player = interacter is PlayerController controller ? controller : ((KoopaWalk)interacter).previousHolder;
            if (player.state == Enums.PowerupState.MegaMushroom) {
                //Break

                //Tilemap
                object[] parametersTile = new object[]{tileLocation.x, tileLocation.y, null};
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTile, parametersTile, ExitGames.Client.Photon.SendOptions.SendReliable);

                //Particle
                object[] parametersParticle = new object[]{tileLocation.x, tileLocation.y, "BrickBreak", new Vector3(particleColor.r, particleColor.g, particleColor.b)};
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SpawnParticle, parametersParticle, ExitGames.Client.Photon.SendOptions.SendUnreliable);

                if (interacter is MonoBehaviourPun pun)
                    pun.photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Block_Break);
                return true;
            }

            spawnResult = player.state <= Enums.PowerupState.Small ? smallPowerup : powerup;
        }

        
        Bump(interacter, direction, worldLocation);

        
        
        object[] parametersBump = new object[]{tileLocation.x, tileLocation.y, direction == InteractionDirection.Down, resultTile, instantiatedObject = spawnResult};
        GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.BumpTile, parametersBump, ExitGames.Client.Photon.SendOptions.SendReliable);
        Vector2 launchVelocity = launchVelocities[Random.Range(0, launchVelocities.Count)];
        if (instantiatedObject != null) {
            Rigidbody2D body = instantiatedObject.GetComponent<Rigidbody2D>();
            body.velocity = new Vector2(launchVelocity.x != 0 ? launchVelocity.x : body.velocity.x, launchVelocity.y);
            body.position += Vector2.up * 0.075f;
        }

        if (interacter is MonoBehaviourPun pun2)
            pun2.photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Block_Powerup);
        return false;
    }
}

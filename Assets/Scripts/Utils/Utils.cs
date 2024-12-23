using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

using Photon.Pun;
using Photon.Realtime;

namespace NSMB.Utils {
    public class Utils {

        public static int FirstPlaceStars {
            get => GameManager.Instance.players.Where(pl => pl.lives != 0).Max(pc => pc.stars);
        }
        public static int FirstPlaceLives {
            get => GameManager.Instance.players.Where(pl => pl.lives != 0).Max(pc => pc.lives);
        }
        public static bool BitTest(long bit, int index) {
            return (bit & (1 << index)) != 0;
        }

        public static Vector3Int WorldToTilemapPosition(Vector3 worldVec, GameManager manager = null, bool wrap = true) {
            if (!manager)
                manager = GameManager.Instance;

            Vector3Int tileLocation = manager.tilemap.WorldToCell(worldVec);
            if (wrap)
                WrapTileLocation(ref tileLocation, manager);

            return tileLocation;
        }

        public static bool WrapWorldLocation(ref Vector3 location, GameManager manager = null) {
            if (manager == null)
                manager = GameManager.Instance;

            if (!manager.loopingLevel)
                return false;

            if (location.x < manager.GetLevelMinX()) {
                location.x += manager.levelWidthTile / 2;
                return true;
            } else if (location.x >= manager.GetLevelMaxX()) {
                location.x -= manager.levelWidthTile / 2;
                return true;
            }
            return false;
        }

        public static void WrapTileLocation(ref Vector3Int tileLocation, GameManager manager = null) {
            if (manager == null)
                manager = GameManager.Instance;

            if (!manager.loopingLevel)
                return;

            if (tileLocation.x < manager.levelMinTileX) {
                tileLocation.x += manager.levelWidthTile;
            } else if (tileLocation.x >= manager.levelMinTileX + manager.levelWidthTile) {
                tileLocation.x -= manager.levelWidthTile;
            }
        }

        public static Vector3Int WorldToTilemapPosition(float worldX, float worldY) {
            return WorldToTilemapPosition(new Vector3(worldX, worldY));
        }

        public static Vector3 TilemapToWorldPosition(Vector3Int tileVec, GameManager manager = null) {
            if (!manager)
                manager = GameManager.Instance;
            return manager.tilemap.CellToWorld(tileVec);
        }

        public static Vector3 TilemapToWorldPosition(int tileX, int tileY) {
            return TilemapToWorldPosition(new Vector3Int(tileX, tileY));
        }

        public static int GetCharacterIndex(Player player = null) {
            if (player == null)
                player = PhotonNetwork.LocalPlayer;

            //Assert.IsNotNull(player, "player is null, are we not connected to Photon?");

            GetCustomProperty(Enums.NetPlayerProperties.Character, out int index, player.CustomProperties);
            return index;
        }
        public static PlayerData GetCharacterData(Player player = null) {
            return GlobalController.Instance.characters[GetCharacterIndex(player)];
        }

        public static TileBase GetTileAtTileLocation(Vector3Int tileLocation) {
            WrapTileLocation(ref tileLocation);
            return GameManager.Instance.tilemap.GetTile(tileLocation);
        }
        public static TileBase GetTileAtWorldLocation(Vector3 worldLocation) {
            return GetTileAtTileLocation(WorldToTilemapPosition(worldLocation));
        }

        public static bool IsTileSolidAtTileLocation(Vector3Int tileLocation) {
            WrapTileLocation(ref tileLocation);
            return GetColliderType(tileLocation) == Tile.ColliderType.Grid;
        }

        private static Tile.ColliderType GetColliderType(Vector3Int tileLocation) {

            Tilemap tm = GameManager.Instance.tilemap;

            if (tm.GetTile<Tile>(tileLocation) is Tile tile)
                return tile.colliderType;

            if (tm.GetTile<RuleTile>(tileLocation) is RuleTile rule)
                return rule.m_DefaultColliderType;

            if (tm.GetTile<AnimatedTile>(tileLocation) is AnimatedTile animated)
                return animated.m_TileColliderType;

            return Tile.ColliderType.None;
        }

        public static bool IsTileSolidBetweenWorldBox(Vector3Int tileLocation, Vector2 worldLocation, Vector2 worldBox, bool boxcast = true) {
            if (boxcast) {
                Collider2D collision = Physics2D.OverlapPoint(worldLocation, LayerMask.GetMask("Ground"));
                if (collision && !collision.isTrigger && !collision.CompareTag("Player"))
                    return true;
            }

            Vector2 ogWorldLocation = worldLocation;
            while (GetTileAtTileLocation(tileLocation) is TileInteractionRelocator it) {
                worldLocation += (Vector2) (Vector3) it.offset * 0.5f;
                tileLocation += it.offset;
            }

            Matrix4x4 tileTransform = GameManager.Instance.tilemap.GetTransformMatrix(tileLocation);

            Vector2 halfBox = worldBox * 0.5f;
            List<Vector2> boxPoints = new();
            boxPoints.Add(ogWorldLocation + Vector2.up * halfBox + Vector2.right * halfBox); // ++
            boxPoints.Add(ogWorldLocation + Vector2.up * halfBox + Vector2.left * halfBox); // +-
            boxPoints.Add(ogWorldLocation + Vector2.down * halfBox + Vector2.left * halfBox); // --
            boxPoints.Add(ogWorldLocation + Vector2.down * halfBox + Vector2.right * halfBox); // -+

            Sprite sprite = GameManager.Instance.tilemap.GetSprite(tileLocation);
            switch (GetColliderType(tileLocation)) {
            case Tile.ColliderType.Grid:
                return true;
            case Tile.ColliderType.None:
                return false;
            case Tile.ColliderType.Sprite:

                for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++) {
                    List<Vector2> points = new();
                    sprite.GetPhysicsShape(i, points);

                    for (int j = 0; j < points.Count; j++) {
                        Vector2 point = points[j];
                        point *= 0.5f;
                        point = tileTransform.MultiplyPoint(point);
                        point += (Vector2) (Vector3) tileLocation * 0.5f;
                        point += (Vector2) GameManager.Instance.tilemap.transform.position;
                        point += Vector2.one * 0.25f;
                        points[j] = point;
                    }

                    for (int j = 0; j < points.Count; j++) {
                        Debug.DrawLine(points[j], points[(j + 1) % points.Count], Color.white, 10);
                    }
                    for (int j = 0; j < boxPoints.Count; j++) {
                        Debug.DrawLine(boxPoints[j], boxPoints[(j + 1) % boxPoints.Count], Color.blue, 3);
                    }


                    if (PolygonsOverlap(points, boxPoints))
                        return true;
                }
                return false;
            }

            return IsTileSolidAtTileLocation(WorldToTilemapPosition(worldLocation));
        }

        public static bool PolygonsOverlap(List<Vector2> polygonA, List<Vector2> polygonB) {
            int edgeCountA = polygonA.Count;
            int edgeCountB = polygonB.Count;

            // Loop through all the edges of both polygons
            for (int i = 0; i < edgeCountA; i++) {
                if (IsInside(polygonB, polygonA[i]))
                    return true;
            }

            for (int i = 0; i < edgeCountB; i++) {
                if (IsInside(polygonA, polygonB[i]))
                    return true;
            }

            return false;
        }

        public static bool IsTileSolidAtWorldLocation(Vector3 worldLocation) {
            Collider2D collision = Physics2D.OverlapPoint(worldLocation, LayerMask.GetMask("Ground"));
            if (collision && !collision.isTrigger && !collision.CompareTag("Player"))
                return true;

            while (GetTileAtWorldLocation(worldLocation) is TileInteractionRelocator it)
                worldLocation += (Vector3) it.offset * 0.5f;

            Vector3Int tileLocation = WorldToTilemapPosition(worldLocation);
            Matrix4x4 tileTransform = GameManager.Instance.tilemap.GetTransformMatrix(tileLocation);

            Sprite sprite = GameManager.Instance.tilemap.GetSprite(tileLocation);
            switch (GetColliderType(tileLocation)) {
            case Tile.ColliderType.Grid:
                return true;
            case Tile.ColliderType.None:
                return false;
            case Tile.ColliderType.Sprite:
                for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++) {
                    List<Vector2> points = new();
                    sprite.GetPhysicsShape(i, points);

                    for (int j = 0; j < points.Count; j++) {
                        Vector2 point = points[j];
                        point *= 0.5f;
                        point = tileTransform.MultiplyPoint(point);
                        point += (Vector2) (Vector3) tileLocation * 0.5f;
                        point += (Vector2) GameManager.Instance.tilemap.transform.position;
                        point += Vector2.one * 0.25f;
                        points[j] = point;
                    }

                    if (IsInside(points, worldLocation))
                        return true;
                }
                return false;
            }

            return IsTileSolidAtTileLocation(WorldToTilemapPosition(worldLocation));
        }

        // Given three collinear points p, q, r,
        // the function checks if point q lies
        // on line segment 'pr'
        static bool OnSegment(Vector2 p, Vector2 q, Vector2 r) {
            if (q.x <= Mathf.Max(p.x, r.x) &&
                q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) &&
                q.y >= Mathf.Min(p.y, r.y)) {
                return true;
            }
            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are collinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        static float Orientation(Vector2 p, Vector2 q, Vector2 r) {
            float val = (q.y - p.y) * (r.x - q.x) -
                    (q.x - p.x) * (r.y - q.y);

            if (Mathf.Abs(val) < 0.001f) {
                return 0; // collinear
            }
            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The function that returns true if
        // line segment 'p1q1' and 'p2q2' intersect.
        static bool DoIntersect(Vector2 p1, Vector2 q1,
                                Vector2 p2, Vector2 q2) {
            // Find the four orientations needed for
            // general and special cases
            float o1 = Orientation(p1, q1, p2);
            float o2 = Orientation(p1, q1, q2);
            float o3 = Orientation(p2, q2, p1);
            float o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4) {
                return true;
            }

            // Special Cases
            // p1, q1 and p2 are collinear and
            // p2 lies on segment p1q1
            if (o1 == 0 && OnSegment(p1, p2, q1)) {
                return true;
            }

            // p1, q1 and p2 are collinear and
            // q2 lies on segment p1q1
            if (o2 == 0 && OnSegment(p1, q2, q1)) {
                return true;
            }

            // p2, q2 and p1 are collinear and
            // p1 lies on segment p2q2
            if (o3 == 0 && OnSegment(p2, p1, q2)) {
                return true;
            }

            // p2, q2 and q1 are collinear and
            // q1 lies on segment p2q2
            if (o4 == 0 && OnSegment(p2, q1, q2)) {
                return true;
            }

            // Doesn't fall in any of the above cases
            return false;
        }

        // Returns true if the point p lies
        // inside the polygon[] with n vertices
        static bool IsInside(List<Vector2> polygon, Vector2 p) {
            // There must be at least 3 vertices in polygon[]
            if (polygon.Count < 3) {
                return false;
            }

            // Create a point for line segment from p to infinite
            Vector2 extreme = new(1000, p.y);

            // Count intersections of the above line
            // with sides of polygon
            int count = 0, i = 0;
            do {
                int next = (i + 1) % polygon.Count;

                // Check if the line segment from 'p' to
                // 'extreme' intersects with the line
                // segment from 'polygon[i]' to 'polygon[next]'
                if (DoIntersect(polygon[i],
                                polygon[next], p, extreme)) {
                    // If the point 'p' is collinear with line
                    // segment 'i-next', then check if it lies
                    // on segment. If it lies, return true, otherwise false
                    if (Orientation(polygon[i], p, polygon[next]) == 0) {
                        return OnSegment(polygon[i], p,
                                        polygon[next]);
                    }
                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise
            return (count % 2 == 1); // Same as (count%2 == 1)
        }

        public static bool IsAnyTileSolidBetweenWorldBox(Vector2 checkPosition, Vector2 checkSize, bool boxcast = true) {
            Vector3Int minPos = WorldToTilemapPosition(checkPosition - (checkSize / 2), wrap: false);
            Vector3Int size = WorldToTilemapPosition(checkPosition + (checkSize / 2), wrap: false) - minPos;

            for (int x = 0; x <= size.x; x++) {
                for (int y = 0; y <= size.y; y++) {

                    Vector3Int tileLocation = new(minPos.x + x, minPos.y + y, 0);
                    WrapTileLocation(ref tileLocation);

                    if (IsTileSolidBetweenWorldBox(tileLocation, checkPosition, checkSize, boxcast))
                        return true;
                }
            }
            return false;
        }

        public static float WrappedDistance(Vector2 a, Vector2 b) {
            GameManager gm = GameManager.Instance;
            if ((gm?.loopingLevel ?? false) && Mathf.Abs(a.x - b.x) > gm.levelWidthTile / 4f)
                a.x -= gm.levelWidthTile / 2f * Mathf.Sign(a.x - b.x);

            return Vector2.Distance(a, b);
        }

        public static bool GetCustomProperty<T>(string key, out T value, ExitGames.Client.Photon.Hashtable properties = null) {
            if (properties == null)
                properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (properties == null) {
                value = default;
                return false;
            }

            properties.TryGetValue(key, out object temp);
            if (temp != null) {
                value = (T) temp;
                return true;
            } else {
                value = default;
                return false;
            }
        }

        public static Powerup[] powerups = null;
        // MAX(0,$B15+(IF(stars behind >0,LOG(B$1+1, 2.71828),0)*$C15*(1-(($M$15-$M$14))/$M$15)))
        public static Powerup GetRandomItem(PlayerController player) {
            GameManager gm = GameManager.Instance;
            GetCustomProperty(Enums.NetRoomProperties.DeathmatchGame, out bool dtm);
            // "losing" variable based on ln(x+1), x being the # of stars we're behind
            int ourStars = dtm ? player.lives : player.stars;
            int leaderStars = dtm ? FirstPlaceLives : FirstPlaceStars;
            GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool teaming);
            GetCustomProperty(Enums.NetRoomProperties.ShareStars, out int starshare);
            if (teaming && starshare == 1 && !dtm) {
                ourStars = gm.teamController.GetTeamStars(player.team);
                leaderStars = gm.teamController.LeaderTeamStars();
            }
            if (powerups == null)
                powerups = Resources.LoadAll<Powerup>("Scriptables/Powerups");

            GetCustomProperty(Enums.NetRoomProperties.StarRequirement, out int starsToWin);
            GetCustomProperty(Enums.NetRoomProperties.NewPowerups, out bool custom);
            GetCustomProperty(Enums.NetRoomProperties.NsmbPowerups, out bool nsmb);
            GetCustomProperty(Enums.NetRoomProperties.TenPlayersPowerups, out bool ten);
            GetCustomProperty(Enums.NetRoomProperties.PropellerMush, out bool wii);
            GetCustomProperty(Enums.NetRoomProperties.TemporaryPowerups, out bool temp);
            GetCustomProperty(Enums.NetRoomProperties.OneUpMush, out bool oneup);
            GetCustomProperty(Enums.NetRoomProperties.FrostyPowerups, out bool frosty);
            GetCustomProperty(Enums.NetRoomProperties.SuperAcornPowerup, out bool acorn);
            GetCustomProperty(Enums.NetRoomProperties.TideFlowerPowerup, out bool tide);
            GetCustomProperty(Enums.NetRoomProperties.FireFlowerPowerup, out bool fire);
            GetCustomProperty(Enums.NetRoomProperties.CobaltStarPowerup, out bool cobalt);
            GetCustomProperty(Enums.NetRoomProperties.StarmanPowerup, out bool star);
            GetCustomProperty(Enums.NetRoomProperties.BlueShellPowerup, out bool blue);
            GetCustomProperty(Enums.NetRoomProperties.MiniMushroomPowerup, out bool mini);
            GetCustomProperty(Enums.NetRoomProperties.MagmaFlowerPowerup, out bool magma);
            GetCustomProperty(Enums.NetRoomProperties.Lightning, out bool lightning);
            
            GetCustomProperty(Enums.NetRoomProperties.Lives, out int livesOn);
            bool lives = false;
            if (livesOn > 0)
                lives = true;

            bool big = gm.spawnBigPowerups;
            bool vertical = gm.spawnVerticalPowerups;
            
            float totalChance = 0;
            foreach (Powerup powerup in powerups) {
                if (powerup.name == "MegaMushroom" && gm.musicState == Enums.MusicState.MegaMushroom)
                    continue;
                if (powerup.name == "CobaltStar" && gm.musicState == Enums.MusicState.CobaltStar)
                    continue;
                if (powerup.name == "Lightning" && (gm.musicState == Enums.MusicState.CobaltStar || gm.lightningCooldown > 0))
                    continue;
                if ((powerup.big && !big) || (powerup.vertical && !vertical) || (powerup.custom && !custom) || (powerup.lives && !lives) || (powerup.frosty && !frosty) || (powerup.ten && !ten) || (powerup.nsmb && !nsmb) || (powerup.wii && !wii) || (powerup.oneup && !oneup) || (powerup.temporary && !temp) || (powerup.cobalt && !cobalt) || (powerup.star && !star) || (powerup.blue && !blue) || (powerup.acorn && !acorn) || (powerup.fire && !fire) || (powerup.mini && !mini) || (powerup.tide && !tide) || (powerup.magma && !magma) || (powerup.lightning && !lightning))
                    continue;

                totalChance += powerup.GetModifiedChance(starsToWin, leaderStars, ourStars);
            }

            float rand = Random.value * totalChance;
            foreach (Powerup powerup in powerups) {
                if (powerup.name == "MegaMushroom" && gm.musicState == Enums.MusicState.MegaMushroom)
                    continue;
                if (powerup.name == "CobaltStar" && gm.musicState == Enums.MusicState.CobaltStar)
                    continue;
                if (powerup.name == "Lightning" && (gm.musicState == Enums.MusicState.CobaltStar || gm.lightningCooldown > 0))
                    continue;
                if ((powerup.big && !big) || (powerup.vertical && !vertical) || (powerup.custom && !custom) || (powerup.lives && !lives) || (powerup.frosty && !frosty) || (powerup.ten && !ten) || (powerup.nsmb && !nsmb) || (powerup.wii && !wii) || (powerup.oneup && !oneup) || (powerup.temporary && !temp) || (powerup.cobalt && !cobalt) || (powerup.star && !star) || (powerup.blue && !blue) || (powerup.acorn && !acorn) || (powerup.fire && !fire) || (powerup.mini && !mini) || (powerup.tide && !tide) || (powerup.magma && !magma) || (powerup.lightning && !lightning))
                    continue;

                float chance = powerup.GetModifiedChance(starsToWin, leaderStars, ourStars);

                if (rand < chance)
                    return powerup;
                rand -= chance;
            }

            return powerups[1];
        }

        public static float QuadraticEaseOut(float v) {
            return -1 * v * (v - 2);
        }

        public static ExitGames.Client.Photon.Hashtable GetTilemapChanges(TileBase[] original, BoundsInt bounds, Tilemap tilemap) {
            Dictionary<int, int> changes = new();
            List<string> tiles = new();

            TileBase[] current = tilemap.GetTilesBlock(bounds);

            for (int i = 0; i < original.Length; i++) {
                if (current[i] == original[i])
                    continue;

                TileBase cTile = current[i];
                string path;
                if (cTile == null) {
                    path = "";
                } else {
                    path = ResourceDB.GetAsset(cTile.name).ResourcesPath;
                }

                if (!tiles.Contains(path))
                    tiles.Add(path);

                changes[i] = tiles.IndexOf(path);
            }

            return new() {
                ["T"] = tiles.ToArray(),
                ["C"] = changes,
            };
        }

        public static void ApplyTilemapChanges(TileBase[] original, BoundsInt bounds, Tilemap tilemap, ExitGames.Client.Photon.Hashtable changesTable) {
            TileBase[] copy = (TileBase[]) original.Clone();

            Dictionary<int, int> changes = (Dictionary<int, int>) changesTable["C"];
            string[] tiles = (string[]) changesTable["T"];

            foreach (KeyValuePair<int, int> pairs in changes) {
                copy[pairs.Key] = GetTileFromCache(tiles[pairs.Value]);
            }

            tilemap.SetTilesBlock(bounds, copy);
        }

        private static readonly Dictionary<string, TileBase> tileCache = new();
        public static TileBase GetTileFromCache(string tilename) {
            if (tilename == null || tilename == "")
                return null;

            if (!tilename.StartsWith("Tilemaps/Tiles/"))
                tilename = "Tilemaps/Tiles/" + tilename;

            return tileCache.ContainsKey(tilename) ?
                tileCache[tilename] :
                tileCache[tilename] = Resources.Load(tilename) as TileBase;
        }

        private static readonly Dictionary<char, byte> uiSymbols = new() {
            ['c'] = 6,
            ['0'] = 11,
            ['1'] = 12,
            ['2'] = 13,
            ['3'] = 14,
            ['4'] = 15,
            ['5'] = 16,
            ['6'] = 17,
            ['7'] = 18,
            ['8'] = 19,
            ['9'] = 20,
            ['x'] = 21,
            ['C'] = 22,
            ['S'] = 23,
            ['/'] = 24,
            [':'] = 25,
            ['P'] = 62,
            ['o'] = 63,
            ['i'] = 64,
            ['n'] = 65,
            ['t'] = 66,
            ['s'] = 67,
        };
        public static readonly Dictionary<char, byte> numberSymbols = new() {
            ['0'] = 27,
            ['1'] = 28,
            ['2'] = 29,
            ['3'] = 30,
            ['4'] = 31,
            ['5'] = 32,
            ['6'] = 33,
            ['7'] = 34,
            ['8'] = 35,
            ['9'] = 36,
            ['+'] = 68,
            ['-'] = 69,
            ['x'] = 70,
            ['/'] = 71,
        };
        public static readonly Dictionary<char, byte> smallSymbols = new() {
            ['0'] = 48,
            ['1'] = 39,
            ['2'] = 40,
            ['3'] = 41,
            ['4'] = 42,
            ['5'] = 43,
            ['6'] = 44,
            ['7'] = 45,
            ['8'] = 46,
            ['9'] = 47,
        };
        public static readonly Dictionary<char, byte> teamCoinSymbols = new() {
            ['0'] = 52,
            ['1'] = 53,
            ['2'] = 54,
            ['3'] = 55,
            ['4'] = 56,
            ['5'] = 57,
            ['6'] = 58,
            ['7'] = 59,
            ['8'] = 60,
            ['9'] = 61,
        };
        public static string GetSymbolString(string str, Dictionary<char, byte> dict = null) {
            if (dict == null)
                dict = uiSymbols;

            StringBuilder ret = new();
            foreach (char c in str) {
                if (dict.TryGetValue(c, out byte index)) {
                    ret.Append("<sprite=").Append(index).Append(">");
                } else {
                    ret.Append(c);
                }
            }
            return ret.ToString();
        }

        public static Color GetPlayerColor(Player player, float s = 1, float v = 1) {
            var teamPlayer = player;
            var teamColor = -1;
            
            int result = -1;
            int count = 0;
            foreach (var pl in PhotonNetwork.PlayerList) {
                GetCustomProperty(Enums.NetPlayerProperties.Spectator, out bool spectating, pl.CustomProperties);
                
                if (spectating)
                    continue;

                if (pl == player)
                    result = count;

                count++;
            }

            GetCustomProperty(Enums.NetPlayerProperties.Spectator, out bool ignoreSpectate, player.CustomProperties);
            GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool teaming);
            
            if (teaming) {
                GetCustomProperty(Enums.NetPlayerProperties.Team, out teamColor, player.CustomProperties);
                if (teamColor == 0 && teaming)
                    return new Color(1f, 0f, 0f, 1f);
                else if (teamColor == 1 && teaming)
                    return new Color(1f, 1f, 0f, 1f);
                else if (teamColor == 2 && teaming)
                    return new Color(0f, 1f, 0f, 1f);
                else if (teamColor == 3 && teaming)
                    return new Color(0f, 0f, 1f, 1f);
                else if (teamColor == 4 && teaming)
                    return new Color(0.65f, 0f, 1f, 1f);
            }
            
            if (result == -1 && !teaming)
                return new Color(0.9f, 0.9f, 0.9f, 0.7f);
            return Color.HSVToRGB(result / ((float) count + 1), s, v);
        }

        public static void TickTimer(ref float counter, float min, float delta, float max = float.MaxValue) {
            counter = Mathf.Clamp(counter - delta, min, max);
        }

        public static Color GetRainbowColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.rainbowGradient.Evaluate((float) time);
        }
        public static Color GetTurnipColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.turnipGradient.Evaluate((float) time);
        }
        public static Color GetVicColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.vicGradient.Evaluate((float) time);
        }
        public static Color GetBlucorColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.blucorGradient.Evaluate((float) time);
        }
        public static Color GetFoxyyyColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.foxyyyGradient.Evaluate((float) time);
        }
        public static Color GetZombleColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.zombleGradient.Evaluate((float) time);
        }
        public static Color GetLustColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.lustGradient.Evaluate((float) time);
        }
        public static Color GetWindowsColor() {
            double time = PhotonNetwork.Time * 0.1;
            time %= 1;
            return GlobalController.Instance.windowsGradient.Evaluate((float) time);
        }
    }
    
}
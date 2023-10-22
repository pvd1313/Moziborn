using Moziborn.Buildings;
using UnityEngine;

namespace Moziborn.Rendering
{
    public static class TextureUtil
    {
        public static void Fill(this Texture2D texture, Color color)
        {
            int textureWidth = texture.width;
            int textureHeight = texture.height;
            
            for (int y = 0; y < textureHeight; y++)
            for (int x = 0; x < textureWidth; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        public static void FillRooms(this Texture2D texture, ILevelLayout levelLayout, Color color)
        {
            BoundsInt levelBounds = levelLayout.bounds;
            
            foreach (BuildingData building in levelLayout.buildings)
            foreach (BoundsInt room in building.rooms)
            foreach (Vector3Int pos in room.allPositionsWithin)
            {
                Vector3Int localPoint = pos - levelBounds.position;
                texture.SetPixel(localPoint.x, localPoint.y, color);
            }
        }

        public static void DrawWalls(this Texture2D texture, ILevelLayout levelLayout, Color color)
        {
            BoundsInt levelBounds = levelLayout.bounds;
            
            foreach (BuildingData building in levelLayout.buildings)
            foreach (Vector2Int wallPos in building.walls)
            {
                Vector2Int localPoint = wallPos - new Vector2Int(levelBounds.position.x, levelBounds.position.y);
                texture.SetPixel(localPoint.x, localPoint.y, color);
            }
        }

        public static void DrawDoors(this Texture2D texture, ILevelLayout levelLayout)
        {
            BoundsInt levelBounds = levelLayout.bounds;
            
            foreach (BuildingData building in levelLayout.buildings)
            foreach (DoorData door in building.doors)
            {
                Vector2Int pos = door.position - new Vector2Int(levelBounds.position.x, levelBounds.position.y);
                texture.SetPixel(pos.x, pos.y, Color.white * door.view.doorState);
            }
        }
    }
}
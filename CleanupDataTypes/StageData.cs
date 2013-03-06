using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace CleanupDataTypes
{
    public class StageData
    {
        public string name;
        public string music;
        public int minutes;
        public Rectangle bounds;
        public float maxZoomOut;
        public string tileTextureName;
        public Vector2 cameraStartPosition;
        public Vector2 player1StartPosition;
        public float player1StartDirection;
        public Vector2 player2StartPosition;
        public float player2StartDirection;

        public float trashSpawnRateYellow;
        public float trashSpawnRateRed;
        public float trashSpawnRateBlue;
        public float trashSpawnRateGreen;
        public float trashSpawnRatePurple;
        public float trashSpawnRateBlack;

        public List<CleanupDataTypes.StageEntity> entities;

    }

    public class StageEntity
    {
        public int type;
        public Vector2 position;
    }
}
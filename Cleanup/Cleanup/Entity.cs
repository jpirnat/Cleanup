using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Cleanup
{
    public class Entity
    {
        public int type;
        public Vector2 position;
        public Rectangle bounds;

        public Entity(int type, Vector2 position)
        {
            this.type = type;
            this.position = position;
            this.bounds = new Rectangle(
                (int)(this.position.X - Cleanup.Instance.textureList[this.type].Width / 2),
                (int)(this.position.Y - Cleanup.Instance.textureList[this.type].Height / 2),
                (int)(Cleanup.Instance.textureList[this.type].Width),
                (int)(Cleanup.Instance.textureList[this.type].Height));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cleanup
{
    class SnakeBody
    {
        public int type;
        public Vector2 position;
        public Rectangle bounds;
        public Vector2 origin;

        public SnakeBody(Vector2 position, int type)
        {
            this.position = position;
            this.type = type;
            //this.texture = Game1.Instance.textureList[type];
            this.origin = new Vector2(Cleanup.Instance.textureList[type].Width / 2, Cleanup.Instance.textureList[type].Height / 2);
            this.updateBounds();

        }

        public void updateBounds()
        {
            this.bounds = new Rectangle(
                (int)(position.X - origin.X),
                (int)(position.Y - origin.Y),
                (int)(Cleanup.Instance.textureList[type].Width),
                (int)(Cleanup.Instance.textureList[type].Height));
        }

    }
}

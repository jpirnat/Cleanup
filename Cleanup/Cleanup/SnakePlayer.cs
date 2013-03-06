using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Cleanup
{

    static class Constants
    {
        public const int BODIES_BETWEEN_BAGS = 5;
        public const float DEFAULT_SPEED = 10.0f;
    }

    class SnakePlayer
    {
        public int id;
        public float direction;
        public float startDirection;
        public Vector2 startPosition;
        public Keys leftKey, rightKey, scrollKey, activateKey;
        public List<SnakeBody> body;
        public List<int> bodyTypes;
        public float speed;
        public int score;
        public bool alive;
        public bool diesThisRound;
        public int selectedPowerup;
        public TimeSpan powerupRemainingTime;
        public bool fireActive;

        public SnakePlayer(int id, float direction, Keys leftKey, Keys rightKey, Keys scrollKey, Keys activateKey, Vector2 position)
        {
            this.id = id;
            this.direction = direction;
            this.startDirection = direction;
            this.startPosition = position;
            this.leftKey = leftKey;
            this.rightKey = rightKey;
            this.scrollKey = scrollKey;
            this.activateKey = activateKey;
            this.body = new List<SnakeBody>();
            this.body.Add(new SnakeBody(position, id));
            this.bodyTypes = new List<int>();
            this.bodyTypes.Add(id);
            this.speed = Constants.DEFAULT_SPEED;
            //this.bodiesBetweenBags = Constants.BODIES_BETWEEN_BAGS;
            this.score = 500;
            this.alive = true;
            this.diesThisRound = false;
            this.selectedPowerup = 0;
            this.fireActive = false;
            /*this.fireRectangle = new RotatedRectangle(new Rectangle(
                (int)(body[0].position.X + Constants.BODIES_BETWEEN_BAGS * Constants.DEFAULT_SPEED * Math.Cos((double)direction)),
                (int)(body[0].position.Y + Constants.BODIES_BETWEEN_BAGS * Constants.DEFAULT_SPEED * Math.Sin((double)direction)),
                Cleanup.Instance.textureList[(int)Cleanup.Types.Fire].Width,
                Cleanup.Instance.textureList[(int)Cleanup.Types.Fire].Height),
                    this.direction,
                    this.body[0].origin);*/
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < bodyTypes.Count(); i++)
            {
                if (body.Count() > (i * Constants.BODIES_BETWEEN_BAGS))
                {
                    spriteBatch.Draw(Cleanup.Instance.textureList[bodyTypes[i]], body[i * Constants.BODIES_BETWEEN_BAGS].position, null, Color.White, (i == 0) ? direction : 0.0f, body[i].origin, 1.0f, SpriteEffects.None, 0.0f);
                }
            }
            if (this.fireActive)
            {
                spriteBatch.Draw(Cleanup.Instance.textureList[(int)Cleanup.Types.Fire], body[0].position + Constants.BODIES_BETWEEN_BAGS * speed * (new Vector2((float)Math.Cos((double)direction), (float)Math.Sin((double)direction))), null, Color.White, direction, body[0].origin, 1.0f, SpriteEffects.None, 0.0f);
                //spriteBatch.Draw(Cleanup.Instance.textureList[(int)Cleanup.Types.Fire], fireRectangle.CollisionRectangle, null, Color.White, direction, body[0].origin, SpriteEffects.None, 0.0f);
            }
        }

        public void CollidedWith(Entity entity)
        {
            if (entity.type == (int)Cleanup.Types.RecyclingBin)
            {
                for (int i = 1; i < this.bodyTypes.Count(); i++)
                {
                    this.score += ((((int)Cleanup.Types.BagYellow <= this.bodyTypes[i]) && (this.bodyTypes[i] <= (int)Cleanup.Types.BagPurple)) ? 20 : 10) * i;
                }

                this.bodyTypes.RemoveRange(1, this.bodyTypes.Count() - 1);
                this.body.RemoveRange(1, this.body.Count() - 1);
                this.selectedPowerup = 0;
            }
            else if (((int)Cleanup.Types.TrashYellow <= entity.type) && (entity.type <= (int)Cleanup.Types.TrashBlack))
            {
                this.bodyTypes.Add(entity.type - 6);
                Cleanup.Instance.entityList.Remove(entity);
            }
            else if (((int)Cleanup.Types.BagYellow <= entity.type) && (entity.type <= (int)Cleanup.Types.BagBlack))
            {
                Cleanup.Instance.entityList.Remove(entity);
                this.diesThisRound = true;
            }
            else if (entity.type == 1)
            { /* a player or garbagetail */
                this.diesThisRound = true;
                //this.respawn();
            }
            else if (entity.type == (int)Cleanup.Types.Wall)
            {
                this.diesThisRound = true;
                //this.respawn();
            }
            else if (entity.type == (int)Cleanup.Types.DeepWater)
            {
                this.diesThisRound = true;
                //this.respawn();
            }
            else if (entity.type == (int)Cleanup.Types.ShallowWater)
            {
                this.speed = Constants.DEFAULT_SPEED / 2;
                this.powerupRemainingTime = new TimeSpan(0, 0, 2);
                //this.respawn();
            }

        }

        public void respawn()
        {
            for (int i = 1; i < this.bodyTypes.Count(); i++)
            {
                if (this.body.Count() >= (i * Constants.BODIES_BETWEEN_BAGS))
                {
                    Cleanup.Instance.entityList.Add(new Entity(this.bodyTypes[i], this.body[i * Constants.BODIES_BETWEEN_BAGS].position));
                }
            }

            this.score -= 50;
            /*for (int i = 1; i < this.bodyTypes.Count(); i++)
            {
                this.score -= 10 * i;
            }*/
            this.body.Clear();
            this.bodyTypes.Clear();
            this.body.Add(new SnakeBody(startPosition, id));
            this.bodyTypes.Add(id);
            this.direction = startDirection;
            this.speed = Constants.DEFAULT_SPEED;
            this.fireActive = false;
            //this.bodiesBetweenBags = Constants.BODIES_BETWEEN_BAGS;
            this.diesThisRound = false;
            this.selectedPowerup = 0;
        }

        public void activatePowerup()
        {
            if (this.bodyTypes[this.selectedPowerup] == (int)Cleanup.Types.BagYellow)
            {
                this.bodyTypes[this.selectedPowerup] = (int)Cleanup.Types.BagBlack;
                this.speed = Constants.DEFAULT_SPEED;
                this.fireActive = true;
                this.selectedPowerup = 0;
                this.powerupRemainingTime = new TimeSpan(0, 0, 3);
            }
            else if (this.bodyTypes[this.selectedPowerup] == (int)Cleanup.Types.BagRed)
            {
                this.speed = Constants.DEFAULT_SPEED / 2;
                this.fireActive = false;
                //this.bodiesBetweenBags = Constants.BODIES_BETWEEN_BAGS * 2;
                this.bodyTypes[this.selectedPowerup] = (int)Cleanup.Types.BagBlack;
                this.selectedPowerup = 0;
                this.powerupRemainingTime = new TimeSpan(0, 0, 10);
            }
            else if (this.bodyTypes[this.selectedPowerup] == (int)Cleanup.Types.BagBlue)
            {
                this.bodyTypes[this.selectedPowerup] = (int)Cleanup.Types.BagBlack;
                this.selectedPowerup = 0;
            }
            else if (this.bodyTypes[this.selectedPowerup] == (int)Cleanup.Types.BagGreen)
            {
                this.speed = Constants.DEFAULT_SPEED * 1.5f;
                this.fireActive = false;
                //this.bodiesBetweenBags = (int)(((float)Constants.BODIES_BETWEEN_BAGS) / 1.5f);
                this.bodyTypes[this.selectedPowerup] = (int)Cleanup.Types.BagBlack;
                this.selectedPowerup = 0;
                this.powerupRemainingTime = new TimeSpan(0, 0, 10);
            }
            else if (this.bodyTypes[this.selectedPowerup] == (int)Cleanup.Types.BagPurple)
            {
                this.bodyTypes[this.selectedPowerup] = (int)Cleanup.Types.BagBlack;
                this.selectedPowerup = 0;
            }
        }
    }
}

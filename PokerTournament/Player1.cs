using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerTournament
{
    class Player1 : Player
    {
        Random rand;

        public Player1(int idNum, string nm, int mny) : base(idNum, nm, mny)
        {
            rand = new Random();
        }

        public override PlayerAction BettingRound1(List<PlayerAction> actions, Card[] hand)
        {
            throw new NotImplementedException();
        }

        public override PlayerAction BettingRound2(List<PlayerAction> actions, Card[] hand)
        {
            throw new NotImplementedException();
        }

        public override PlayerAction Draw(Card[] hand)
        {
            // Don't draw or discard all for testing.
            PlayerAction action = new PlayerAction(Name, "Draw", "draw", 0);
            int chance = rand.Next(0, 2);

            switch (chance)
            {
                case (1):
                    // Change nothing
                    break;
                case (0):
                    // discard all
                    action = new PlayerAction(Name, "Draw", "draw", 5);
                    break;
            }

            return action;
        }
    }
}

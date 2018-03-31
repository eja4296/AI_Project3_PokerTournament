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
            Card high;
            int rating = Evaluate.RateAHand(hand, out high);
            int cardsToDraw = 0;

            switch (rating)
            {
                // High Card
                case 1:
                    if (high.Value > 10)
                    {
                        // We have a high card of KJQA
                        for (int i = 0; i < hand.Length; i++)
                        {
                            if (hand[i].Value != high.Value)
                            {
                                hand[i] = null;
                            }
                        }
                        cardsToDraw = 4;
                    }
                    else
                    {
                        // Discard this whole trash hand.
                        for (int i = 0; i < hand.Length; i++)
                        {
                            hand[i] = null;
                        }
                        cardsToDraw = 5;
                    }
                    break;

                // 1 Pair
                case 2:
                    // Find the location of the pair
                    // Find the highest non-pair card
                    // If it's over 10, keep it and discard rest, if not, discard everything that isn't the pair
                    break;

                // 2 Pair
                case 3:
                    // Find the location of the pairs
                    // Discard the other card
                    break;

                // 3 of a kind
                case 4:
                    // Find the location of the three
                    // Find the highest non-three card
                    // If it's over 10, keep it and discard other, if not, discard everything that isn't the three
                    break;

                // Straight
                case 5:
                    // Do nothing, as we don't want to discard
                    cardsToDraw = 0;
                    break;

                // Flush
                case 6:
                    // Do nothing, as we don't want to discard
                    cardsToDraw = 0;
                    break;

                // Full House
                case 7:
                    // Do nothing, as we don't want to discard
                    cardsToDraw = 0;
                    break;

                // Four of a Kind
                case 8:
                    // Find the 4 cards
                    // Discard the other
                    break;

                // Straight Flush
                case 9:
                    // Do nothing, as we don't want to discard
                    cardsToDraw = 0;
                    break;

                // Royal Flush
                case 10:
                    // Do nothing, as we don't want to discard
                    cardsToDraw = 0;
                    break;
            }

            ///// Don't draw or discard all for testing.
            ///int chance = rand.Next(0, 2);
            ///
            ///switch (chance)
            ///{
            ///    case (1):
            ///        // Change nothing
            ///        break;
            ///    case (0):
            ///        // discard all
            ///        action = new PlayerAction(Name, "Draw", "draw", 5);
            ///        break;
            ///}

            PlayerAction action = new PlayerAction(Name, "Draw", "draw", cardsToDraw);
            return action;
        }
    }
}

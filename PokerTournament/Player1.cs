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
                    int pairOneInd = 0;
                    int pairTwoInd = 0;
                    for (int x = 0; x < hand.Length; x++)
                    {
                        for (int y = 0; y < hand.Length; y++)
                        {
                            if (y != x && hand[y].Value == hand[x].Value)
                            {
                                pairOneInd = x;
                                pairTwoInd = y;
                            }
                        }
                    }

                    // Find the highest non-pair card
                    int compareValue = 0;
                    int highInd = 0;
                    for (int i = 0; i < hand.Length; i++)
                    {
                        if (i != pairOneInd && i != pairTwoInd && hand[i].Value > compareValue)
                        {
                            compareValue = hand[i].Value;
                            highInd = i;
                        }
                    }

                    // If it's over 10, keep it and discard rest, if not, discard everything that isn't the pair
                    if (compareValue > 10)
                    {
                        for (int i = 0; i < hand.Length; i++)
                        {
                            if (i != pairOneInd && i != pairTwoInd && i != highInd)
                            {
                                hand[i] = null;
                            }
                        }
                        cardsToDraw = 2;
                    }
                    else
                    {
                        for (int i = 0; i < hand.Length; i++)
                        {
                            if (i != pairOneInd && i != pairTwoInd)
                            {
                                hand[i] = null;
                            }
                        }
                        cardsToDraw = 3;
                    }
                    break;

                // 2 Pair
                case 3:
                    // Find the location of non-pair card and discard
                    bool match;
                    int currTestInd = -1;
                    do
                    {
                        match = false;
                        currTestInd++;
                        for (int i = 0; i < hand.Length; i++)
                        {
                            if (hand[currTestInd].Value == hand[i].Value)
                            {
                                match = true;
                            }
                        }
                    }
                    while (match == true);

                    hand[currTestInd] = null;
                    cardsToDraw = 1;

                    break;

                // 3 of a kind
                case 4:
                    // Find the location of non-three cards
                    int notMatchingOne = -1;
                    int notMatchingTwo = -1;

                    for (int x = 0; x < hand.Length; x++)
                    {
                        match = false;
                        for (int y = 0; y < hand.Length; y++)
                        {
                            if (hand[x].Value == hand[y].Value)
                            {
                                match = true;
                            }
                        }
                        if (match == false)
                        {
                            if (notMatchingOne == -1)
                            {
                                notMatchingOne = x;
                            }
                            else
                            {
                                notMatchingTwo = x;
                            }
                        }
                    }

                    // Discard both
                    hand[notMatchingOne] = null;
                    hand[notMatchingTwo] = null;
                    cardsToDraw = 2;
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
                    // Find the location of non-pair card and discard
                    currTestInd = -1;
                    do
                    {
                        match = false;
                        currTestInd++;
                        for (int i = 0; i < hand.Length; i++)
                        {
                            if (hand[currTestInd].Value == hand[i].Value)
                            {
                                match = true;
                            }
                        }
                    }
                    while (match == true);

                    if (hand[currTestInd].Value > 10)
                    {
                        cardsToDraw = 0;
                    }
                    else
                    {
                        hand[currTestInd] = null;
                        cardsToDraw = 1;
                    }
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

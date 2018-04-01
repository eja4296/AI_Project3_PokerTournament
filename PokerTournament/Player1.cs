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
        float[] handPercentages;
        float[,] handMovementPercentages;
        float[,] handPercentagesByCardCount;
        float[] handSpeculativeValues;
        float baseBetValue = 10;
        float opponentCurrentRoundBet = 0;
        float opponentCurrentRoundBetCount = 0;
        int previousMoney = -1;
        int previousHandRating = 0;

        private void LogOpponentBet(float amt) {
            opponentCurrentRoundBet += amt;
            opponentCurrentRoundBetCount++;
        }

        private void LogPreviousRound() {
            float averageOpponentBetFromPreviousRound = (opponentCurrentRoundBetCount > 0) ? opponentCurrentRoundBet / opponentCurrentRoundBetCount : 0;

            // If this is the first round initialize some temp values
            if (previousMoney == -1) {
                previousMoney = Money;
                return;
            }

            // If we lost the previous round, assume the opponent's rating was halfway between ours and the max
            // If we won, assume it was halfway between ours and the min
            float speculativeOpponentRating = (previousMoney > Money) ? (10 + previousHandRating) / 2 : (1 + previousHandRating) / 2;

            float speculativeOpponentBaseBet = averageOpponentBetFromPreviousRound / speculativeOpponentRating;

            // Our base bet should seek towards what we think the opponent's is if we were able to record any bets from them this round
            if(speculativeOpponentRating != 0)
                baseBetValue = (baseBetValue + speculativeOpponentBaseBet) / 2;

            // Reset per-round values
            opponentCurrentRoundBet = 0;
            opponentCurrentRoundBetCount = 0;
        }

        private float OddsOfDrawing(int rating, int numberOfCards) {
            return handPercentagesByCardCount[numberOfCards - 1, rating - 1];
        }

        private float GetSpeculativeValueOfHand(int currentRating) {
            return handSpeculativeValues[currentRating - 1];
        }

        public Player1(int idNum, string nm, int mny) : base(idNum, nm, mny)
        {
            rand = new Random();
            handPercentages = new float[]{ 50.1177f, 42.2569f, 4.7539f, 2.1128f, 0.3925f, 0.1965f, 0.1441f, 0.024f, 0.00139f, 0.000154f };

            // Odds of getting a hand with a particular rating from a particular hand size
            // Slightly inaccurate due to not accounting for the fact that some of the cards will already be in our hand
            handPercentagesByCardCount = new float[,] {
                // high card    pair        2 pair      3 of a kind straight    flush       full house  4 of a kind s. flush    r. flush
                { 1,            0,          0,          0,          0,          0,          0,          0,          0,          0 }, // 1 card hand
                { .94117f,      .058823f,   0,          0,          0,          0,          0,          0,          0,          0 }, // 2 card hand
                { .82588f,      .171764f,   0,          .002352f,   0,          0,          0,          0,          0,          0 },
                { .67611f,      .30424f,    .01037f,    .00921f,    0,          0,          0,          .000048f,   0,          0 },
                { .501177f,     .422569f,   .047539f,   .021128f,   .003925f,   .001965f,   .001441f,   .00024f,    .0000139f,  .00000154f }
            };

            // Odds of going from the first rating to the second after draw action
            // We don't discard with a straight or better, so this only goes up to three of a kind
            // These are probably wrong to some degree, but we don't need perfect accuracy
            handMovementPercentages = new float[,] {
                { // from 1 - high card to
                    .77863f, // high card
                    .191663f, // pair
                    OddsOfDrawing(2, 4) * .12234f, // 2 pair
                    .02708f, // 3 of a kind
                    0, // straight
                    0, // flush
                    0, // full house
                    0, // 4 of a kind
                    0, // straight flush
                    0  // royal flush
                },
                { // from 2 - pair to
                    0, // high card
                    1 - (OddsOfDrawing(2, 3) + .115102f + OddsOfDrawing(4, 3) + .004897f), // pair
                    OddsOfDrawing(2, 3), // 2 pair
                    .115102f, // 3 of a kind
                    0, // straight
                    0, // flush
                    OddsOfDrawing(4, 3), // full house
                    .004897f, // 4 of a kind
                    0, // straight flush
                    0  // royal flush
                },
                { // from 3 - two pair to
                    0, // high card
                    0, // pair
                    1 - .0833333f, // 2 pair
                    0, // 3 of a kind
                    0, // straight
                    0, // flush
                    .0833333f, // full house
                    0, // 4 of a kind
                    0, // straight flush
                    0  // royal flush
                },
                { // from 4 - 3 of a kind to
                    0, // high card
                    0, // pair
                    0, // 2 pair
                    1 - (OddsOfDrawing(2, 2) + .06251f), // 3 of a kind
                    0, // straight
                    0, // flush
                    OddsOfDrawing(2, 2), // full house
                    .06251f, // 4 of a kind
                    0, // straight flush
                    0  // royal flush
                }, 
            };

            handSpeculativeValues = new float[10];

            // Calculate the value of a given hand with the possible change from draws taken into account
            for(int c = 0; c < handMovementPercentages.GetLength(0); c++) {
                handSpeculativeValues[c] = 0;
                for(int n = 0; n < handMovementPercentages.GetLength(1); n++) {
                    handSpeculativeValues[c] += handMovementPercentages[c, n] * (n + 1);
                }
            }

            // Straights and better don't discard, so their values are what they already are
            for(int c = handMovementPercentages.GetLength(0); c < 10; c++) {
                handSpeculativeValues[c] = c + 1;
            }
        }

        public override PlayerAction BettingRound1(List<PlayerAction> actions, Card[] hand)
        {
            LogPreviousRound();
            float amountBet = baseBetValue;
            string actionName = "bet";

            // Get hand rating
            Card high;
            float rating = GetSpeculativeValueOfHand(Evaluate.RateAHand(hand, out high));
            amountBet *= rating;
            

            // Other player has gone first, but do something
            if (actions.Count != 0)
            {
                foreach(PlayerAction pa in actions)
                {

                    // If the action is not our player's, and the action is not draw
                    if (pa.Name != "Player1" && pa.ActionPhase != "Draw")
                    {
                        // For Bet1
                        if (pa.ActionPhase == "Bet1")
                        {
                            // Other player has bet
                            if (pa.ActionName == "bet")
                            {
                                // player must call, raise, or fold
                                // Just call for now
                                actionName = "call";
                                amountBet = pa.Amount;
                                LogOpponentBet(pa.Amount);
                            }
                            else if (pa.ActionName == "check")
                            {
                                // Player must bet
                                actionName = "bet";
                            }
                            else if (pa.ActionName == "call")
                            {
                                // Do nothing?
                            }
                            else if (pa.ActionName == "raise")
                            {
                                // Player should call or fold
                                // Just call for now
                                actionName = "call";
                                amountBet = pa.Amount;
                            }
                            // Opponent folded
                            else
                            {
                                // Do nothing
                            }
                        }

                        // For Bet2
                        if (pa.ActionPhase == "Bet2")
                        {
                            // Other player has bet
                            if (pa.ActionName == "bet")
                            {
                                // player must call, raise, or fold
                                // Just call for now
                                actionName = "call";
                                amountBet = pa.Amount;
                                LogOpponentBet(pa.Amount);
                            }
                            else if (pa.ActionName == "check")
                            {
                                // Player must bet
                                actionName = "bet";
                            }
                            else if (pa.ActionName == "call")
                            {
                                // Do nothing?
                            }
                            else if (pa.ActionName == "raise")
                            {
                                // Player should call or fold
                                // Just call for now
                                actionName = "call";
                                amountBet = pa.Amount;
                            }
                            // Opponent folded
                            else
                            {
                                // Do nothing
                            }
                        }

                    }
                }
                
            }
            // Our player goes first, must check or bet
            else
            {
                actionName = "bet";
            }

            // Player can bet, check, raise, fold, call
            PlayerAction action = new PlayerAction(Name, "Bet1", actionName, (int)Math.Round(amountBet));
            return action;
        }

        public override PlayerAction BettingRound2(List<PlayerAction> actions, Card[] hand)
        {
            // Just use the same strategy for round 2
            PlayerAction action = BettingRound1(actions, hand);

            // create a new PlayerAction object
            return new PlayerAction(action.Name, "Bet2", action.ActionName, action.Amount);

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

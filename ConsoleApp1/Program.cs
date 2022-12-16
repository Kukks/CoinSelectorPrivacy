using System.Diagnostics;
using System.Text;

namespace CoinSelection
{
    public class SubsetSolution:IEquatable<SubsetSolution>
    {
        public TimeSpan TimeElapsed { get; set; }
        public List<Coin> Coins { get; set; } = new();
        public List<Payment> HandledPayments { get; set; } = new();

        public decimal TotalValue => Coins.Sum(coin => coin.Value);
        public decimal TotalPaymentCost => HandledPayments.Sum(payment => payment.Value);
        public decimal LeftoverValue => TotalValue - TotalPaymentCost;

        public decimal Score()
        {
            var score = 0m;

            decimal ComputeCoinScore(List<Coin> coins)
            {
                var w = 0m;
                foreach (var smartCoin in coins)
                {
                    if (smartCoin.AnonymitySet <= 0)
                    {
                        w += smartCoin.Value;
                    }
                    else
                    {
                        w += smartCoin.Value / (decimal) smartCoin.AnonymitySet;
                    }
                }

                return w / (coins.Count == 0 ? 1 : coins.Count);
            }

            decimal ComputePaymentScore(List<Payment> pendingPayments)
            {
                return pendingPayments.Count;
            }

            score += ComputeCoinScore(Coins);
            score += ComputePaymentScore(HandledPayments);

            return score;
        }


        public string GetId()
        {
            return string.Join("-",
                Coins.OrderBy(coin => coin.Name).Select(coin => coin.Name)
                    .Concat(HandledPayments.OrderBy(arg => arg.Value).Select(p => p.Value.ToString())));

        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                $"Solution({GetId()}) total value: {TotalValue} total payments:{TotalPaymentCost} leftover: {LeftoverValue} score: {Score()} Compute time: {TimeElapsed} ");
            sb.AppendLine($"Used coins: {string.Join(", ", Coins.Select(coin => coin.Name + " " + coin.Value + " A"+coin.AnonymitySet))}");
            sb.AppendLine($"handled payments: {string.Join(", ", HandledPayments.Select(p => p.Value))}");
            return sb.ToString();
        }

        public bool Equals(SubsetSolution? other)
        {
            return GetId() == other?.GetId();
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubsetSolution) obj);
        }
    }

    public class Payment
    {
        public decimal Value { get; set; }

        public Payment(decimal value)
        {
            Value = value;
        }
    }

    public enum AnonsetType
    {
        Red,
        Orange,
        Green
    }

    public class Coin
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public double AnonymitySet { get; set; }
        public string TransactionId { get; set; }

        public Coin(string name, int value, int anonymitySet)
        {
            Name = name;
            Value = value;
            AnonymitySet = anonymitySet;
        }

        public static AnonsetType CoinColor(Coin coin, int anonsetTarget)
        {
            return coin.AnonymitySet <= 0 ? AnonsetType.Red :
                coin.AnonymitySet >= anonsetTarget ? AnonsetType.Green : AnonsetType.Orange;
        }
    }

    class Program
    {
        private static IEnumerable<Coin> Generate(int num)
        {
            Console.WriteLine($"generating {num} coins");
            
            for (var i = 0; i < num; i++)
            {
                 yield return new Coin("Coin " + i, Random.Shared.Next(1, 100), Random.Shared.Next(0, 3));
            }
        }
        private static IEnumerable<Payment> GenerateP(int num)
        {
            
            Console.WriteLine($"generating {num} payments");
            for (var i = 0; i < num; i++)
            {
                 yield return new Payment(Random.Shared.Next(1,100));
            }
        }

        static void Main(string[] args)
        {
            // // Create a list of coins
            // List<Coin> coins = new List<Coin>
            // {
            //     new Coin("coin1", 10, 0),
            //     new Coin("coin2", 5, 1),
            //     new Coin("coin3", 15, 2),
            //     new Coin("coin4", 20, 0),
            //     new Coin("coin5", 5, 3)
            // };
            //
            // // a list of payments we want to do 
            // List<Payment> payments = new List<Payment>
            // {
            //     new Payment(5),
            //     new Payment(2),
            //     new Payment(5),
            //     new Payment(3),
            // };
            List<Coin> coins = Generate(Random.Shared.Next(1, 50)).ToList();
            List<Payment> payments = GenerateP(Random.Shared.Next(0, 0)).ToList();

            var solutions = new List<SubsetSolution>();
            for (int i = 0; i < 100; i++)
            {
                var solution = SelectCoins(coins, payments, 2, 3, new Dictionary<AnonsetType, int>()
                {
                    {AnonsetType.Red, 1999}
                });
                Console.WriteLine($"Attempting solution #{i}");
                Console.Write(solution);
                solutions.Add(solution);
            }

            solutions = solutions.DistinctBy(solution => solution.GetId()).ToList();
            
            Console.WriteLine($"removed duplicate solutions, now there are {solutions.Count} solutions");
            
            
            Console.WriteLine("best solution(s):");
            foreach (var subsetSolution in solutions.GroupBy(solution => solution.Score()).OrderByDescending(grouping => grouping.Key).First())
            {
                Console.WriteLine(subsetSolution);
            }
           

            

        }
        public static IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Shared.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        static List<T> SlightlyShiftOrder<T>(List<T> list, int chanceOfShiftPercentage)
        {
            // Create a random number generator
            var rand = new Random();
            List<T> workingList = new List<T>(list);
// Loop through the coins and determine whether to swap the positions of two consecutive coins in the list
            for (int i = 0; i < workingList.Count() - 1; i++)
            {
                // If a random number between 0 and 1 is less than or equal to 0.1, swap the positions of the current and next coins in the list
                if (rand.NextDouble() <= chanceOfShiftPercentage / 100)
                {
                    // Swap the positions of the current and next coins in the list
                    var temp = workingList[i];
                    workingList[i] = workingList[i + 1];
                    workingList[i + 1] = temp;
                }
            }

            return workingList;
        }

        private static List<Coin> RandomizeCoins(List<Coin> coins, int anonsetTarget)
        {
            var remainingCoins = new List<Coin>(coins);
            var workingList = new List<Coin>();
            while (remainingCoins.Any())
            {
                var currentCoin = remainingCoins.First();
                remainingCoins.RemoveAt(0);
                var lastCoin = workingList.LastOrDefault();
                if (lastCoin is null || Coin.CoinColor(currentCoin, anonsetTarget) == AnonsetType.Green || !remainingCoins.Any() ||
                    (remainingCoins.Count == 1 && remainingCoins.First().TransactionId == currentCoin.TransactionId) ||
                    lastCoin.TransactionId != currentCoin.TransactionId || Random.Shared.Next(1, 10) < 5)
                {
                    workingList.Add(currentCoin);
                }
                else
                {
                    remainingCoins.Insert(1, currentCoin);
                }
            }


            return workingList.ToList();
        }

        static SubsetSolution SelectCoins(List<Coin> coins, List<Payment> pendingPayments, int anonScoreTarget,
            int maxCoins,
            Dictionary<AnonsetType, int> maxPerType)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Sort the coins in descending order by their value and then slightly randomize
            var remainingCoins = SlightlyShiftOrder(RandomizeCoins(
                coins.OrderBy(coin => Coin.CoinColor(coin, anonScoreTarget)).ThenByDescending(x => x.Value).ToList(),
                anonScoreTarget), 10);
            var remainingPendingPayments = new List<Payment>(pendingPayments);
            var solution = new SubsetSolution();

            while (remainingCoins.Any())
            {
                var coin = remainingCoins.First();
                var color = Coin.CoinColor(coin, anonScoreTarget);
                // If the selected coins list is at its maximum size, break out of the loop
                if (solution.Coins.Count == maxCoins)
                {
                    break;
                }

                remainingCoins.Remove(coin);
                if (maxPerType.TryGetValue(color, out var maxColor) &&
                    solution.Coins.Count(coin1 => Coin.CoinColor(coin1, anonScoreTarget) == color) == maxColor)
                {
                    
                    continue;
                }

                solution.Coins.Add(coin);

                // Loop through the pending payments and handle each payment by subtracting the payment amount from the total value of the selected coins
                var potentialPayments =
                    
                    Shuffle(remainingPendingPayments.Where(payment => payment.Value <= solution.LeftoverValue).ToList());
                while (potentialPayments.Any())
                {
                    var payment = potentialPayments.First();
                    solution.HandledPayments.Add(payment);
                    remainingPendingPayments.Remove(payment);
                    potentialPayments = Shuffle(remainingPendingPayments.Where(payment => payment.Value <= solution.LeftoverValue).ToList());
                }

                if (!remainingPendingPayments.Any())
                {
//if we've handled all payments, 
                    remainingCoins.RemoveAll(coin1 =>
                    {
                        var remainingCoinColor = Coin.CoinColor(coin1, anonScoreTarget);
                        switch (remainingCoinColor)
                        {
                            case AnonsetType.Red:
                            case AnonsetType.Orange:
                                //we still need to mix these coins more later anyway,
                                //so let's check how many coins we are allowed to add max and how many we added, and use that percentage as the random chance of not adding it.
                                var maxCoinCapacity = ((solution.Coins.Count / maxCoins) * 100) * 100;

                                return Random.Shared.Next(1, 100) > maxCoinCapacity;
                            //let's not mix green coins for no reason, except for some randomness. we filter out ~80% at random on each coin addition selection now)
                            case AnonsetType.Green:
                                return Random.Shared.Next(1, 10) < 8;
                        }

                        return false;
                    });
                }
            }
            stopwatch.Stop();
            solution.TimeElapsed = stopwatch.Elapsed;
            return solution;
        }
    }
}
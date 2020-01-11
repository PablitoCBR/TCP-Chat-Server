namespace Core.Services.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Core.Services.Security.Interfaces;

    public class PrimeNumberGenerator : IPrimeNumberGenerator
    {
        private readonly int[] _primeNumbers;

        public PrimeNumberGenerator()
        {
            _primeNumbers = GeneratePrimeNumbers(minValue: 1000000000);
        }

        public int GetRandomPrimeNumber() => _primeNumbers[new Random().Next(_primeNumbers.Length)];

        private int[] GeneratePrimeNumbers(int minValue = 0, int maxValue = Int32.MaxValue)
        {
            IList<int> primes = new List<int> { 2 };
            int nextPrimeNumber = 3;

            do
            {
                int sqrt = (int)Math.Sqrt(nextPrimeNumber);
                bool isPrime = true;
                for (int i = 0; primes[i] < sqrt; i++)
                {
                    if (nextPrimeNumber % primes[i] == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }

                if (isPrime)
                    primes.Add(nextPrimeNumber);
            }
            while (this.TryAdd(ref nextPrimeNumber, 2, maxValue));

            return primes.Where(prime => prime > minValue).ToArray();
        }

        private bool TryAdd(ref int number, int value, int limit)
        {
            if (limit - value >= number)
            {
                number += value;
                return true;
            }
            else return false;
        }
    }
}

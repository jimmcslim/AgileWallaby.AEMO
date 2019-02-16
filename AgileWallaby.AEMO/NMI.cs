using System;

namespace AgileWallaby.AEMO
{
    public sealed class NMI
    {
        public NMI(string nmi)
        {
            if (string.IsNullOrWhiteSpace(nmi))
            {
                throw new ArgumentOutOfRangeException(nameof(nmi), "NMI is null.");
            }
            
            if (nmi.Length != 10 && nmi.Length != 11)
            {
                throw new ArgumentOutOfRangeException(nameof(nmi), "NMI should be 10 or 11 characters in length.");
            }

            if (nmi.ToUpper().IndexOfAny(new[] {'I', 'O'}) >= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nmi), "NMI cannot contain I or O, ambiguous with 1 and 0.");
            }
            
            if (nmi.Length == 10)
            {
                Checksum = CalculateChecksum(nmi);
            }
            else
            {
                var expectedChecksum = CalculateChecksum(nmi.Substring(0, 10));
                var actualChecksum = int.Parse(nmi[10].ToString());
                if (expectedChecksum != actualChecksum)
                {
                    throw new ArgumentException(nameof(nmi), 
                        $"Checksum is incorrect, expected {expectedChecksum}, was {actualChecksum}.");
                }

                Checksum = actualChecksum;
            }
            
            Value = nmi.ToUpper();
        }

        public string Value { get; }
        public int Checksum { get; }

        public string Base => Value.Substring(0, 10);

        public string Full => $"{Value.Substring(0, 10)}{(Checksum)}";

        //Adapted from sample implementation at https://bit.ly/2GKxM9E
        private static int CalculateChecksum(string nmi)
        {
            var checksum = 0;
            var v = 0;
            var multiply = true;
            for (var i = nmi.Length; i > 0; i--)
            {
                var d = (int)nmi[i - 1];
                if (multiply) { d *= 2; }
                multiply = !multiply;
                while (d > 0)
                {
                    v += d % 10;
                    d /= 10;
                }
            }
            checksum = (10 - v % 10) % 10;
            return checksum;
        }

        private bool Equals(NMI other)
        {
            return string.Equals(Base, other.Base, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((NMI) obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Base);
        }
    }
}
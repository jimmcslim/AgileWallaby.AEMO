using System;
using Xunit;

namespace AgileWallaby.AEMO.Tests
{
    public class NMITests
    {
        [Fact]
        public void Encapsulates_A_NMI()
        {
            var nmi = new NMI("1234567890");
            Assert.Equal("1234567890", nmi.Value);
        }

        [Fact]
        public void Rejects_Null_Empty_Or_Non_Valid_Length_NMIs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new NMI(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NMI(string.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NMI("123456789"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NMI("123456789012"));
        }

        [Theory]
        // ReSharper disable StringLiteralTypo
        // Sample NMIs from https://bit.ly/2GKxM9E
        [InlineData("2001985732", 8)]
        [InlineData("QAAAVZZZZZ", 3)]
        [InlineData("2001985733", 6)]
        [InlineData("QCDWW00010", 2)]
        [InlineData("3075621875", 8)]
        [InlineData("SMVEW00085", 8)]
        [InlineData("3075621876", 6)]
        [InlineData("VAAA000065", 7)]
        [InlineData("4316854005", 9)]
        [InlineData("VAAA000066", 5)]
        [InlineData("4316854006", 7)]
        [InlineData("VAAA000067", 2)]
        [InlineData("6305888444", 6)]
        [InlineData("VAAASTY576", 8)]
        [InlineData("6350888444", 2)]
        [InlineData("VCCCX00009", 1)]
        [InlineData("7001888333", 8)]
        [InlineData("VEEEX00009", 1)]
        [InlineData("7102000001", 7)]
        [InlineData("VKTS786150", 2)]
        [InlineData("NAAAMYS582", 6)]
        [InlineData("VKTS867150", 5)]
        [InlineData("NBBBX11110", 0)]
        [InlineData("VKTS871650", 7)]
        [InlineData("NBBBX11111", 8)]
        [InlineData("VKTS876105", 7)]
        [InlineData("NCCC519495", 5)]
        [InlineData("VKTS876150", 3)]
        [InlineData("NGGG000055", 4)]
        [InlineData("VKTS876510", 8)]
        // ReSharper restore StringLiteralTypo
        public void Accepts_11_Digit_NMI_With_Valid_Checksum(string exampleNMI, int exampleChecksum)
        {
            Assert.Equal(exampleChecksum, new NMI(exampleNMI).Checksum);   
        }

        [Theory]
        [InlineData("NMI1234567")]
        [InlineData("NMO1234567")]
        [InlineData("NM11234567O")]
        [InlineData("NM11234567I")]
        public void Rejects_NMI_With_I_Or_O(string nmiWithInvalidCharacter)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new NMI(nmiWithInvalidCharacter));
        }

        [Theory]
        [InlineData("12345678900")]
        [InlineData("12345678901")]
        [InlineData("12345678902")]
        [InlineData("12345678903")]
        [InlineData("12345678904")]
        [InlineData("12345678905")]
        [InlineData("12345678906")]
        // 12345678907 is valid.
        [InlineData("12345678908")]
        [InlineData("12345678909")]
        public void Rejects_11_Digit_NMI_With_Invalid_Checksum(string nmiWithInvalidChecksum)
        {
            Assert.Throws<ArgumentException>(() => new NMI(nmiWithInvalidChecksum));
        }

        [Fact]
        public void Can_Provide_NMI_Inclusive_Or_Exclusive_Of_Checksum()
        {
            var nmiWithoutChecksum = new NMI("2001985732"); // Checksum is 8.
            Assert.Equal(8, nmiWithoutChecksum.Checksum);
            
            Assert.Equal("2001985732", nmiWithoutChecksum.Base);
            Assert.Equal("20019857328", nmiWithoutChecksum.Full);
            
            var nmiWithChecksum = new NMI("20019857328");
            Assert.Equal("2001985732", nmiWithChecksum.Base);
            Assert.Equal("20019857328", nmiWithChecksum.Full);
        }

        [Theory]
        [InlineData("2001985732", "2001985732", true)]
        [InlineData("2001985732", "20019857328", true)]
        [InlineData("20019857328", "2001985732", true)]
        // ReSharper disable StringLiteralTypo
        [InlineData("QAAAVZZZZZ", "2001985732", false)]
        [InlineData("2001985732", "QAAAVZZZZZ", false)]
        [InlineData(null, "QAAAVZZZZZ", false)]
        [InlineData("QAAAVZZZZZ", null, false)]
        // ReSharper restore StringLiteralTypo
        [InlineData(null, null, true)]
        public void Implements_Equality(string lhs, string rhs, bool areEqual)
        {
            var lhsNmi = lhs == null ? null : new NMI(lhs);
            var rhsNmi = rhs == null ? null : new NMI(rhs);
            
            Assert.Equal(Equals(lhsNmi, rhsNmi), areEqual);
            Assert.Equal(lhsNmi?.GetHashCode() == rhsNmi?.GetHashCode(), areEqual);
        }
    }
}    
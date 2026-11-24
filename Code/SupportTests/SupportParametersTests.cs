using System.Linq;
using SupportApp;
using Xunit;

namespace SupportTests
{
    public class SupportParametersTests
    {
        [Fact]
        public void DefaultParameters_AreValid()
        {
            var parameters = SupportParameters.Default();

            Assert.Empty(parameters.Validate());
        }

        [Theory]
        [InlineData(60, 50, 100, 20, 20, 20)]
        [InlineData(150, 100, 300, 100, 80, 80)]
        public void BoundaryParameters_AreValid(
            double supportLength,
            double supportWidth,
            double supportHeight,
            double grooveLength,
            double grooveHeight,
            double grooveDepth)
        {
            var parameters = new SupportParameters
            {
                SupportLength = supportLength,
                SupportWidth = supportWidth,
                SupportHeight = supportHeight,
                GrooveLength = grooveLength,
                GrooveHeight = grooveHeight,
                GrooveDepth = grooveDepth
            };

            Assert.Empty(parameters.Validate());
        }

        [Theory]
        [InlineData(59.9)]
        [InlineData(150.1)]
        public void LengthOutOfRange_IsReported(double supportLength)
        {
            var parameters = new SupportParameters { SupportLength = supportLength };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.SupportLength);
        }

        [Theory]
        [InlineData(49.9)]
        [InlineData(100.1)]
        public void WidthOutOfRange_IsReported(double supportWidth)
        {
            var parameters = new SupportParameters { SupportWidth = supportWidth };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.SupportWidth);
        }

        [Theory]
        [InlineData(99.9)]
        [InlineData(300.1)]
        public void HeightOutOfRange_IsReported(double supportHeight)
        {
            var parameters = new SupportParameters { SupportHeight = supportHeight };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.SupportHeight);
        }

        [Theory]
        [InlineData(19.9)]
        [InlineData(100.1)]
        public void GrooveLengthOutOfRange_IsReported(double grooveLength)
        {
            var parameters = new SupportParameters { GrooveLength = grooveLength };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveLength);
        }

        [Theory]
        [InlineData(19.9)]
        [InlineData(80.1)]
        public void GrooveHeightOutOfRange_IsReported(double grooveHeight)
        {
            var parameters = new SupportParameters { GrooveHeight = grooveHeight };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveHeight);
        }

        [Theory]
        [InlineData(19.9)]
        [InlineData(80.1)]
        public void GrooveDepthOutOfRange_IsReported(double grooveDepth)
        {
            var parameters = new SupportParameters { GrooveDepth = grooveDepth };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveDepth);
        }

        [Fact]
        public void GrooveLengthExceedsLimit_IsReportedWithHints()
        {
            var parameters = new SupportParameters
            {
                SupportLength = 80,
                GrooveLength = 70
            };

            var errors = parameters.Validate();
            var error = Assert.Single(errors.Where(
                e => e.Parameter == SupportParameter.GrooveLength));

            Assert.Contains("60", error.Message);
            Assert.Contains("90", error.Message);
        }

        [Fact]
        public void GrooveLengthEqualToLimit_IsValid()
        {
            var parameters = new SupportParameters
            {
                SupportLength = 80,
                GrooveLength = 60
            };

            Assert.DoesNotContain(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveLength);
        }

        [Fact]
        public void GrooveHeightExceedsLimit_IsReported()
        {
            var parameters = new SupportParameters
            {
                SupportHeight = 100,
                GrooveHeight = 61
            };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveHeight);
        }

        [Fact]
        public void GrooveDepthExceedsLimit_IsReported()
        {
            var parameters = new SupportParameters
            {
                SupportWidth = 50,
                GrooveDepth = 41
            };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveDepth);
        }

        [Fact]
        public void TechAssignmentExample_IsValid()
        {
            var parameters = new SupportParameters
            {
                SupportLength = 80,
                SupportWidth = 50,
                SupportHeight = 100,
                GrooveLength = 60,
                GrooveHeight = 50,
                GrooveDepth = 40
            };

            Assert.Empty(parameters.Validate());
        }

        [Fact]
        public void TechAssignmentExample_WithGrooveLength70_IsInvalid()
        {
            var parameters = new SupportParameters
            {
                SupportLength = 80,
                SupportWidth = 50,
                SupportHeight = 100,
                GrooveLength = 70,
                GrooveHeight = 50,
                GrooveDepth = 40
            };

            Assert.NotEmpty(parameters.Validate());
        }

        [Fact]
        public void DefaultParameters_AreNotThroughSlot()
        {
            Assert.False(SupportParameters.Default().ThroughSlot);
        }

        [Fact]
        public void ThroughSlot_IgnoresDepthValidation()
        {
            var parameters = new SupportParameters
            {
                SupportLength = 80,
                SupportWidth = 50,
                SupportHeight = 100,
                GrooveLength = 60,
                GrooveHeight = 50,
                GrooveDepth = 80,
                ThroughSlot = true
            };

            Assert.Empty(parameters.Validate());
        }

        [Theory]
        [InlineData(79.9)]
        [InlineData(80.1)]
        public void ThroughSlot_AnyDepthValueIsValid(double grooveDepth)
        {
            var parameters = new SupportParameters
            {
                SupportWidth = 60,
                GrooveDepth = grooveDepth,
                ThroughSlot = true
            };

            Assert.DoesNotContain(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveDepth);
        }

        [Fact]
        public void ThroughSlot_StillValidatesGrooveLength()
        {
            var parameters = new SupportParameters
            {
                SupportLength = 80,
                GrooveLength = 70,
                ThroughSlot = true
            };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveLength);
        }

        [Fact]
        public void ThroughSlot_StillValidatesGrooveHeight()
        {
            var parameters = new SupportParameters
            {
                SupportHeight = 100,
                GrooveHeight = 61,
                ThroughSlot = true
            };

            Assert.Contains(parameters.Validate(),
                e => e.Parameter == SupportParameter.GrooveHeight);
        }

        [Fact]
        public void ValidationError_ContainsParameterAndMessage()
        {
            var parameters = new SupportParameters { SupportLength = 10 };

            var error = Assert.Single(parameters.Validate());

            Assert.Equal(SupportParameter.SupportLength, error.Parameter);
            Assert.Contains("длины опоры L", error.Message);
        }

        [Fact]
        public void Validate_DoesNotDuplicateDependencyError_WhenBothParametersAreInvalid()
        {
            var parameters = new SupportParameters
            {
                SupportLength = 50,
                GrooveLength = 120
            };

            var errors = parameters.Validate();

            Assert.Equal(2, errors.Count);
            Assert.Contains(errors, e => e.Parameter == SupportParameter.SupportLength);
            Assert.Contains(errors, e => e.Parameter == SupportParameter.GrooveLength);
        }
    }
}

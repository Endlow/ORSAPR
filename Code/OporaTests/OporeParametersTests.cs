using System.Collections.Generic;
using OporaApp;
using Xunit;

namespace OporaTests
{
    public class OporeParametersTests
    {
        [Fact]
        public void DefaultParameters_AreValid()
        {
            var parameters = OporeParameters.Default();
            Assert.Empty(parameters.Validate());
        }

        [Theory]
        [InlineData(60, 50, 100, 20, 20, 20)]
        [InlineData(150, 100, 300, 100, 80, 80)]
        public void BoundaryParameters_AreValid(double l, double w, double h, double d, double h1, double p)
        {
            var parameters = new OporeParameters { L = l, W = w, H = h, D = d, H1 = h1, P = p };
            Assert.Empty(parameters.Validate());
        }

        [Theory]
        [InlineData(59.9)]
        [InlineData(150.1)]
        public void LengthOutOfRange_IsReported(double l)
        {
            var parameters = new OporeParameters { L = l };
            Assert.Contains(parameters.Validate(), e => e.FieldName == "L");
        }

        [Theory]
        [InlineData(49.9)]
        [InlineData(100.1)]
        public void WidthOutOfRange_IsReported(double w)
        {
            var parameters = new OporeParameters { W = w };
            Assert.Contains(parameters.Validate(), e => e.FieldName == "W");
        }

        [Fact]
        public void GrooveLengthExceedsLimit_IsReportedWithHints()
        {
            var parameters = new OporeParameters { L = 80, D = 70 };
            var errors = parameters.Validate();
            var error = Assert.Single(errors.FindAll(e => e.FieldName == "D"));
            Assert.Contains("60", error.Message);
        }

        [Fact]
        public void GrooveLengthEqualToLimit_IsValid()
        {
            var parameters = new OporeParameters { L = 80, D = 60 };
            Assert.DoesNotContain(parameters.Validate(), e => e.FieldName == "D");
        }

        [Fact]
        public void GrooveHeightExceedsLimit_IsReported()
        {
            var parameters = new OporeParameters { H = 100, H1 = 61 };
            Assert.Contains(parameters.Validate(), e => e.FieldName == "H1");
        }

        [Fact]
        public void GrooveDepthExceedsLimit_IsReported()
        {
            var parameters = new OporeParameters { W = 50, P = 41 };
            Assert.Contains(parameters.Validate(), e => e.FieldName == "P");
        }

        [Fact]
        public void TechAssignmentExample_IsValid()
        {
            var parameters = new OporeParameters { L = 80, W = 50, H = 100, D = 60, H1 = 50, P = 40 };
            Assert.Empty(parameters.Validate());
        }

        [Fact]
        public void TechAssignmentExample_WithD70_IsInvalid()
        {
            var parameters = new OporeParameters { L = 80, W = 50, H = 100, D = 70, H1 = 50, P = 40 };
            Assert.NotEmpty(parameters.Validate());
        }

        [Fact]
        public void DefaultParameters_AreNotThroughSlot()
        {
            Assert.False(OporeParameters.Default().ThroughSlot);
        }

        [Fact]
        public void ThroughSlot_IgnoresDepthValidation()
        {
            var parameters = new OporeParameters { L = 80, W = 50, H = 100, D = 60, H1 = 50, P = 80, ThroughSlot = true };
            Assert.Empty(parameters.Validate());
        }

        [Theory]
        [InlineData(79.9)]
        [InlineData(80.1)]
        public void ThroughSlot_AnyDepthValueIsValid(double p)
        {
            var parameters = new OporeParameters { W = 60, P = p, ThroughSlot = true };
            Assert.DoesNotContain(parameters.Validate(), e => e.FieldName == "P");
        }

        [Fact]
        public void ThroughSlot_StillValidatesGrooveLength()
        {
            var parameters = new OporeParameters { L = 80, D = 70, ThroughSlot = true };
            Assert.Contains(parameters.Validate(), e => e.FieldName == "D");
        }

        [Fact]
        public void ThroughSlot_StillValidatesGrooveHeight()
        {
            var parameters = new OporeParameters { H = 100, H1 = 61, ThroughSlot = true };
            Assert.Contains(parameters.Validate(), e => e.FieldName == "H1");
        }
    }
}

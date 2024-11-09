using System.Globalization;
using OpenTelemetry.Trace;

namespace Lib.Service.Trace;

public class TraceSampler : Sampler
{
    private readonly long _idUpperBound;

        private static SamplingResult DropSamplingResult => new(SamplingDecision.Drop);

        public TraceSampler(double probability, bool enabled = false)
        {
            if (probability is < 0.0 or > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0.0 and 1.0");
            }

            IsEnabled = enabled;

            // The expected description is like TraceIdRatioBasedSampler{0.000100}
            Description = "TraceIdRatioBasedSampler{" + probability.ToString("F6", CultureInfo.InvariantCulture) + "}";

            _idUpperBound = probability switch
            {
                // Special case the limits, to avoid any possible issues with lack of precision across
                // double/long boundaries. For probability == 0.0, we use Long.MIN_VALUE as this guarantees
                // that we will never sample a trace, even in the case where the id == Long.MIN_VALUE, since
                // Math.Abs(Long.MIN_VALUE) == Long.MIN_VALUE.
                0.0 => long.MinValue,
                1.0 => long.MaxValue,
                _ => (long)(probability * long.MaxValue)
            };
        }

        public bool IsEnabled { get; private set; }

        public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
        {
            if (IsEnabled)
            {
                // Always sample if we are within probability range. This is true even for child activities (that
                // may have had a different sampling decision made) to allow for different sampling policies,
                // and dynamic increases to sampling probabilities for debugging purposes.
                // Note use of '<' for comparison. This ensures that we never sample for probability == 0.0,
                // while allowing for a (very) small chance of *not* sampling if the id == Long.MAX_VALUE.
                // This is considered a reasonable trade-off for the simplicity/performance requirements (this
                // code is executed in-line for every Activity creation).
                Span<byte> traceIdBytes = stackalloc byte[16];
                samplingParameters.TraceId.CopyTo(traceIdBytes);

                return new(Math.Abs(GetLowerLong(traceIdBytes)) < _idUpperBound ? SamplingDecision.RecordAndSample : SamplingDecision.Drop);
            }
            else
            {
                return DropSamplingResult;
            }
        }

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }

        private static long GetLowerLong(ReadOnlySpan<byte> bytes)
        {
            long result = 0;
            for (var i = 0; i < 8; i++)
            {
                result <<= 8;
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                result |= bytes[i] & 0xff;
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
            }

            return result;
        }
}
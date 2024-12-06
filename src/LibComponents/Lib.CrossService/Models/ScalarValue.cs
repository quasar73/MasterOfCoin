namespace Lib.CrossService.Models
{
    public class ScalarValue<T> where T : struct
    {
        public ScalarValue()
        {
        }

        public ScalarValue(T value)
        {
            Value = value;
        }

        public T Value { get; set; }

        public static ScalarValue<T> From(T value)
            => new ScalarValue<T>(value);

        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is ScalarValue<T> scalar)
            {
                return Value.Equals(scalar.Value);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string? ToString()
        {
            return Value.ToString();
        }

        #endregion
    }
}

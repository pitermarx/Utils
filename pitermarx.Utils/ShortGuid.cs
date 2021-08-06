using System;

namespace pitermarx.Utils
{
    /// <summary> Like Guid, but shorter and url safe </summary>
    public struct ShortGuid
    {
        /// <summary> A read-only instance of the ShortGuid class whose value is guaranteed to be all zeroes. </summary>
        public static readonly ShortGuid Empty = new(Guid.Empty);

        public readonly Guid Guid;
        public readonly string Value;

        /// <summary> Creates a ShortGuid from a Guid </summary>
        public ShortGuid(Guid guid)
        {
            Value = Encode(guid);
            Guid = guid;
        }

        /// <summary> Encodes the given Guid as a base64 string that is 22 characters long. </summary>
        private static string Encode(Guid guid)
        {
            string encoded = Convert.ToBase64String(guid.ToByteArray());
            return encoded.Replace("/", "_").Replace("+", "-").Substring(0, 22);
        }

        /// <summary> Decodes the given base64 string </summary>
        private static Guid Decode(string value)
        {
            value = value.Replace("_", "/").Replace("-", "+") + "==";
            return new Guid(Convert.FromBase64String(value));
        }

        /// <summary> Determines if both ShortGuids have the same underlying Guid value. </summary>
        public static bool operator ==(ShortGuid x, ShortGuid y) => x.Equals(y);

        /// <summary> Determines if both ShortGuids do not have the same underlying Guid value. </summary>
        public static bool operator !=(ShortGuid x, ShortGuid y) => !x.Equals(y);

        /// <summary> Implicitly converts the ShortGuid to it's Guid equivilent </summary>
        public static implicit operator Guid(ShortGuid shortGuid) => shortGuid.Guid;

        /// <summary> Implicitly converts the Guid to a ShortGuid </summary>
        public static implicit operator ShortGuid(Guid guid) => new(guid);

        /// <summary> Initialises a new instance of the ShortGuid class </summary>
        public static ShortGuid NewGuid() => new(Guid.NewGuid());

        /// <summary> Initialises a new instance of the ShortGuid class </summary>
        public static ShortGuid Parse(string str) => new(Decode(str));

        /// <summary> Returns the base64 encoded guid as a string </summary>
        public override string ToString() => Value;

        /// <summary>
        /// Returns a value indicating whether this instance and a
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj) => obj switch
        {
            ShortGuid guid => Guid.Equals(guid.Guid),
            Guid _ => Guid.Equals(obj),
            string _ => Value.Equals(obj),
            _ => false,
        };

        /// <summary> Returns the HashCode for underlying Guid. </summary>
        public override int GetHashCode() => Guid.GetHashCode();
    }
}
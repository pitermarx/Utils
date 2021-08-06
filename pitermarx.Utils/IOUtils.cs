using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace pitermarx.Utils
{
    /// <summary>
    /// Helper methods for input and output operations.
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        ///  Deserializes a XML representation to an object through System.Xml.Serialization.XmlSerializer using UTF8 encoding.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="xmlString">The XML string.</param>
        /// <returns>An instance of the type T.</returns>
        /// <see cref="System.Xml.Serialization.XmlSerializer"/>
        public static T FromXml<T>(this string xmlString)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(ms);
        }

        /// <summary>
        /// Serializes an object to XML through System.Xml.Serialization.XmlSerializer.
        /// The class of the object must be marked [Serializable].
        /// </summary>
        /// <param name="o">The object to serialize.</param>
        /// <returns>The object serialization.</returns>
        /// <see cref="System.Xml.Serialization.XmlSerializer"/>
        public static string ToXml(this object o)
        {
            using var ms = new MemoryStream();
            var serializer = new System.Xml.Serialization.XmlSerializer(o.GetType());
            serializer.Serialize(ms, o);
            ms.Position = 0;

            using var reader = new StreamReader(ms);
            return reader.ReadToEnd();
        }

        public static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(fileName.Where(c => !invalid.Contains(c)).ToArray());
        }

        /// <summary>
        /// AppDomain's BaseDirectory is inconsistent. Lets fix that
        /// http://stackoverflow.com/questions/20405965/whats-the-convention-for-trailing-slash-in-appdomain-currentdomain-basedirectory
        /// </summary>
        public static string GetCurrentDirectory()
        {
            return AddTrailingSlash(AppDomain.CurrentDomain.BaseDirectory);
        }

        public static string AddTrailingSlash(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public static IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public static string AppendTimeStamp(this string text, DateTime? time = null, bool useMilliseconds = false)
        {
            time ??= DateTime.UtcNow;
            // filename date time format as ISO_8601 international standard: [yyyyMMddTHHmmss]
            return string.Format("{0}-{1}{2}",
                text,
                time.Value.ToString("yyyyMMddTHHmmss"),
                useMilliseconds ? time.Value.Millisecond.ToString(CultureInfo.InvariantCulture) : string.Empty);
        }
    }
}
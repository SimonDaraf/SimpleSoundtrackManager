using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedSoundtrackSystem.MVVM.Model.Utils
{
    /// <summary>
    /// <c>Class</c> Used to serialize and deserialize data.
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// <c>Method</c> Serializes and writes data to specified file path.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="toSerialize">The object to serialize</param>
        /// <param name="filePath">The file path to save the file to.</param>
        public static void ToBinary<T>(T toSerialize, string filePath)
        {
            using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            MessagePackSerializer.Serialize(fileStream, toSerialize); // Serialize and write.
            fileStream.Dispose();
        }

        /// <summary>
        /// <c>Method</c> Deserializes the specified file to specified type.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="filePath">The path to the file to deserialize</param>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(string filePath)
        {
            using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            T deserialized = MessagePackSerializer.Deserialize<T>(fileStream); // Deserializes file.
            fileStream.Dispose();
            return deserialized;
        }
    }
}

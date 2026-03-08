using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LAIR.ResourceAPIs.WordNet
{
    /// <summary>
    /// Performs binary search over a sorted text file. Internal replacement for
    /// LAIR.IO.BinarySearchTextStream — the only LAIR.IO type used by WordNetEngine.
    ///
    /// Design notes:
    ///   * The binary search operates directly on the FileStream at the byte level
    ///     so that stream position is always exact (no StreamReader buffering issues).
    ///   * A StreamReader is held separately and exposed via the Stream property for
    ///     the linear-read path in WordNetEngine.AllWords. Callers reset it themselves
    ///     (DiscardBufferedData + BaseStream.Position = 0) before reading.
    /// </summary>
    internal class BinarySearchTextStream
    {
        /// <summary>
        /// Compares a search key to the current line.
        /// Return -1 if key comes before current line, 1 if after, 0 if match.
        /// </summary>
        internal delegate int SearchComparisonDelegate(object key, string currentLine);

        private readonly FileStream _fileStream;
        private readonly StreamReader _reader;
        private readonly SearchComparisonDelegate _comparison;

        /// <summary>
        /// Gets the underlying reader for linear access (e.g. AllWords).
        /// Callers must call DiscardBufferedData() and reset BaseStream.Position
        /// before using this after a Search call.
        /// </summary>
        internal StreamReader Stream
        {
            get { return _reader; }
        }

        internal BinarySearchTextStream(string path, SearchComparisonDelegate searchComparison)
        {
            _fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = new StreamReader(_fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096);
            _comparison = searchComparison;
        }

        /// <summary>
        /// Searches the entire file for a line matching <paramref name="key"/>.
        /// </summary>
        internal string Search(object key)
        {
            long start = 0;
            long end = _fileStream.Length;

            while (start < end)
            {
                long mid = start + (end - start) / 2;
                _fileStream.Position = mid;

                // When mid > start we may be mid-line; skip to the next complete line.
                // When mid == start, start is always at a line boundary (either 0 or set
                // from _fileStream.Position after a previous ReadLine) so we read directly.
                if (mid > start)
                    SkipToNextLine();

                long lineStart = _fileStream.Position;
                if (lineStart >= end)
                {
                    end = mid;
                    continue;
                }

                string line = ReadLineFromStream();
                if (line == null)
                {
                    end = mid;
                    continue;
                }

                int cmp = _comparison(key, line);
                if (cmp == 0)
                    return line;

                if (cmp < 0)
                    end = mid;
                else
                    start = _fileStream.Position;
            }

            return null;
        }

        internal void Close()
        {
            _reader.Close();
        }

        private void SkipToNextLine()
        {
            int b;
            while ((b = _fileStream.ReadByte()) != -1 && b != '\n')
            {
            }
        }

        private string ReadLineFromStream()
        {
            var buf = new List<byte>(128);
            int b;
            while ((b = _fileStream.ReadByte()) != -1)
            {
                if (b == '\n')
                    break;
                buf.Add((byte)b);
            }

            if (buf.Count == 0 && b == -1)
                return null;

            // Strip trailing \r for files with \r\n line endings
            if (buf.Count > 0 && buf[buf.Count - 1] == (byte)'\r')
                buf.RemoveAt(buf.Count - 1);

            return Encoding.UTF8.GetString(buf.ToArray());
        }
    }
}

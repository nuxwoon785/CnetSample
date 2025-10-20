using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CNetTest
{
    public partial class Form1 : Form
    {
        private SerialPort _serial;
        private readonly List<byte> _rxBuffer = new List<byte>();
        private readonly object _rxLock = new object();

        // New fields to support synchronous ReadBuffer
        private TaskCompletionSource<byte[]> _pendingResponse;
        private readonly object _responseLock = new object();

        public Form1()
        {
            InitializeComponent();

            _serial = new SerialPort("COM6", 9600, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 500,
                WriteTimeout = 500,
                Encoding = Encoding.ASCII
            };

            // Subscribe to DataReceived to handle incoming data
            _serial.DataReceived += Serial_DataReceived;
        }
        private void ProcessRxBuffer()
        {
            // This runs under _rxLock
            while (true)
            {
                // Find start (ENQ)
                int start = _rxBuffer.IndexOf(0x05);
                if (start < 0)
                {
                    // no start marker, discard data before and wait
                    _rxBuffer.Clear();
                    return;
                }

                // If start is not at 0, remove preceding bytes
                if (start > 0)
                {
                    _rxBuffer.RemoveRange(0, start);
                }

                // Need at least ENQ + something + EOT + BCC => minimal length 3
                if (_rxBuffer.Count < 3)
                    return;

                // Find EOT after start
                int eotIndex = _rxBuffer.IndexOf(0x04, 1); // search after ENQ
                if (eotIndex < 0)
                {
                    // EOT not yet received
                    return;
                }

                // Ensure BCC exists after EOT
                int bccIndex = eotIndex + 1;
                if (bccIndex >= _rxBuffer.Count)
                {
                    // wait for BCC byte
                    return;
                }

                // We have a candidate frame from 0..bccIndex
                int frameLen = bccIndex + 1;
                byte[] frame = _rxBuffer.GetRange(0, frameLen).ToArray();

                // Validate BCC (XOR of all previous bytes)
                byte calc = 0x00;
                for (int i = 0; i < frameLen - 1; i++)
                    calc ^= frame[i];

                byte bcc = frame[frameLen - 1];
                if (calc == bcc)
                {
                    // valid frame - process it
                    byte[] completeFrame = frame; // capture

                    // If a caller is waiting via ReadBuffer, signal it. Otherwise show MessageBox on UI thread.
                    TaskCompletionSource<byte[]> tcs = null;
                    lock (_responseLock)
                    {
                        tcs = _pendingResponse;
                    }

                    if (tcs != null)
                    {
                        // Try to set result; ignore failure if already cancelled or timed out
                        tcs.TrySetResult(completeFrame);
                    }
                    else
                    {
                        // marshal to UI thread for user notification
                        string ascii = _serial.Encoding.GetString(completeFrame, 0, frameLen);
                        string hex = BitConverter.ToString(completeFrame).Replace('-', ' ');

                        this.BeginInvoke((Action)(() =>
                        {
                            MessageBox.Show($"Complete frame received ({frameLen} bytes)\nHEX: {hex}\nASCII: {ascii}", "Frame", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    }

                    // remove processed frame from buffer and continue loop
                    _rxBuffer.RemoveRange(0, frameLen);
                }
                else
                {
                    // BCC mismatch - discard the start byte and try to resync
                    _rxBuffer.RemoveAt(0);
                }
            }
        }
        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var sp = (SerialPort)sender;

                int toRead = sp.BytesToRead;
                if (toRead <= 0)
                    return;

                byte[] buffer = new byte[toRead];
                int read = sp.Read(buffer, 0, toRead);

                lock (_rxLock)
                {
                    // Append received bytes to buffer
                    _rxBuffer.AddRange(buffer.Take(read));

                    // Try to extract complete frames
                    ProcessRxBuffer();
                }
            }
            catch
            {
                // ignore read errors
            }
        }

        /// <summary>
        /// Send a request frame and wait synchronously for a valid response frame (ENQ...EOT+BCC).
        /// Returns the complete frame bytes if received within timeoutMs, otherwise null.
        /// </summary>
        /// <param name="request">Bytes to send.</param>
        /// <param name="timeoutMs">Timeout in milliseconds.</param>
        /// <returns>Complete response frame bytes or null if timed out.</returns>
        public byte[] ReadBuffer(byte[] request, int timeoutMs = 2000)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (_serial == null || !_serial.IsOpen)
                throw new InvalidOperationException("Serial port is not open.");

            lock (_responseLock)
            {
                if (_pendingResponse != null)
                    throw new InvalidOperationException("A read is already in progress.");

                _pendingResponse = new TaskCompletionSource<byte[]>();
            }

            try
            {
                // Send request
                _serial.Write(request, 0, request.Length);

                // Wait for response or timeout
                var task = _pendingResponse.Task;
                bool signaled = task.Wait(timeoutMs);
                if (signaled)
                {
                    return task.Result;
                }
                else
                {
                    // Timeout - try to cancel
                    lock (_responseLock)
                    {
                        _pendingResponse.TrySetCanceled();
                    }
                    return null;
                }
            }
            finally
            {
                // Clear pending reference
                lock (_responseLock)
                {
                    _pendingResponse = null;
                }
            }
        }

        private static string NormalizeStation(string station)
        {
            if (string.IsNullOrWhiteSpace(station))
                throw new ArgumentException("Station cannot be null or empty.", nameof(station));

            station = station.Trim();
            if (station.Length < 2)
                station = station.PadLeft(2, '0');

            return station;
        }

        private static string FormatDecimal(int value, int minimumDigits)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative.");

            string text = value.ToString(CultureInfo.InvariantCulture);
            if (text.Length < minimumDigits)
            {
                text = text.PadLeft(minimumDigits, '0');
            }

            return text;
        }

        private static byte[] BuildFrameFromAscii(string asciiPayload)
        {
            if (asciiPayload == null) throw new ArgumentNullException(nameof(asciiPayload));

            var bytes = new List<byte>(asciiPayload.Length + 3)
            {
                0x05
            };

            bytes.AddRange(Encoding.ASCII.GetBytes(asciiPayload));
            bytes.Add(0x04);

            byte bcc = 0x00;
            foreach (var b in bytes)
            {
                bcc ^= b;
            }

            bytes.Add(bcc);
            return bytes.ToArray();
        }

        private byte[] SendCommand(string asciiPayload, int timeoutMs)
        {
            var frame = BuildFrameFromAscii(asciiPayload);
            var response = ReadBuffer(frame, timeoutMs);
            if (response == null)
                throw new TimeoutException("Timed out waiting for PLC response.");

            return response;
        }

        private static string ExtractAsciiPayload(byte[] frame)
        {
            if (frame == null)
                return null;

            if (frame.Length < 3 || frame[0] != 0x05)
                throw new FormatException("Invalid frame format.");

            int eotIndex = Array.IndexOf(frame, (byte)0x04, 1);
            if (eotIndex < 0)
                throw new FormatException("EOT not found in frame.");

            if (eotIndex <= 1)
                return string.Empty;

            return Encoding.ASCII.GetString(frame, 1, eotIndex - 1);
        }

        private string SendCommandForAscii(string asciiPayload, int timeoutMs)
        {
            var response = SendCommand(asciiPayload, timeoutMs);
            return ExtractAsciiPayload(response);
        }

        public string ReadDevice(IEnumerable<string> deviceAddresses, string station = "00", int timeoutMs = 2000)
        {
            if (deviceAddresses == null) throw new ArgumentNullException(nameof(deviceAddresses));

            var addressList = deviceAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).ToList();
            if (addressList.Count == 0)
                throw new ArgumentException("At least one device address must be provided.", nameof(deviceAddresses));

            var builder = new StringBuilder();
            builder.Append(NormalizeStation(station));
            builder.Append("RSS");
            builder.Append(FormatDecimal(addressList.Count, 2));

            foreach (var address in addressList)
            {
                builder.Append(FormatDecimal(address.Length, 2));
                builder.Append(address);
            }

            return SendCommandForAscii(builder.ToString(), timeoutMs);
        }

        public string WriteDevice(IEnumerable<KeyValuePair<string, string>> deviceValues, string station = "00", int timeoutMs = 2000)
        {
            if (deviceValues == null) throw new ArgumentNullException(nameof(deviceValues));

            var filtered = deviceValues.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key)).ToList();
            if (filtered.Count == 0)
                throw new ArgumentException("At least one device/value pair must be provided.", nameof(deviceValues));

            var builder = new StringBuilder();
            builder.Append(NormalizeStation(station));
            builder.Append("WSS");
            builder.Append(FormatDecimal(filtered.Count, 2));

            foreach (var kvp in filtered)
            {
                string address = kvp.Key.Trim();
                string value = (kvp.Value ?? string.Empty).Trim();

                builder.Append(FormatDecimal(address.Length, 2));
                builder.Append(address);
                builder.Append(FormatDecimal(value.Length, 2));
                builder.Append(value);
            }

            return SendCommandForAscii(builder.ToString(), timeoutMs);
        }

        public string WriteDevice(IDictionary<string, ushort> deviceValues, string station = "00", int timeoutMs = 2000, bool uppercaseHex = true)
        {
            if (deviceValues == null) throw new ArgumentNullException(nameof(deviceValues));

            var converted = new List<KeyValuePair<string, string>>();
            foreach (var kvp in deviceValues)
            {
                string value = uppercaseHex
                    ? kvp.Value.ToString("X4", CultureInfo.InvariantCulture)
                    : kvp.Value.ToString("D", CultureInfo.InvariantCulture);
                converted.Add(new KeyValuePair<string, string>(kvp.Key, value));
            }

            return WriteDevice(converted, station, timeoutMs);
        }

        public string ReadBlock(string startAddress, int wordCount, string station = "00", int timeoutMs = 2000)
        {
            if (string.IsNullOrWhiteSpace(startAddress))
                throw new ArgumentException("Start address cannot be null or empty.", nameof(startAddress));

            if (wordCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(wordCount), "Word count must be positive.");

            startAddress = startAddress.Trim();

            var builder = new StringBuilder();
            builder.Append(NormalizeStation(station));
            builder.Append("RSB");
            builder.Append(FormatDecimal(startAddress.Length, 2));
            builder.Append(startAddress);
            builder.Append(FormatDecimal(wordCount, 2));

            return SendCommandForAscii(builder.ToString(), timeoutMs);
        }

        public string WriteBlock(string startAddress, IEnumerable<ushort> values, string station = "00", int timeoutMs = 2000)
        {
            if (string.IsNullOrWhiteSpace(startAddress))
                throw new ArgumentException("Start address cannot be null or empty.", nameof(startAddress));

            if (values == null) throw new ArgumentNullException(nameof(values));

            var wordList = values.ToList();
            if (wordList.Count == 0)
                throw new ArgumentException("At least one value must be provided.", nameof(values));

            startAddress = startAddress.Trim();

            var dataBuilder = new StringBuilder();
            foreach (var word in wordList)
            {
                dataBuilder.Append(word.ToString("X4", CultureInfo.InvariantCulture));
            }

            string data = dataBuilder.ToString();

            var builder = new StringBuilder();
            builder.Append(NormalizeStation(station));
            builder.Append("WSB");
            builder.Append(FormatDecimal(startAddress.Length, 2));
            builder.Append(startAddress);
            builder.Append(FormatDecimal(wordList.Count, 2));
            builder.Append(FormatDecimal(data.Length, 4));
            builder.Append(data);

            return SendCommandForAscii(builder.ToString(), timeoutMs);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!_serial.IsOpen)
                {
                    _serial.Open();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Serial send failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_serial != null)
                {
                    // Unsubscribe and close
                    _serial.DataReceived -= Serial_DataReceived;
                    if (_serial.IsOpen)
                        _serial.Close();
                    _serial.Dispose();
                    _serial = null;
                }
            }
            catch
            {
                // ignore
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string response = ReadBlock("%MW100", 10);
                MessageBox.Show($"ReadBlock response: {response}", "PLC", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Serial send failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

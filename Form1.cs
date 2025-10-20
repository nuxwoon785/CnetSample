using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private byte[] BuildSampleFrame()
        {
            var bytes = new List<byte>();
            bytes.Add(0x05);
            //00(국번), R(읽기)  SS(명령어타입)  02(몇개) 변수1길이,변수1명 변수2길이,변수2명
            string payload = "00RSS0206%MW10006%MW101";
            string payload1 = "00RSB06%MW10010";
            bytes.AddRange(Encoding.ASCII.GetBytes(payload1));
            bytes.Add(0x04);

            // compute BCC
            byte bcc = 0x00;
            foreach (var b in bytes)
                bcc ^= b;
            bytes.Add(bcc);

            return bytes.ToArray();
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
                var frame = BuildSampleFrame();

                // Send frame bytes
                _serial.Write(frame, 0, frame.Length);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Serial send failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

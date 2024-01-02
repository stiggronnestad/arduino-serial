using System.IO.Ports;
using System.Text;

namespace Usignert.ArduinoSerial
{
    /// <summary>
    /// Arduino serial port.
    /// </summary>
    public class ArduinoSerialPort
    {
        /// <summary>
        /// Gets or sets the port name.
        /// </summary>
        public string PortName
        {
            get
            {
                return _serialPort.PortName;
            }
            set
            {
                _serialPort.PortName = value;
            }
        }

        /// <summary>
        /// Gets the available port names.
        /// </summary>
        public static string[] PortNames => SerialPort.GetPortNames();

        /// <summary>
        /// Gets or sets the baud rate.
        /// </summary>
        public int BaudRate
        {
            get
            {
                return _serialPort.BaudRate;
            }
            set
            {
                _serialPort.BaudRate = value;
            }
        }

        /// <summary>
        /// Is the port open?
        /// </summary>
        public bool IsOpen => _serialPort.IsOpen;

        /// <summary>
        /// Gets or sets the end of submission character.
        /// </summary>
        public char EndOfSubmissionChar { get; set; } = '\n';

        /// <summary>
        /// Event for a complete submission, ending with the EndOfSubmissionChar.
        /// </summary>
        public event Action<string>? OnCompleteSubmission;

        private bool _ready = false;
        private readonly SerialPort _serialPort;
        private readonly StringBuilder _stringBuilder = new();

        private ArduinoSerialPort(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits,
                DtrEnable = true,
                RtsEnable = true
            };

            OnCompleteSubmission = null;
        }

        ~ArduinoSerialPort()
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }

        public static ArduinoSerialPort Create(ArduinoBoard board)
        {
            var portName = GetHighestComPort();
            return Create(portName, board.BaudRate);
        }
        public static ArduinoSerialPort Create(ArduinoBoard board, string portName)
        {
            return Create(portName, board.BaudRate);
        }
        public static ArduinoSerialPort Create(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            var p = new ArduinoSerialPort(portName, baudRate, parity, dataBits, stopBits);
            return p;
        }

        /// <summary>
        /// Gets the highest COM port number.
        /// </summary>
        /// <returns></returns>
        public static string GetHighestComPort()
        {
            var names = SerialPort.GetPortNames();
            var highest = 0;

            foreach (var name in names)
            {
                var number = int.Parse(name.Replace("COM", ""));
                if (number > highest)
                {
                    highest = number;
                }
            }

            return $"COM{highest}";
        }

        /// <summary>
        /// Starts the serial port.
        /// </summary>
        /// <param name="portName"></param>
        public void Start(string portName = "")
        {
            if (!string.IsNullOrWhiteSpace(portName))
            {
                PortName = portName;
            }

            try
            {
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Port {0} is in use {PortName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial exception: {ex}");
            }

        }

        /// <summary>
        /// Stops the serial port.
        /// </summary>
        public void Stop()
        {
            _serialPort.Close();
            _serialPort.Dispose();
            _ready = false;
        }

        /// <summary>
        /// Sends a command to the Arduino.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(string command)
        {
            if (!_serialPort.IsOpen || !_ready) return;
            _serialPort.WriteLine(command);
            // HACK: To avoid bogus data this is needed, maybe there is a b etter way? Byte queue?
            Thread.Sleep(25);
        }

        /// <summary>
        /// Sets the data from an ArduinoBoard.
        /// </summary>
        /// <param name="board"></param>
        public void SetData(ArduinoBoard board)
        {
            BaudRate = board.BaudRate;
        }

        /// <summary>
        /// Data received internal event wrapper.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!_serialPort.IsOpen) return;

            var msg = _serialPort.ReadExisting();

            foreach (char c in msg)
            {
                if (c == EndOfSubmissionChar)
                {
                    var cmd = _stringBuilder.ToString();
                    _stringBuilder.Clear();

                    if (!string.IsNullOrWhiteSpace(cmd))
                    {
                        SerialPort_CompleteSubmission(cmd.TrimEnd());
                    }
                }
                else
                {
                    _stringBuilder.Append(c);
                }
            }
        }

        /// <summary>
        /// Complete submission internal event wrapper.
        /// </summary>
        /// <param name="cmd"></param>
        private void SerialPort_CompleteSubmission(string cmd)
        {
            if (cmd.StartsWith("READY", StringComparison.InvariantCultureIgnoreCase))
            {
                _ready = true;
            }
            else
            {
                OnCompleteSubmission?.Invoke(cmd);
            }
        }
    }
}

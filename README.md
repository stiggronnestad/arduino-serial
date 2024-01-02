# arduino-serial
A simple wrapper around Serial Port used for, but not limited to, communication with Arduino boards.

- Simple setup for sending commands/information between devices.
- Event handler for complete submission of data.
- Customizable `EndOfSubmissionChar`.
- Helper for getting the "last" COM port available, `GetHighestComPort()`.

## Code Example

#### NOTE!
> The arduino/device must send `READY` after the connection has been established. This is done to avoid the garbage/noise that is transmitted during boot/reboot when flashing new firmware.

### Arduino Sketch
```c
void setup()
{
    Serial.begin(115200);
    Serial.print("READY#");
    // or, depending on `EndOfSubmissionChar`
    Serial.println("READY");
}

void loop()
{
    Serial.print("COMMAND|DATA,DATA,DATA#"); // arbitrary usage sample
    delay(1000);
}
```

### Usings
```C#
using System.IO.Ports;
using Usignert.ArduinoSerial;
```

### Basic Constructor and Event Handling
```C#
var port = ArduinoSerialPort.Create(ArduinoBoard.Uno);
port.OnCompleteSubmission += Port_CompleteSubmission;
port.Start("COM3");

void Port_CompleteSubmission(string data)
{
    // Data until the EndOfSubmissionChar, newline character by default.
}
```

### Specific Setup
```C#
var port = ArduinoSerialPort.Create("COM3", 115200, Parity.None, 8, StopBits.One);
port.EndOfSubmissionChar = '#';
port.OnCompleteSubmission += Port_CompleteSubmission;
port2.Start();

void Port_CompleteSubmission(string data)
{
    // Data until the EndOfSubmissionChar, '#' in this case.
}
```

### Sending Data

`SendCommand` basically wraps `Write` on the Serial Port and includes the `EndOfSubmissionChar`.
```C#
port.SendCommand("Hello World!");
```

### Getting COM Ports

```C#
var availablePorts = ArduinoSerialPort.PortNames; // [COM1, COM2, COM3]
var lastPort = ArduinoSerialPort.GetHighestComPort(); // COM3
```
namespace Usignert.ArduinoSerial
{
    /// <summary>
    /// Represents an Arduino board.
    /// </summary>
    public struct ArduinoBoard
    {
        public string Name;
        public int BaudRate;

        public static ArduinoBoard Uno => new() { Name = "Uno", BaudRate = 115200 };
        public static ArduinoBoard Mega => new() { Name = "Mega", BaudRate = 115200 };
        public static ArduinoBoard Due => new() { Name = "Due", BaudRate = 115200 };

        /// <summary>
        /// Gets the ArduinoBoard from the ArduinoBoards enum.
        /// </summary>
        /// <param name="board"></param>
        /// <returns>ArduinoBoard</returns>
        public static ArduinoBoard GetBoard(ArduinoBoards board)
        {
            return board switch
            {
                ArduinoBoards.Uno => Uno,
                ArduinoBoards.Mega => Mega,
                ArduinoBoards.Due => Due,
                _ => Uno,
            };
        }
    }
}

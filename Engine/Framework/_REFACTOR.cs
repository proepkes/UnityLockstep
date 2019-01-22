/*
 * THIS CLASS CONTAINS IDEAS FOR FUTURE REFACTORINGS
 */


/*
 * GENERIC COMMAND-PARSER-SERVICE, BECAUSE THE FRAMEWORK SHOULD NOT DETERMINE ANY COMMANDTAGS/COMMANDS, REPLACES DefaultInputService 
 */


//public interface IInputDecoder<T>
//{
//    T DecodeInput(SerializedInput input);
//}

//public class NetDataInputDecoder : IInputDecoder<ushort>
//{
//    private readonly NetDataReader _dataReader;

//    public NetDataInputDecoder()
//    {
//        _dataReader = new NetDataReader();
//    }

//    public ushort DecodeInput(SerializedInput input)
//    {
//        return _dataReader.GetUShort();
//    }
//}

//public class GenericParseInputService<T> : IInputService
//{
//    private readonly IInputDecoder<T> inputDecoder;
//    public IDictionary<T, ICommand> InputMap { get; }

//    public GenericParseInputService(IInputDecoder<T> inputDecoder, IDictionary<T, ICommand> inputMap)
//    {
//        InputMap = inputMap;
//        this.inputDecoder = inputDecoder;
//        CommandMap.Add(CommandTag.Spawn, new SpawnCommand());
//        CommandMap.Add(CommandTag.Navigate, new NavigateCommand());
//    }

//    public void Execute(InputContext context, SerializedInput serializedInput)
//    {
//        var key = inputDecoder.DecodeInput(serializedInput);
//        InputMap[key].Deserialize();
//        _dataReader.SetSource(serializedInput.Data);

//        var commandTag = (CommandTag)_dataReader.GetByte();

//        var cmd = CommandMap[commandTag];
//        cmd.Deserialize(_dataReader);
//        cmd.Execute(context);
//    }
//}

/*
 ******** BETTER WAY FOR SERVICE INJECTION TO MAKE SURE ALL SERVICES ARE AVAILABLE // 
 */
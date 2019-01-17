/*
 * THIS CLASS CONTAINS IDEAS FOR FUTURE REFACTORINGS
 */


/*
 * GENERIC COMMAND-PARSER-SERVICE, BERCAUSE THE FRAMEWORK SHOULD NOT DETERMINE ANY COMMANDTAGS/COMMANDS, REPLACES DefaultCommandService 
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

//public class GenericParseInputService<T> : IParseInputService
//{
//    private readonly IInputDecoder<T> inputDecoder;
//    public IDictionary<T, ISerilalizableCommand> InputMap { get; }

//    public GenericParseInputService(IInputDecoder<T> inputDecoder, IDictionary<T, ISerilalizableCommand> inputMap)
//    {
//        InputMap = inputMap;
//        this.inputDecoder = inputDecoder;
//        CommandMap.Add(CommandTag.Spawn, new SpawnCommand());
//        CommandMap.Add(CommandTag.Navigate, new NavigateCommand());
//    }

//    public void Parse(InputContext context, SerializedInput serializedInput)
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
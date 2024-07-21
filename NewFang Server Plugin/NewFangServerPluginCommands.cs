//using Torch.Commands;
//using Torch.Commands.Permissions;
//using VRage.Game.ModAPI;

//namespace NewFang_Server_Plugin {
//    [Category("NewFang_Server_Plugin")]
//    public class NewFang_Server_PluginCommands : CommandModule {

//        public NewFang_Server_Plugin Plugin => (NewFang_Server_Plugin)Context.Plugin;

//        [Command("test", "This is a Test Command.")]
//        [Permission(MyPromoteLevel.Moderator)]
//        public void Test() {
//            Context.Respond("This is a Test from " + Context.Player);
//        }

//        [Command("testWithCommands", "This is a Test Command.")]
//        [Permission(MyPromoteLevel.None)]
//        public void TestWithArgs(string foo, string bar = null) {
//            Context.Respond("This is a Test " + foo + ", " + bar);
//        }
//    }
//}

namespace NewFangServerPlugin.WebAPI.Models {
    public struct MessageRespone {
        public string Message { get; set; }
        public MessageRespone(string message) {
            Message = message;
        }
    }
}

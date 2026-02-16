using ActressMas;
using System;

namespace RemoteMessages
{
    public class PingAgent : Agent
    {
        public override void Setup()
        {
            Global.Rtb.AppendText($"[{Name}] Sending request\r\n");
            Send("pong-agent@Container2", "request-info");
        }

        public override void Act(Message message)
        {
            try
            {
                Global.Rtb.AppendText($"\t{message.Format()}\r\n");
            }
            catch (Exception ex)
            {
                Global.Rtb.AppendText($"{ex.Message}\r\n");
            }
        }
    }
}
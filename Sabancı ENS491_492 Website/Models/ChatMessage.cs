namespace Sabancı_ENS491_492_Website.Models
{

    public class ChatMessage
    {
        public int Id { get; set; }
        public int SenderId { get; set; }   
        public int ReceiverId { get; set; }  
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation properties if needed
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }


}

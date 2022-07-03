namespace NotesApp.Domain.Entities
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTimeOffset Expires { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}

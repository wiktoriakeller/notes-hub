﻿namespace NotesApp.Services.Dto
{
    public class NoteDto
    {
        public string NoteName { get; set; }
        public string Content { get; set; }
        public ICollection<TagDto> Tags { get; set; }
    }
}
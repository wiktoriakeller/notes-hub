﻿using NotesApp.Services.Interfaces;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;
using NotesApp.Services.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using NotesApp.Services.Authorization;
using NotesApp.Services.Exceptions;
using System.Linq.Expressions;
using HashidsNet;

namespace NotesApp.Services.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _notesRepository;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;
        private readonly IHashids _hashids;

        public NoteService(INoteRepository notesRepository, 
            IMapper mapper,
            IAuthorizationService authorizationService,
            IUserContextService userContextService,
            IHashids hashids)
        {
            _notesRepository = notesRepository;
            _mapper = mapper;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
            _hashids = hashids;
        }

        public async Task<NoteDto?> GetNoteById(string hashId)
        {
            var id = GetRawId(hashId);
            var note = await _notesRepository.GetByIdAsync(id, "Tags");
            await CheckAuthorization(note);

            if (note == null)
                throw new NotFoundException("Resource couldn't be found");

            return _mapper.Map<NoteDto>(note);
        }

        public Task<PagedResult<NoteDto>> GetNotes(NoteQuery query)
        {
            query.SearchType = query.SearchType?.ToLower().Trim();
            query.SearchPhrase = query.SearchPhrase?.ToLower().Trim();

            if(!string.IsNullOrEmpty(query.SearchType) && !string.IsNullOrEmpty(query.SearchPhrase))
            {
                var userId = GetUserId();
                return query.SearchType switch
                {
                    "all" => GetPagedNotes(query),
                    "name" => GetPagedNotes(query, n => n.NoteName.ToLower().Contains(query.SearchPhrase) && n.UserId == userId),
                    "content" => GetPagedNotes(query, n => n.Content.ToLower().Contains(query.SearchPhrase) && n.UserId == userId),
                    "tags" => GetPagedNotes(query, n => n.Tags.Select(t => t.TagName.ToLower()).Any(t => t.Contains(query.SearchPhrase)) && n.UserId == userId),
                    _ => GetPagedNotes(query)
                };
            }

            return GetPagedNotes(query);
        }

        public async Task<IEnumerable<NoteDto>> GetAllNotes()
        {
            var userId = GetUserId();
            var notes = await _notesRepository.GetAllAsync(n => n.UserId == userId, "Tags");
            await CheckAuthorization(notes);
            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<PagedResult<NoteDto>> GetPagedNotes(NoteQuery query)
        {
            var userId = GetUserId();
            var notes = await _notesRepository.GetAllAsync(
                n => n.UserId == userId && 
                (string.IsNullOrEmpty(query.SearchPhrase) || n.NoteName.ToLower().Contains(query.SearchPhrase) || n.Content.ToLower().Contains(query.SearchPhrase) ||
                n.Tags.Select(t => t.TagName.ToLower()).Any(t => t.Contains(query.SearchPhrase))), "Tags");
            
            await CheckAuthorization(notes);

            var pagedNotes = notes
                    .Skip(query.PageSize * (query.PageNumber - 1))
                    .Take(query.PageSize);
            var notesDto = _mapper.Map<IEnumerable<NoteDto>>(pagedNotes);
            var pagedResult = new PagedResult<NoteDto>(notesDto, notes.Count, query.PageSize, query.PageNumber);
            return pagedResult;
        }

        public async Task<PagedResult<NoteDto>> GetPagedNotes(NoteQuery query, Expression<Func<Note, bool>> predicate)
        {
            var notes = await _notesRepository.GetAllAsync(predicate, "Tags");
            await CheckAuthorization(notes);

            var pagedNotes = notes
                    .Skip(query.PageSize * (query.PageNumber - 1))
                    .Take(query.PageSize);

            var notesDto = _mapper.Map<IEnumerable<NoteDto>>(pagedNotes);
            var pagedResult = new PagedResult<NoteDto>(notesDto, notes.Count, query.PageSize, query.PageNumber);
            return pagedResult;
        }

        public async Task<PublicLinkDto> GeneratePublicLink(CreatePublicLinkDto dto, string hashId)
        {
            var id = GetRawId(hashId);
            var note = await _notesRepository.GetByIdAsync(id);
            await CheckAuthorization(note);

            if (note == null)
                throw new NotFoundException("Resource couldn't be found");
            
            note.PublicHashId = string.Empty;
            if (!dto.ResetPublicHashId)
            {
                var rng = new Random();
                var salt = rng.Next();
                var publicHashId = _hashids.EncodeLong(id + salt);

                note.PublicHashId = publicHashId;
                note.PublicHashIdSalt = salt;
                note.PublicLinkValidTill = DateTimeOffset.Now.AddDays(1);
            }

            await _notesRepository.UpdateAsync(note);
            return _mapper.Map<PublicLinkDto>(note);
        }

        public async Task<PublicNoteDto> GetPublicNote(string publicHashId)
        {
            var notes = await _notesRepository.GetAllNotesWithUsersAndTagsAsync(n => n.PublicHashId != string.Empty && n.PublicHashId == publicHashId);
            
            if (notes.Count == 1)
            {
                var note = notes.First();

                if(note.PublicLinkValidTill < DateTimeOffset.Now)
                    throw new NotFoundException("Resource couldn't be found");

                return _mapper.Map<PublicNoteDto>(note);
            }

            throw new NotFoundException("Resource couldn't be found");
        }

        public async Task<string> AddNote(CreateNoteDto noteDto)
        {
            var note = _mapper.Map<Note>(noteDto);
            var userId = GetUserId();
            note.UserId = userId;

            var noteNameValidation = await ValidateNoteUniqueness(noteDto.NoteName, "");
            if (!noteNameValidation)
                throw new BadRequestException("Note name should be unique");

            await _notesRepository.AddAsync(note);
            note.HashId = EncodeId(note.Id);
            await _notesRepository.UpdateAsync(note);
            return note.HashId;
        }

        public async Task<NoteDto> UpdateNote(UpdateNoteDto noteDto, string hashId)
        {
            var id = GetRawId(hashId);
            var note = await _notesRepository.GetByIdAsync(id, "Tags");
            await CheckAuthorization(note, Operation.Update);

            if (note == null)
                throw new NotFoundException("Resource couldn't be found");

            var noteNameValidation = await ValidateNoteUniqueness(noteDto.NoteName, hashId);
            if(!noteNameValidation)
                throw new BadRequestException("Note name should be unique");

            note.NoteName = noteDto.NoteName;
            note.Content = noteDto.Content;
            note.ImageLink = noteDto.ImageLink;
            note.Tags = _mapper.Map<ICollection<Tag>>(noteDto.Tags);

            await _notesRepository.UpdateAsync(note);
            return _mapper.Map<NoteDto>(note);
        }

        public async Task DeleteNote(string hashid)
        {
            int id = GetRawId(hashid);
            var note = await _notesRepository.GetByIdAsync(id);
            await CheckAuthorization(note, Operation.Delete);

            if (note == null)
                throw new NotFoundException("Resource couldn't be found");

            await _notesRepository.DeleteAsync(note);
        }

        private async Task CheckAuthorization(IEnumerable<Note> notes, Operation operation = Operation.Read)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(_userContextService.User, notes,
               new ResourceOperationRequirement(operation));

            if (!authorizationResult.Succeeded)
                throw new ForbiddenException("You don't have access to this resource");
        }

        private async Task CheckAuthorization(Note note, Operation operation = Operation.Read)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(_userContextService.User, note,
               new ResourceOperationRequirement(operation));

            if (!authorizationResult.Succeeded)
                throw new ForbiddenException("You don't have access to this resource");
        }

        private int GetUserId()
        {
            var userId = _userContextService.GetUserId;

            if (userId.HasValue)
                return userId.Value;

            throw new UnauthenticatedException("You are unauthenticated");
        }

        private int GetRawId(string hashId)
        {
            var rawId = _hashids.Decode(hashId);

            if (rawId.Length == 0)
                throw new NotFoundException("Invalid identifier");

            return rawId[0];
        }

        private string EncodeId(int id)
        {
            return _hashids.Encode(id);
        }

        private async Task<bool> ValidateNoteUniqueness(string name, string hashId)
        {
            var notes = await GetAllNotes();
            foreach (var note in notes)
            {
                if (note.NoteName == name && note.HashId != hashId)
                    return false;
            }

            return true;
        }
    }
}

﻿using Chirp.Core.DTOs;

namespace Chirp.Core.Interfaces;

public interface ICheepRepository
{
    Task<List<CheepDTO>> ReadCheepsFromAuthor(string userName, int page);
    Task<List<CheepDTO>> ReadAllCheeps(int page);
    Task CreateCheep(CheepDTO newCheep);
    Task UpdateCheep(CheepDTO alteredCheep);
    Task<int> GetTotalPages(string authorName);
    Task DeleteCheep(int cheepId);
    Task DeleteUserCheeps(AuthorDTO author);
    public Task<List<CheepDTO>> GetPopularCheeps(int page);
}
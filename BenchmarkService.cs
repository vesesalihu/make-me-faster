using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using Microsoft.EntityFrameworkCore;
using MakeMeFaster.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MakeMeFaster
{
    [MemoryDiagnoser]
    [InProcess]
    //[HideColumns(BenchmarkDotNet.Columns.Column.Job, BenchmarkDotNet.Columns.Column.RatioSD, BenchmarkDotNet.Columns.Column.StdDev, BenchmarkDotNet.Columns.Column.AllocRatio)]
    //[Config(typeof(Config))]
    public class BenchmarkService
    {
        public BenchmarkService()
        {
        }

        //private class Config : ManualConfig
        //{
        //    public Config()
        //    {
        //        SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
        //    }
        //}

        /// <summary>
        /// Get top 2 Authors (FirstName, LastName, UserName, Email, Age, Country) 
        /// from country Serbia aged 27, with the highest BooksCount
        /// and all his/her books (Book Name/Title and Publishment Year) published before 1900
        /// </summary>
        /// <returns></returns>
        // [Benchmark]
        public List<AuthorDTO> GetAuthors()
        {
            using var dbContext = new AppDbContext();

            var authors = dbContext.Authors
                                        .Include(x => x.User)
                                        .ThenInclude(x => x.UserRoles)
                                        .ThenInclude(x => x.Role)
                                        .Include(x => x.Books)
                                        .ThenInclude(x => x.Publisher)
                                        .ToList()
                                        .Select(x => new AuthorDTO
                                        {
                                            UserCreated = x.User.Created,
                                            UserEmailConfirmed = x.User.EmailConfirmed,
                                            UserFirstName = x.User.FirstName,
                                            UserLastActivity = x.User.LastActivity,
                                            UserLastName = x.User.LastName,
                                            UserEmail = x.User.Email,
                                            UserName = x.User.UserName,
                                            UserId = x.User.Id,
                                            RoleId = x.User.UserRoles.FirstOrDefault(y => y.UserId == x.UserId).RoleId,
                                            BooksCount = x.BooksCount,
                                            AllBooks = x.Books.Select(y => new BookDto
                                            {
                                                Id = y.Id,
                                                Name = y.Name,
                                                Published = y.Published,
                                                ISBN = y.ISBN,
                                                PublisherName = y.Publisher.Name
                                            }).ToList(),
                                            AuthorAge = x.Age,
                                            AuthorCountry = x.Country,
                                            AuthorNickName = x.NickName,
                                            Id = x.Id
                                        })
                                        .ToList()
                                        .Where(x => x.AuthorCountry == "Serbia" && x.AuthorAge == 27)
                                        .ToList();

            var orderedAuthors = authors.OrderByDescending(x => x.BooksCount).ToList().Take(2).ToList();

            List<AuthorDTO> finalAuthors = new List<AuthorDTO>();
            foreach (var author in orderedAuthors)
            {
                List<BookDto> books = new List<BookDto>();

                var allBooks = author.AllBooks;

                foreach (var book in allBooks)
                {
                    if (book.Published.Year < 1900)
                    {
                        book.PublishedYear = book.Published.Year;
                        books.Add(book);
                    }
                }

                author.AllBooks = books;
                finalAuthors.Add(author);
            }

            return finalAuthors;
        }

        [Benchmark]
        public List<AuthorDTO_Optimized> GetAuthors_Optimized1()
        {

            // Include your optimization below, with comments explaining your thought process
            using var dbContext = new AppDbContext();


            List<AuthorDTO_Optimized> authors = new List<AuthorDTO_Optimized>();

            var authors1 = (from a in dbContext.Authors.AsEnumerable()
                            join us in dbContext.Users.AsEnumerable()
                            on a.UserId equals us.Id
                            join b in dbContext.Books.AsEnumerable()
                            on a.Id equals b.AuthorId
                            where a.Country == "Serbia" && a.Age == 27 && b.Published.Year < 1900
                            orderby a.BooksCount descending
                            select new AuthorDTO_Optimized
                            {
                                FirstName = us.FirstName,
                                LastName = us.LastName,
                                Age = a.Age,
                                Books = a.Books.Select(y => new BookDTO_Optimized
                                {
                                    Published = y.Published,
                                    PublishedYear = y.Published.Year,
                                    Title = y.Name
                                }),
                                Country = a.Country,
                                Email = us.Email,
                                UserName = us.UserName
                            }).Take(2).AsEnumerable();
          
            authors = authors1.ToList<AuthorDTO_Optimized>();

            return authors;

        }


    }
}
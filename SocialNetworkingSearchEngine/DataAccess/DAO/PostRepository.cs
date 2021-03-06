﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;
using Core.RepositoryInterfaces;
using NHibernate.Criterion;

namespace DataAccess.DAO
{
    public class PostRepository : RepositoryBase<Post, Guid>, IPostRepository
    {
        public List<Post> GetAllByTagName(string tagName)
        {
            return Session.QueryOver<Post>().Left.JoinQueryOver<Tag>(x => x.Tags).Where(x => x.Name == tagName).List<Post>() as List<Post>;
        }

        public List<Post> GetAllByProfile(Guid profileId)
        {
            return Session.QueryOver<Post>().Left.JoinQueryOver<Profile>(x => x.Profile).Where(x => x.Id == profileId).List<Post>() as List<Post>;
        }

        public List<Post> GetAllByTagNameId(Guid tagId)
        {
            return Session.QueryOver<Post>().Left.JoinQueryOver<Tag>(x => x.Tags).Where(x => x.Id == tagId).List<Post>() as List<Post>;
        }

        public List<Post> GetByQuery(string query)
        {
            query = !String.IsNullOrEmpty(query) ? "%" + query + "%" : query;
            return Session.QueryOver<Post>().Where(x => x.Content.IsInsensitiveLike(query) || x.Query.IsInsensitiveLike(query)).List<Post>() as List<Post>;
        }

        public IEnumerable<Post> GetByAssignedUser(User assignedUser)
        {
            var posts = Session.QueryOver<Post>().Where(x => x.CurrentOwner.Id == assignedUser.Id).List<Post>();
            return posts;
        }

        public IEnumerable<Post> GetNotProcessedByAssignedUser(User assignedUser)
        {
            var posts = Session.QueryOver<Post>().Where(x => x.CurrentOwner.Id == assignedUser.Id &&
                x.Calification <= 0).List();

            return posts.Where(x=> x.PostTags.Count == 0);
        }

        public IEnumerable<Post> GetNotAssigned(int cant)
        {
            var posts = Session.QueryOver<Post>().Where(x => x.CurrentOwner == null).Take(cant).List<Post>();
            return posts;
        }

        public bool ExistPost(string postUrl)
        {
            var posts = Session.QueryOver<Post>().Where(x => x.UrlPost.IsInsensitiveLike(postUrl)).List<Post>();
            return (posts.Count > 0);
        }
    }
}

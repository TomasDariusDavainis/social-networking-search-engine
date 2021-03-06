﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEnginesBase.Utils;
using SearchEnginesBase.Entities;

namespace GooglePlusSearchEngine
{
    public class GooglePlusSearchEngine : SearchEnginesBase.Interfaces.ISearchEngine
    {
        const string _apiKey = "AIzaSyAoWGAxAZ8LkAX8B5FG0wVBGBBUg6GXgVo";
        const int _maxResultPerPage = 100;

        private StringBuilder _queryParams;

        public string Name
        {
            get { return "Google +"; }
        }

        public SocialNetworkingSearchResult Search(string searchParameters, int page)
        {
            if (_queryParams == null) _queryParams = new StringBuilder();
            _queryParams.Append(GetParameters(20, string.Empty, string.Empty, 0, _apiKey));

            var list = GetResultItems(searchParameters);
            _queryParams.Clear();
            return new SocialNetworkingSearchResult() { SocialNetworkingItems = list, SocialNetworkingName = Name };
        }

        public List<SocialNetworkingItem> GetResultItems(string searchParameters)
        {
            var engineURL = GetEngineUrl();
            var list = new List<SocialNetworkingItem>();
            var nextResultTokenParam = string.Empty;
            for (int i = 0; i < _maxResultPerPage/20; i++)
            {
                var jsonResults = Utils.BuildSearchQuery(engineURL, searchParameters, _queryParams + nextResultTokenParam);
                var entity = Utils.DeserializarJsonTo<SearchResultsGooglePlus>(jsonResults);
                list.AddRange(SocialNetworkingItemList(entity));
                nextResultTokenParam = "&pageToken=" + entity.NextPageToken;
            }
            return list;
        }

        public SocialNetworkingSearchResult Search(string searchParameters, int page, string location, string language = null)
        {
            _queryParams = new StringBuilder();
            _queryParams.Append(GetLocationParam(location));
            _queryParams.Append(GetLenguaje(language));
            return Search(searchParameters, page);
        }

        public List<string> CountriesToFilterISOCodes
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        private string GetEngineUrl()
        {
            return "https://www.googleapis.com/plus/v1/activities?query=";
        }

        /// <summary>
        /// Metodo que arma los parametros para la consulta a la API REST de Google Plus.
        /// </summary>
        /// <param name="maximosResultados">Cantidad maxima de resultados por Response, sirve para paginar (valor entre 1 y 20).</param>
        /// <param name="ordenarPor">Especifica como ordenar los resultados de la busqueda. Los valores pueden ser "best" o "recent".</param>
        /// <param name="pageToken">Para obtener la siguiente pagina de resultados. El valor es "nextPageToken".</param>
        /// <param name="pp">No se que es, pero siempre es igual a 1</param>
        /// <param name="apiKey">Clave de la aplicacion para poder usar Google Plus</param>
        /// <returns></returns>
        private string GetParameters(int maximosResultados, string ordenarPor, string pageToken, int pp, string apiKey)
        {
            var _maxResults = maximosResultados == 0 ? String.Empty : "&maxResults=" + maximosResultados;
            var _orderBy = String.IsNullOrEmpty(ordenarPor) ? String.Empty : "&orderBy=" + ordenarPor;
            var _pageToken = String.IsNullOrEmpty(pageToken) ? String.Empty : "&pageToken=" + pageToken;
            var _pp = pp == 0 ? "&pp=1" : "&pp=" + pp;
            var _key = "&key=" + apiKey;

            return  _maxResults + _orderBy + _pageToken + _pp + _key;
        }

        private string GetLenguaje(string lenguaje)
        {
            if (string.IsNullOrWhiteSpace(lenguaje) || lenguaje.Length > 2)
            {
                throw new ArgumentException("lang no es un ISO 639-1 valido");
            }

            return "&language=" + lenguaje;
        }

        /// <summary>
        /// Arma el parametro para buscar por locacion en la API REST.
        /// </summary>
        /// <param name="location">location en ISO 3166-1 country codes</param>
        /// <returns></returns>
        private string GetLocationParam(string location)
        {
            //Retorna Empty porque hasta el momento google plus no soporta search por location
            return string.Empty;
        }

        //Este metodo itera los resultados y crea las entidades de dominio
        private List<SocialNetworkingItem> SocialNetworkingItemList(SearchResultsGooglePlus entity)
        {
            if (entity == null)
                return null;

            List<SocialNetworkingItem> users = (from u in entity.Results
                                                select new SocialNetworkingItem
                                                {
                                                    SocialNetworkName = Name,
                                                    UserName = u.Actor.FromUser,
                                                    ProfileImage = u.Actor.Imagen.ProfileImageUrl,
                                                    Content = u.Objeto.Text,
                                                    UrlPost = u.Objeto.UrlPost,
                                                    UrlProfile = u.Actor.UserProfile,
                                                    CreatedAt = DateTimeOffset.Parse(u.CreatedAt, System.Globalization.CultureInfo.GetCultureInfo("en-US")).UtcDateTime,
                                                    Source = string.Empty
                                                }).ToList();
            return users;
        }
    }
}

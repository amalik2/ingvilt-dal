using System;
using System.Collections.Generic;
using System.Linq;

namespace Ingvilt.Models.DataAccess.Search {
    public class SearchParser {
        private static readonly char DELIMITER = ':';
        private static readonly int MAX_QUERY_COMPONENTS = 50;

        public static List<ISearchQueryGenerator> GetAllVideoDecorators() {
            var arrayValue = "[\"1\", \"2\"]";
            var intValue = "3";
            var list = new List<ISearchQueryGenerator> {
                new CharacterInVideoSearchQueryGenerator(arrayValue),
                new CharacterNotInVideoSearchQueryGenerator(arrayValue),
                new OneOfCharactersInVideoSearchQueryGenerator(arrayValue),
                new NoneOfCharactersInQueryGenerator(arrayValue),
                new VideoWithTagsQueryGenerator(arrayValue),
                new VideoWithoutTagsQueryGenerator(arrayValue),
                new CharactersWithTagsInVideoQueryGenerator(arrayValue),
                new VideoTimesWatchedGreaterThanQueryGenerator(intValue),
                new VideoTimesWatchedLessThanQueryGenerator(intValue),
                new VideoUserRatingGreaterThanQueryGenerator(intValue),
                new VideoUserRatingLessThanQueryGenerator(intValue),
                new VideoExternalRatingGreaterThanQueryGenerator(intValue),
                new VideoExternalRatingLessThanQueryGenerator(intValue),
                new VideoTitleQueryGenerator(arrayValue),
                new VideoSeriesQueryGenerator(arrayValue),
                new VideoDescriptionQueryGenerator(arrayValue),
                new CreatorInVideoQueryGenerator(arrayValue),
                new VideoAtLocationsQueryGenerator(arrayValue),
                new VideoNotAtLocationsQueryGenerator(arrayValue),
                new VideoWithCreatorQueryGenerator(arrayValue)
            };

            return list.OrderBy(g => g.GetName()).ToList();
        }

        public static List<ISearchQueryGenerator> GetAllCharacterDecorators() {
            var arrayValue = "[\"test\"]";
            var intValue = "3";
            var list = new List<ISearchQueryGenerator> {
                new CharacterNameQueryGenerator(arrayValue),
                new CharacterWithoutNameQueryGenerator(arrayValue),
                new CharacterWithTagsQueryGenerator(arrayValue),
                new CharacterWithoutTagsQueryGenerator(arrayValue),
                new CharacterRatingGreaterThanQueryGenerator(intValue),
                new CharacterRatingLessThanQueryGenerator(intValue),
                new CharacterDescriptionQueryGenerator(arrayValue),
                new CharacterAgeGreaterThanQueryGenerator(intValue),
                new CharacterAgeLessThanQueryGenerator(intValue)
            };

            return list.OrderBy(g => g.GetName()).ToList();
        }

        public static List<ISearchQueryGenerator> GetAllFileDecorators() {
            var arrayValue = "[\"test\"]";
            var list = new List<ISearchQueryGenerator> {
                new MediaFileWithCharacterNameGenerator(arrayValue),
                new MediaFileWithTagsGenerator(arrayValue),
                new MediaFileWithLocationsGenerator(arrayValue),
                new MediaFileWithNameGenerator(arrayValue)
            };

            return list.OrderBy(g => g.GetName()).ToList();
        }

        public static List<ISearchQueryGenerator> GetAllSeriesDecorators() {
            var arrayValue = "[\"test\"]";
            var list = new List<ISearchQueryGenerator> {
                new SeriesNameQueryGenerator(arrayValue),
                new SeriesWithTagsGenerator(arrayValue)
            };

            return list.OrderBy(g => g.GetName()).ToList();
        }

        protected virtual ISearchQueryGenerator GetDecorator(string name, string value) {
            try {
                // videos
                if (name == CharacterInVideoSearchQueryGenerator.GENERATOR_NAME) {
                    return new CharacterInVideoSearchQueryGenerator(value);
                } else if (name == CharacterNotInVideoSearchQueryGenerator.GENERATOR_NAME) {
                    return new CharacterNotInVideoSearchQueryGenerator(value);
                } else if (name == OneOfCharactersInVideoSearchQueryGenerator.GENERATOR_NAME) {
                    return new OneOfCharactersInVideoSearchQueryGenerator(value);
                } else if (name == NoneOfCharactersInQueryGenerator.GENERATOR_NAME) {
                    return new NoneOfCharactersInQueryGenerator(value);
                } else if (name == VideoWithTagsQueryGenerator.GENERATOR_NAME) {
                    return new VideoWithTagsQueryGenerator(value);
                } else if (name == VideoWithoutTagsQueryGenerator.GENERATOR_NAME) {
                    return new VideoWithoutTagsQueryGenerator(value);
                } else if (name == CharactersWithTagsInVideoQueryGenerator.GENERATOR_NAME) {
                    return new CharactersWithTagsInVideoQueryGenerator(value);
                } else if (name == VideoTimesWatchedGreaterThanQueryGenerator.GENERATOR_NAME) {
                    return new VideoTimesWatchedGreaterThanQueryGenerator(value);
                } else if (name == VideoTimesWatchedLessThanQueryGenerator.GENERATOR_NAME) {
                    return new VideoTimesWatchedLessThanQueryGenerator(value);
                } else if (name == VideoUserRatingGreaterThanQueryGenerator.GENERATOR_NAME) {
                    return new VideoUserRatingGreaterThanQueryGenerator(value);
                } else if (name == VideoUserRatingLessThanQueryGenerator.GENERATOR_NAME) {
                    return new VideoUserRatingLessThanQueryGenerator(value);
                } else if (name == VideoExternalRatingGreaterThanQueryGenerator.GENERATOR_NAME) {
                    return new VideoExternalRatingGreaterThanQueryGenerator(value);
                } else if (name == VideoExternalRatingLessThanQueryGenerator.GENERATOR_NAME) {
                    return new VideoExternalRatingLessThanQueryGenerator(value);
                } else if (name == VideoTitleQueryGenerator.GENERATOR_NAME) {
                    return new VideoTitleQueryGenerator(value);
                } else if (name == VideoSeriesQueryGenerator.GENERATOR_NAME) {
                    return new VideoSeriesQueryGenerator(value);
                } else if (name == VideoDescriptionQueryGenerator.GENERATOR_NAME) {
                    return new VideoDescriptionQueryGenerator(value);
                } else if (name == CreatorInVideoQueryGenerator.GENERATOR_NAME) {
                    return new CreatorInVideoQueryGenerator(value);
                } else if (name == VideoAtLocationsQueryGenerator.GENERATOR_NAME) {
                    return new VideoAtLocationsQueryGenerator(value);
                } else if (name == VideoNotAtLocationsQueryGenerator.GENERATOR_NAME) {
                    return new VideoNotAtLocationsQueryGenerator(value);
                } else if (name == VideoWithCreatorQueryGenerator.GENERATOR_NAME) {
                    return new VideoWithCreatorQueryGenerator(value);
                }

                // characters
                if (name == CharacterNameQueryGenerator.GENERATOR_NAME) {
                    return new CharacterNameQueryGenerator(value);
                } else if (name == CharacterWithoutNameQueryGenerator.GENERATOR_NAME) {
                    return new CharacterWithoutNameQueryGenerator(value);
                } else if (name == CharacterWithTagsQueryGenerator.GENERATOR_NAME) {
                    return new CharacterWithTagsQueryGenerator(value);
                } else if (name == CharacterWithoutTagsQueryGenerator.GENERATOR_NAME) {
                    return new CharacterWithoutTagsQueryGenerator(value);
                } else if (name == CharacterRatingGreaterThanQueryGenerator.GENERATOR_NAME) {
                    return new CharacterRatingGreaterThanQueryGenerator(value);
                } else if (name == CharacterRatingLessThanQueryGenerator.GENERATOR_NAME) {
                    return new CharacterRatingLessThanQueryGenerator(value);
                } else if (name == CharacterDescriptionQueryGenerator.GENERATOR_NAME) {
                    return new CharacterDescriptionQueryGenerator(value);
                } else if (name == CharacterAgeGreaterThanQueryGenerator.GENERATOR_NAME) {
                    return new CharacterAgeGreaterThanQueryGenerator(value);
                } else if (name == CharacterAgeLessThanQueryGenerator.GENERATOR_NAME) {
                    return new CharacterAgeLessThanQueryGenerator(value);
                }

                // files
                if (name == MediaFileWithCharacterNameGenerator.GENERATOR_NAME) {
                    return new MediaFileWithCharacterNameGenerator(value);
                } else if (name == MediaFileWithTagsGenerator.GENERATOR_NAME) {
                    return new MediaFileWithTagsGenerator(value);
                } else if (name == MediaFileWithLocationsGenerator.GENERATOR_NAME) {
                    return new MediaFileWithLocationsGenerator(value);
                } else if (name == MediaFileWithNameGenerator.GENERATOR_NAME) {
                    return new MediaFileWithNameGenerator(value);
                }

                // series
                if (name == SeriesNameQueryGenerator.GENERATOR_NAME) {
                    return new SeriesNameQueryGenerator(value);
                } else if (name == SeriesWithTagsGenerator.GENERATOR_NAME) {
                    return new SeriesWithTagsGenerator(value);
                }

                return null;
            } catch (Exception) {
                return null;
            }
        }

        public List<ISearchQueryGenerator> ParseSearchQuery(string query) {
            var list = new List<ISearchQueryGenerator>();
            var name = "";
            var value = "";
            var parsingValue = false;

            Action clearCurrentAttribute = () => {
                name = "";
                value = "";
                parsingValue = false;
            };

            Action addDecoratorForAttribute = () => {
                value = value.Replace("\\'", "'").Substring(1);
                var decorator = GetDecorator(name, value);
                if (decorator != null) {
                    list.Add(decorator);
                }
            };

            foreach (var c in query) {
                if (c == DELIMITER && !parsingValue) {
                    parsingValue = true;
                } else if (parsingValue) {
                    var end = c == '\'' && value.Length > 0 && value.Last() != '\\';
                    if (end) {
                        addDecoratorForAttribute();
                        if (list.Count == MAX_QUERY_COMPONENTS) {
                            break;
                        }

                        clearCurrentAttribute();
                    } else {
                        value += c;
                    }
                } else {
                    if (c == ' ') {
                        clearCurrentAttribute();
                        continue;
                    }

                    name += c;
                }
            }

            return list;
        }

        public string ConvertGeneratorsToRawQuery(List<ISearchQueryGenerator> generators) {
            var rawSearchQuery = "";
            foreach (var generator in generators) {
                rawSearchQuery += generator.GetName() + DELIMITER + "'" + generator.GetValueAsString() + "' ";
            }

            return rawSearchQuery;
        }
    }
}
